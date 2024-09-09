using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MOARDebugging
{
    public static Color SAVE_POINT_COLOR = Color.blue;
    public static Color PATROL_COLOR = Color.red;
    private const float DEBUG_FLAG_HEIGHT = 3.0f;

    /// <summary>
    /// Given two nodes determine the position on a spline when valid
    /// </summary>
    /// <param name="p_nodeA">Node A</param>
    /// <param name="p_nodeB">Node B</param>
    /// <param name="p_percent">Percent on spline</param>
    /// <param name="p_position">out: the position when calculated</param>
    /// <returns>true when able to get posiiton on a spline</returns>
    public static bool GetSplinePosition(Pathing_Node p_nodeA, Pathing_Node p_nodeB, float p_percent, out Vector3 p_position)
    {
        p_position = Vector3.zero;

        if (p_nodeA != null && p_nodeB != null) //Valid setup
        {
            if (p_nodeA.ContainsConjoinedNode(p_nodeB))
            {
                Pathing_Spline.SPLINE_POSITION splinePosition = p_nodeA.DetermineNodePosition(p_nodeB);

                if (splinePosition != Pathing_Spline.SPLINE_POSITION.MAX_LENGTH)//Valid spline setup
                {
                    Pathing_Node.Spline_Details splineDetails = p_nodeA.m_pathingSplineDetails[(int)splinePosition];

                    //Setup spline variables
                    float length = 0.0f;

                    //Straigh
                    Vector3 straightDir = Vector3.zero;

                    //Bezier
                    Vector3 bezierPointA = Vector3.zero;
                    Vector3 bezierPointB = Vector3.zero;
                    Vector3 bezierControlA = Vector3.zero;
                    Vector3 bezierControlB = Vector3.zero;

                    //Circle
                    float circleHeight = 0.0f;
                    Vector3 circleCenter = Vector3.zero;
                    Vector3 centerADir = Vector3.zero;

                    switch (splineDetails.m_splineType)
                    {
                        case Pathing_Spline.SPLINE_TYPE.STRAIGHT:
                            Pathing_Spline.RebuildStraight(splineDetails.m_nodePrimary, splineDetails.m_nodeSecondary, out straightDir, out length);
                            break;
                        case Pathing_Spline.SPLINE_TYPE.BEZIER:
                            Pathing_Spline.RebuildBezier(splineDetails.m_nodePrimary, splineDetails.m_nodeSecondary, splineDetails.m_bezierStrength, out bezierPointA, out bezierPointB, out bezierControlA, out bezierControlB, out length);
                            break;
                        case Pathing_Spline.SPLINE_TYPE.CIRCLE:
                            Pathing_Spline.RebuildCircle(splineDetails.m_nodePrimary, splineDetails.m_nodeSecondary, splineDetails.m_circleDir, splineDetails.m_circleAngle, out circleHeight, out circleCenter, out centerADir, out length);
                            break;
                    }

                    switch (splineDetails.m_splineType)
                    {
                        case Pathing_Spline.SPLINE_TYPE.STRAIGHT:
                            p_position = Pathing_Spline.GetStraightPosition(p_percent, splineDetails.m_nodePrimary, straightDir);
                            break;
                        case Pathing_Spline.SPLINE_TYPE.BEZIER:
                            p_position = Pathing_Spline.GetBezierPosition(p_percent, bezierPointA, bezierPointB, bezierControlA, bezierControlB);
                            break;
                        case Pathing_Spline.SPLINE_TYPE.CIRCLE:
                            p_position = Pathing_Spline.GetCirclePosition(p_percent, splineDetails.m_circleDir, splineDetails.m_circleAngle, circleHeight, circleCenter, centerADir);
                            break;
                    }
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Given two nodes determine the position on a spline when valid
    /// </summary>
    /// <param name="p_nodeA">Node A</param>
    /// <param name="p_nodeB">Node B</param>
    /// <param name="p_percent">Percent on spline</param>
    /// <param name="p_position">out: the forward direction when calculated</param>
    /// <returns>true when able to get posiiton on a spline</returns>
    public static bool GetSplineForward(Pathing_Node p_nodeA, Pathing_Node p_nodeB, float p_percent, out Vector3 p_position)
    {
        p_position = Vector3.zero;

        if (p_nodeA != null && p_nodeB != null) //Valid setup
        {
            if (p_nodeA.ContainsConjoinedNode(p_nodeB))
            {
                Pathing_Spline.SPLINE_POSITION splinePosition = p_nodeA.DetermineNodePosition(p_nodeB);

                if (splinePosition != Pathing_Spline.SPLINE_POSITION.MAX_LENGTH)//Valid spline setup
                {
                    Pathing_Node.Spline_Details splineDetails = p_nodeA.m_pathingSplineDetails[(int)splinePosition];

                    //Setup spline variables
                    float length = 0.0f;

                    //Straigh
                    Vector3 straightDir = Vector3.zero;

                    //Bezier
                    Vector3 bezierPointA = Vector3.zero;
                    Vector3 bezierPointB = Vector3.zero;
                    Vector3 bezierControlA = Vector3.zero;
                    Vector3 bezierControlB = Vector3.zero;

                    //Circle
                    float circleHeight = 0.0f;
                    Vector3 circleCenter = Vector3.zero;
                    Vector3 centerADir = Vector3.zero;

                    switch (splineDetails.m_splineType)
                    {
                        case Pathing_Spline.SPLINE_TYPE.STRAIGHT:
                            Pathing_Spline.RebuildStraight(splineDetails.m_nodePrimary, splineDetails.m_nodeSecondary, out straightDir, out length);
                            break;
                        case Pathing_Spline.SPLINE_TYPE.BEZIER:
                            Pathing_Spline.RebuildBezier(splineDetails.m_nodePrimary, splineDetails.m_nodeSecondary, splineDetails.m_bezierStrength, out bezierPointA, out bezierPointB, out bezierControlA, out bezierControlB, out length);
                            break;
                        case Pathing_Spline.SPLINE_TYPE.CIRCLE:
                            Pathing_Spline.RebuildCircle(splineDetails.m_nodePrimary, splineDetails.m_nodeSecondary, splineDetails.m_circleDir, splineDetails.m_circleAngle, out circleHeight, out circleCenter, out centerADir, out length);
                            break;
                    }

                    switch (splineDetails.m_splineType)
                    {
                        case Pathing_Spline.SPLINE_TYPE.STRAIGHT:
                            p_position = Pathing_Spline.GetStraightForward(p_percent, straightDir);
                            break;
                        case Pathing_Spline.SPLINE_TYPE.BEZIER:
                            p_position = Pathing_Spline.GetBezierForward(p_percent, bezierPointA, bezierPointB, bezierControlA, bezierControlB);
                            break;
                        case Pathing_Spline.SPLINE_TYPE.CIRCLE:
                            p_position = Pathing_Spline.GetCircleForward(p_percent, splineDetails.m_circleDir, splineDetails.m_circleAngle, circleHeight, circleCenter, centerADir);
                            break;
                    }
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Given two nodes determine the spline length
    /// </summary>
    /// <param name="p_nodeA">Node A</param>
    /// <param name="p_nodeB">Node B</param>
    /// <returns>length of spline if valid, default to 1</returns>
    public static float GetSplineLength(Pathing_Node p_nodeA, Pathing_Node p_nodeB)
    {
        if (p_nodeA != null && p_nodeB != null) //Valid setup
        {
            if (p_nodeA.ContainsConjoinedNode(p_nodeB))
            {
                Pathing_Spline.SPLINE_POSITION splinePosition = p_nodeA.DetermineNodePosition(p_nodeB);

                if (splinePosition != Pathing_Spline.SPLINE_POSITION.MAX_LENGTH)//Valid spline setup
                {
                    Pathing_Node.Spline_Details splineDetails = p_nodeA.m_pathingSplineDetails[(int)splinePosition];

                    //Setup spline variables
                    float length = 0.0f;

                    //Straigh
                    Vector3 straightDir = Vector3.zero;

                    //Bezier
                    Vector3 bezierPointA = Vector3.zero;
                    Vector3 bezierPointB = Vector3.zero;
                    Vector3 bezierControlA = Vector3.zero;
                    Vector3 bezierControlB = Vector3.zero;

                    //Circle
                    float circleHeight = 0.0f;
                    Vector3 circleCenter = Vector3.zero;
                    Vector3 centerADir = Vector3.zero;

                    switch (splineDetails.m_splineType)
                    {
                        case Pathing_Spline.SPLINE_TYPE.STRAIGHT:
                            Pathing_Spline.RebuildStraight(splineDetails.m_nodePrimary, splineDetails.m_nodeSecondary, out straightDir, out length);
                            return length;
                        case Pathing_Spline.SPLINE_TYPE.BEZIER:
                            Pathing_Spline.RebuildBezier(splineDetails.m_nodePrimary, splineDetails.m_nodeSecondary, splineDetails.m_bezierStrength, out bezierPointA, out bezierPointB, out bezierControlA, out bezierControlB, out length);
                            return length;
                        case Pathing_Spline.SPLINE_TYPE.CIRCLE:
                            Pathing_Spline.RebuildCircle(splineDetails.m_nodePrimary, splineDetails.m_nodeSecondary, splineDetails.m_circleDir, splineDetails.m_circleAngle, out circleHeight, out circleCenter, out centerADir, out length);
                            return length;
                    }
                }
            }
        }

        return 1.0f;
    }

    /// <summary>
    /// Draw a flag using gizmos
    /// </summary>
    /// <param name="p_position">Point to draw flag</param>
    /// <param name="p_color">Color to use in gizmos</param>
    public static void DrawFlag(Vector3 p_position, Color p_color)
    {
        Gizmos.color = p_color;
        Gizmos.DrawLine(p_position, p_position + Vector3.up * DEBUG_FLAG_HEIGHT); //Stick
        Gizmos.DrawLine(p_position + Vector3.up * DEBUG_FLAG_HEIGHT, p_position + Vector3.up * (DEBUG_FLAG_HEIGHT - 0.5f) + Vector3.forward); //Top flag edge
        Gizmos.DrawLine(p_position + Vector3.up * (DEBUG_FLAG_HEIGHT - 1.0f), p_position + Vector3.up * (DEBUG_FLAG_HEIGHT - 0.5f) + Vector3.forward); //Bottom flag edge

    }
}
