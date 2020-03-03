using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingController : MonoBehaviour
{
    public class PathfindingNode
    {
        public float m_cost;
        public Pathing_Spline m_nodeSpline;
        public PathfindingNode m_previousNode;

        public PathfindingNode(float p_cost, Pathing_Spline p_nodeSpline, PathfindingNode p_previousNode)
        {
            m_cost = p_cost;
            m_nodeSpline = p_nodeSpline;
            m_previousNode = p_previousNode;
        }
    }


    public static List<Pathing_Spline> GetPath(Character p_character, Pathing_Spline p_goalSpline)
    {
        List<Pathing_Spline> path = new List<Pathing_Spline>();

        Pathing_Spline currentSpline = p_character.m_splinePhysics.m_currentSpline;

        //Valid inputs
        if (currentSpline == null || p_goalSpline == null || currentSpline == p_goalSpline)
            return path;

        List<PathfindingNode> openNodes = new List<PathfindingNode>();
        List<PathfindingNode> closedNodes = new List<PathfindingNode>();

        PathfindingNode currentNode = new PathfindingNode(0.0f, currentSpline, null);

        closedNodes.Add(currentNode);
        AddToOpenList(currentNode, openNodes, closedNodes);

        //Dijkstra pathfinding
        while (openNodes.Count > 0 && currentNode.m_nodeSpline != p_goalSpline)
        {
            currentNode = GetCheapestNode(openNodes);
            closedNodes.Add(currentNode);
            openNodes.Remove(currentNode);

            AddToOpenList(currentNode, openNodes, closedNodes);
        }

        if(currentNode.m_nodeSpline == p_goalSpline)
        {
            //Build path
            while(currentNode.m_previousNode != null)
            {
                path.Add(currentNode.m_nodeSpline);
                currentNode = currentNode.m_previousNode; 
            }
        }
        path.Reverse();
        return path;
    }

    private static Pathing_Node GetStartingTrigger(Pathing_Spline p_currentSpline, float p_currentPercent)
    {
        if (p_currentPercent <= 0.5f)
            return p_currentSpline.m_nodeA;
        return p_currentSpline.m_nodeB;
    }

    private static void AddToOpenList(PathfindingNode p_currentNode, List<PathfindingNode> p_openList, List<PathfindingNode> p_closedList)
    {
        HashSet<Pathing_Spline> adjacentSplines = new HashSet<Pathing_Spline>();//Unique list

        if (p_currentNode.m_nodeSpline.m_nodeA != null)//add start of spline adjacent splines if start exists
        { 
            foreach (Pathing_Spline adjacentSpline in p_currentNode.m_nodeSpline.m_nodeA.m_adjacentSplines) 
            {
                adjacentSplines.Add(adjacentSpline);
            }
            foreach (Pathing_Spline adjacentSpline in p_currentNode.m_nodeSpline.m_nodeA.m_adjacentSplines)
            {
                adjacentSplines.Add(adjacentSpline);
            }
        }

        foreach (Pathing_Spline adjacentSpline in adjacentSplines)
        {
            if(!(ListContainsSpline(p_openList, adjacentSpline) || ListContainsSpline(p_closedList, adjacentSpline)))//Is spline already in the list
            {
                //Get other trigger attached to spline
                p_openList.Add(new PathfindingNode(p_currentNode.m_cost + adjacentSpline.m_splineLength, adjacentSpline, p_currentNode));
            }
        }
    }

    private static bool ListContainsSpline(List<PathfindingNode> p_searchList, Pathing_Spline p_spline)
    {
        foreach (PathfindingNode pathfindingNode in p_searchList)
        {
            if (pathfindingNode.m_nodeSpline == p_spline)
                return true;
        }
        return false;
    }

    private static PathfindingNode GetCheapestNode(List<PathfindingNode> p_openList)
    {
        float lowestCost = float.PositiveInfinity;
        PathfindingNode cheapestNode = null;

        foreach (PathfindingNode pathfindingNode in p_openList)
        {
            if (pathfindingNode.m_cost < lowestCost)
            {
                lowestCost = pathfindingNode.m_cost;
                cheapestNode = pathfindingNode;
            }
        }

        return cheapestNode;
    }
}
