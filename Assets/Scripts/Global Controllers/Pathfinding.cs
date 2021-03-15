using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
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

    /// <summary>
    /// Determine if a path is valid, and if still setup correctly based off enetity movements
    /// </summary>
    /// <param name="p_currentPath">Current path that needs to be checked</param>
    /// <param name="p_currentEntity">Current entity pathfinding</param>
    /// <param name="p_currentTarget">Target Entity</param>
    /// <returns>True when valid path</returns>
    public static bool ValidPath(List<Pathing_Spline> p_currentPath, Entity p_currentEntity, Entity p_currentTarget)
    {
        if (p_currentPath.Count == 0) //Empty path
            return false;

        if (p_currentPath[p_currentPath.Count - 1] != p_currentTarget.m_splinePhysics.m_currentSpline)// Target has moved away
            return false;

        // Spline[0] is not current spline, but next desired spline
        Pathing_Node nextNode = GetConnectingNode(p_currentEntity.m_splinePhysics.m_currentSpline, p_currentPath[0]);
        if (nextNode == null)// No avalible connecting nodes
            return false;

        return true;
    }

    /// <summary>
    /// Given two entities, determine best estimation of distance between
    /// -Same Spline: Get percentage difference, get spline lenght based off diference
    /// -Connected Splines: Get distance to the node on both ends
    /// -Non-connected : Do basic distance math
    /// </summary>
    /// <param name="p_entityLHS">First entity</param>
    /// <param name="p_entityRHS">Second enity</param>
    /// <returns>Best approx distance using above rules, Infinity if invalid data</returns>
    public static float GetDistance(Entity p_entityLHS, Entity p_entityRHS)
    {
        if (p_entityLHS == null || p_entityRHS == null) //Invalid data
            return float.MaxValue;

        Pathing_Spline LHSSpline = p_entityLHS.m_splinePhysics.m_currentSpline;
        Pathing_Spline RHSSpline = p_entityRHS.m_splinePhysics.m_currentSpline;

        float LHSSplinePercentage = p_entityLHS.m_splinePhysics.m_currentSplinePercent;
        float RHSSplinePercentage = p_entityRHS.m_splinePhysics.m_currentSplinePercent;

        if (LHSSpline == RHSSpline)//Same spline, best case
        {
            float percentageDiff = Mathf.Abs(LHSSplinePercentage - RHSSplinePercentage);
            return LHSSpline.m_splineLength * percentageDiff;
        }
        else
        {
            Pathing_Node connnectedNode = GetConnectingNode(LHSSpline, RHSSpline);

            if(connnectedNode!= null) //Connected Nodes, best case
            {
                float LHSToNodeDistance = Mathf.Abs(LHSSpline.GetPercentForNode(connnectedNode) - LHSSplinePercentage) * LHSSpline.m_splineLength;
                float RHSToNodeDistance = Mathf.Abs(RHSSpline.GetPercentForNode(connnectedNode) - RHSSplinePercentage) * RHSSpline.m_splineLength;

                return LHSToNodeDistance + RHSToNodeDistance;
            }
            else //No connections, worse case, estimate will be wrong
            {
                return (p_entityLHS.transform.position - p_entityRHS.transform.position).magnitude;
            }
        }
    }

    /// <summary>
    /// Get the node connecting two splines
    /// </summary>
    /// <param name="p_splineLHS">First Spline</param>
    /// <param name="p_splineRHS">Second Spline</param>
    /// <returns>Connecting node, null if invalid or no connection</returns>
    public static Pathing_Node GetConnectingNode(Pathing_Spline p_splineLHS, Pathing_Spline p_splineRHS)
    {
        if (p_splineLHS == null || p_splineRHS == null)
            return null;

        if (p_splineLHS.m_nodePrimary.ContainsSpline(p_splineRHS))
        {
            return p_splineLHS.m_nodePrimary;
        }
        else if (p_splineLHS.m_nodeSecondary.ContainsSpline(p_splineRHS))
        {
            return p_splineLHS.m_nodeSecondary;
        }

        return null;
    }

    /// <summary>
    /// Build path using basic a* navigation
    /// Nodes are built form the splines themselves
    /// Does not include current spline, rather next spline to move towards is List[0], final spline is current spline of target
    /// </summary>
    /// <param name="p_character">Character looking for path</param>
    /// <param name="p_goalSpline"></param>
    /// <returns>path to goal, may be empty if invalid data provided, already there.</returns>
    public static List<Pathing_Spline> GetPath(Entity p_character, Pathing_Spline p_goalSpline)
    {
        List<Pathing_Spline> path = new List<Pathing_Spline>();

        if (p_character == null)
            return path;

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
            //Build path - Doesnt include start spline, does include goal spline
            while(currentNode.m_previousNode != null)
            {
                path.Add(currentNode.m_nodeSpline);
                currentNode = currentNode.m_previousNode; 
            }
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// Add new "node" that is spline and its cost, include all the adjacent nodes
    /// </summary>
    /// <param name="p_currentNode">Current node in use</param>
    /// <param name="p_openList">All possible open nodes</param>
    /// <param name="p_closedList">All possible closed nodes</param>
    private static void AddToOpenList(PathfindingNode p_currentNode, List<PathfindingNode> p_openList, List<PathfindingNode> p_closedList)
    {
        HashSet<Pathing_Spline> adjacentSplines = new HashSet<Pathing_Spline>();//Unique list

        if (p_currentNode.m_nodeSpline.m_nodePrimary != null)//add start of spline adjacent splines if start exists
        {
            foreach (Pathing_Spline adjacentSpline in p_currentNode.m_nodeSpline.m_nodePrimary.m_adjacentSplines)
            {
                adjacentSplines.Add(adjacentSpline);
            }
        }

        if (p_currentNode.m_nodeSpline.m_nodeSecondary != null) //add end of spline adjacent splines if start exists
        {
            foreach (Pathing_Spline adjacentSpline in p_currentNode.m_nodeSpline.m_nodeSecondary.m_adjacentSplines)
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

    /// <summary>
    /// Does this list contain the spline
    /// </summary>
    /// <param name="p_searchList">List to check against</param>
    /// <param name="p_spline">spline looking for</param>
    /// <returns>true when spline is found</returns>
    private static bool ListContainsSpline(List<PathfindingNode> p_searchList, Pathing_Spline p_spline)
    {
        foreach (PathfindingNode pathfindingNode in p_searchList)
        {
            if (pathfindingNode.m_nodeSpline == p_spline)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Get the cheapest next node
    /// </summary>
    /// <param name="p_openList">All nodes set to open</param>
    /// <returns>Cheapest node, null when none avalible</returns>
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
