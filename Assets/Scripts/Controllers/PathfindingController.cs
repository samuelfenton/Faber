using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingController : MonoBehaviour
{
    public class PathfindingNode
    {
        public float m_cost;
        public Navigation_Spline m_navigationBranch;
        public Navigation_Trigger m_trigger;
        public PathfindingNode m_previousNode;

        public PathfindingNode(float p_cost, Navigation_Spline p_navigationBranch, Navigation_Trigger p_trigger, PathfindingNode p_previousNode)
        {
            m_cost = p_cost;
            m_navigationBranch = p_navigationBranch;
            m_trigger = p_trigger;
            m_previousNode = p_previousNode;
        }
    }

    public List<Navigation_Spline> GetPath(Character p_character, Navigation_Spline p_goalSpline)
    {
        List<Navigation_Spline> path = new List<Navigation_Spline>();

        Navigation_Spline currentSpline = p_character.m_characterCustomPhysics.m_currentSpline;

        //Valid inputs
        if (currentSpline == null || p_goalSpline == null || currentSpline == p_goalSpline)
            return path;

        Navigation_Trigger startingTrigger = GetStartingTrigger(p_character.m_characterCustomPhysics.m_currentSpline, p_character.m_characterCustomPhysics.m_currentSplinePercent);

        List<PathfindingNode> openNodes = new List<PathfindingNode>();
        List<PathfindingNode> closedNodes = new List<PathfindingNode>();

        PathfindingNode currentNode = new PathfindingNode(0.0f, null, startingTrigger, null);

        AddToOpenList(currentNode, openNodes, closedNodes);

        //Dijkstra pathfinding
        while (openNodes.Count > 0 || currentNode.m_navigationBranch == p_goalSpline)
        {
            closedNodes.Add(currentNode);
            currentNode = GetCheapestNode(openNodes);
            openNodes.Remove(currentNode);

            AddToOpenList(currentNode, openNodes, closedNodes);
        }

        //Build path
        while(currentNode.m_previousNode != null)
        {
            path.Add(currentNode.m_navigationBranch);
            currentNode = currentNode.m_previousNode;
        }

        return path;
    }

    private Navigation_Trigger GetStartingTrigger(Navigation_Spline p_currentSpline, float p_currentPercent)
    {
        if (p_currentPercent <= 0.5f)
            return p_currentSpline.m_splineStart;
        return p_currentSpline.m_splineEnd;
    }

    private void AddToOpenList(PathfindingNode p_currentNode, List<PathfindingNode> p_openList, List<PathfindingNode> p_closedList)
    {
        foreach (Navigation_Spline adjacentSpline in p_currentNode.m_trigger.m_adjacentSplines)
        {
            if(!(ListContainsSpline(p_openList, adjacentSpline) || ListContainsSpline(p_closedList, adjacentSpline)))//Is spline already in the list
            {
                //Get other trigger attached to spline
                Navigation_Trigger newTrigger = adjacentSpline.m_splineStart == p_currentNode.m_trigger ? adjacentSpline.m_splineEnd : adjacentSpline.m_splineStart;
                p_openList.Add(new PathfindingNode(p_currentNode.m_cost + adjacentSpline.m_splineLength, adjacentSpline, newTrigger, p_currentNode));
            }
        }
    }

    private bool ListContainsSpline(List<PathfindingNode> p_searchList, Navigation_Spline p_spline)
    {
        foreach (PathfindingNode pathfindingNode in p_searchList)
        {
            if (pathfindingNode.m_navigationBranch == p_spline)
                return true;
        }
        return false;
    }

    private PathfindingNode GetCheapestNode(List<PathfindingNode> p_openList)
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
