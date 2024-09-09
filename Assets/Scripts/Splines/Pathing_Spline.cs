using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing_Spline : MonoBehaviour
{
    public enum SPLINE_TYPE { STRAIGHT, BEZIER, CIRCLE, NOT_IN_USE}
    public enum CIRCLE_DIR { CLOCKWISE, COUNTER_CLOCKWISE }
    public enum SPLINE_POSITION {FORWARD = 0, FORWARD_RIGHT, FORWARD_LEFT, BACKWARD, BACKWARD_RIGHT, BACKWARD_LEFT, MAX_LENGTH} //Where on a node is the spline


    //Assigned from editor
    public Pathing_Node m_nodePrimary = null; //Always at percentage 0.0f
    public SPLINE_POSITION m_nodePositionPrimary = SPLINE_POSITION.MAX_LENGTH;
    public Pathing_Node m_nodeSecondary = null;  //Always at percentage 1.0f
    public SPLINE_POSITION m_nodePositionSecondary = SPLINE_POSITION.MAX_LENGTH;

    private bool m_disableYSnapping = false;
    private SPLINE_TYPE m_splineType = SPLINE_TYPE.NOT_IN_USE;
    private CIRCLE_DIR m_circleDir = CIRCLE_DIR.CLOCKWISE;
    private float m_circleAngle = 90.0f;
    private float m_bezierStrength = 10.0f;

    //Derived varibles
    public float m_splineLength = 1.0f;

    //Straight
    private Vector3 m_straightDir;

    //Bezier
    private Vector3 m_bezierPointA;
    private Vector3 m_bezierPointB;
    private Vector3 m_bezierControlA;
    private Vector3 m_bezierControlB;

    //Circle
    private float m_circleHeight;
    private Vector3 m_circleCenter;
    private Vector3 m_centerADir;

    /// <summary>
    /// Assigne all varibles to a spline
    /// Applies spline to relavant nodes
    /// </summary>
    /// <param name="p_details">Details to base spline off</param>
    public void InitVaribles(Pathing_Node.Spline_Details p_details)
    {
        m_nodePrimary = p_details.m_nodePrimary;
        m_nodeSecondary = p_details.m_nodeSecondary;
        m_nodePositionPrimary = m_nodePrimary.DetermineNodePosition(m_nodeSecondary);
        m_nodePositionSecondary = m_nodeSecondary.DetermineNodePosition(m_nodePrimary); ;

        m_disableYSnapping = p_details.m_disableYSnapping;
        m_splineType = p_details.m_splineType;
        m_circleDir = p_details.m_circleDir;
        m_circleAngle = p_details.m_circleAngle;
        m_bezierStrength = p_details.m_bezierStrength;

        m_nodePrimary.AddSpline(this, m_nodePositionPrimary);
        m_nodeSecondary.AddSpline(this, m_nodePositionSecondary);

        SetupSpline();

        transform.position = GetPosition(0.5f);
    }

    /// <summary>
    /// Is this a valid spline? Not nodes in use have this as a designated spline
    /// </summary>
    /// <returns></returns>
    public bool IsValidSpline()
    {
        if (m_nodePrimary == null || m_nodeSecondary == null)
            return false;

        return m_nodePrimary.ContainsSpline(this) && m_nodeSecondary.ContainsSpline(this);
    }

    /// <summary>
    /// Update a spline with updated details
    /// </summary>
    /// <param name="p_splineDetails">Details to update</param>
    public void UpdateSplineDetails(Pathing_Node.Spline_Details p_splineDetails)
    {
        m_splineType = p_splineDetails.m_splineType;
        m_circleDir = p_splineDetails.m_circleDir;
        m_circleAngle = p_splineDetails.m_circleAngle;
        m_bezierStrength = p_splineDetails.m_bezierStrength;

        SetupSpline();
    }

    /// <summary>
    /// Rebuild the spline for derived varibles 
    /// </summary>
    public void SetupSpline()
    {
        switch (m_splineType)
        {
            case SPLINE_TYPE.STRAIGHT:
                RebuildStraight(m_nodePrimary, m_nodeSecondary, out m_straightDir, out m_splineLength);
                break;
            case SPLINE_TYPE.BEZIER:
                RebuildBezier(m_nodePrimary, m_nodeSecondary, m_bezierStrength, out m_bezierPointA, out m_bezierPointB, out m_bezierControlA, out m_bezierControlB, out m_splineLength);
                break;
            case SPLINE_TYPE.CIRCLE:
                RebuildCircle(m_nodePrimary, m_nodeSecondary, m_circleDir, m_circleAngle, out m_circleHeight, out m_circleCenter, out m_centerADir, out m_splineLength);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Get the splines length
    /// </summary>
    /// <returns>Returns stored varible m_splineLength</returns>
    public float GetSplineLength()
    {
        return m_splineLength;
    }
    /// <summary>
    /// Get the position of the spline based off percent
    /// </summary>
    /// <param name="p_percent">What percent along the spline</param>
    /// <returns>Postions based off spline type and settings</returns>
    public Vector3 GetPosition(float p_percent)
    {
        switch (m_splineType)
        {
            case SPLINE_TYPE.STRAIGHT:
                return GetStraightPosition(p_percent, m_nodePrimary, m_straightDir);
            case SPLINE_TYPE.BEZIER:
                return GetBezierPosition(p_percent, m_bezierPointA, m_bezierPointB, m_bezierControlA, m_bezierControlB);
            case SPLINE_TYPE.CIRCLE:
                return GetCirclePosition(p_percent, m_circleDir, m_circleAngle, m_circleHeight, m_circleCenter, m_centerADir);
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Does this spline have YSnapping?
    /// </summary>
    /// <returns>True when YSnapping enabled</returns>
    public bool GetYSnapping()
    {
        return m_disableYSnapping;
    }

    /// <summary>
    /// Get the forward direction of the spline based off percent
    /// </summary>
    /// <param name="p_percent">What percent along the spline</param>
    /// <returns>Forward Dir based off spline type and settings</returns>
    public Vector3 GetForwardDir(float p_percent)
    {
        switch (m_splineType)
        {
            case SPLINE_TYPE.STRAIGHT:
                return GetStraightForward(p_percent, m_straightDir);
            case SPLINE_TYPE.BEZIER:
                return GetBezierForward(p_percent, m_bezierPointA, m_bezierPointB, m_bezierControlA, m_bezierControlB);
            case SPLINE_TYPE.CIRCLE:
                return GetCircleForward(p_percent, m_circleDir, m_circleAngle, m_circleHeight, m_circleCenter, m_centerADir);
        }

        return Vector3.forward;
    }

    /// <summary>
    /// Determine what percent a node is on the spline
    /// </summary>
    /// <param name="p_node">Node to check against</param>
    /// <returns>Will return 1 for node B or default to 0 for A or not found</returns>
    public float GetPercentForNode(Pathing_Node p_node)
    {
        return p_node == m_nodeSecondary ? 0.999f : 0.001f;
    }

    /// <summary>
    /// Given a distance calculate how far traveled percent wise
    /// </summary>
    /// <param name="p_distanceTravelled">Distance</param>
    /// <returns>What percent</returns>
    public float ChangeInPercent(float p_distanceTravelled)
    {
        return p_distanceTravelled / m_splineLength;
    }

    /// <summary>
    /// Given a current percent and an offset, whats the new percent?
    /// </summary>
    /// <param name="p_currentPercent">Current spline percent</param>
    /// <param name="p_offsetAmount">Offset amount, or forward amount</param>
    /// <returns>percent value</returns>
    public float GetSplinePercentFromOffset(float p_currentPercent, float p_offsetAmount)
    {
        return p_currentPercent + (p_offsetAmount / m_splineLength);
    }

    #region Rebuild

    /// <summary>
    /// Build a stright spline
    /// </summary>
    /// <param name="p_nodePrimary">Primary node in spline</param>
    /// <param name="p_nodeSecondary">Secondary node in spline</param>
    /// <param name="p_straightDir">Out: Straight spline direction</param>
    /// <param name="p_length">Out: Spline length</param>
    public static void RebuildStraight(Pathing_Node p_nodePrimary, Pathing_Node p_nodeSecondary, out Vector3 p_straightDir, out float p_length)
    {
        p_straightDir = p_nodeSecondary.transform.position - p_nodePrimary.transform.position;
        p_length = MOARMaths.VectorDistanceNoY(p_straightDir);
    }


    /// <summary>
    /// Setup varibles used in bezier calculations
    /// </summary>
    /// <param name="p_nodePrimary">Primary node in spline</param>
    /// <param name="p_nodeSecondary">Secondary node in spline</param>
    /// <param name="p_bezierStrenght">The strength of the bezier curve</param>
    /// <param name="p_bezierPointA">Out: Calcualted Bezier Point A</param>
    /// <param name="p_bezierPointB">Out: Calcualted Bezier Point B</param>
    /// <param name="p_bezierControlA">Out: Calcualted Bezier Control A</param>
    /// <param name="p_bezierControlB">Out: Calcualted Bezier Control B</param>
    /// <param name="p_length">Out: Spline length</param>
    public static void RebuildBezier(Pathing_Node p_nodePrimary, Pathing_Node p_nodeSecondary, float p_bezierStrenght, out Vector3 p_bezierPointA, out Vector3 p_bezierPointB, out Vector3 p_bezierControlA, out Vector3 p_bezierControlB, out float p_length)
    {
        p_bezierPointA = p_nodePrimary.transform.position;
        p_bezierPointB = p_nodeSecondary.transform.position;
        p_bezierControlA = p_bezierPointA + p_nodePrimary.transform.forward * p_bezierStrenght;
        p_bezierControlB = p_bezierPointB - p_nodeSecondary.transform.forward * p_bezierStrenght;

        //See example at https://stackoverflow.com/questions/29438398/cheap-way-of-calculating-cubic-bezier-length
        float chord = (p_bezierPointB - p_bezierPointA).magnitude;
        float cont_net = (p_bezierPointA - p_bezierControlA).magnitude + (p_bezierControlB - p_bezierControlA).magnitude + (p_bezierPointB - p_bezierControlB).magnitude;

        p_length = (cont_net + chord) / 2;
    }

    /// <summary>
    /// Setup varibles in circle calculations
    /// </summary>
    /// <param name="p_nodePrimary">Primary node in spline</param>
    /// <param name="p_nodeSecondary">Secondary node in spline</param>
    /// <param name="p_circleDirection">Waht direction is the circle, clockwise or anit?</param>
    /// <param name="p_circleAngle">How many degrees is the circle</param>
    /// <param name="p_circleHeight">Out: Height of the given circle</param>
    /// <param name="p_circleCenter">Out: Center position of hte circle</param>
    /// <param name="p_centerADir">Out: Vector outwards from circle</param>
    /// <param name="p_length">Out: Spline length</param>
    public static void RebuildCircle(Pathing_Node p_nodePrimary, Pathing_Node p_nodeSecondary, CIRCLE_DIR p_circleDirection, float p_circleAngle, out float p_circleHeight,  out Vector3 p_circleCenter, out Vector3 p_centerADir, out float p_length)
    {
        Vector3 ABDir = p_nodeSecondary.transform.position - p_nodePrimary.transform.position;
        Vector3 ABHalf = ABDir / 2.0f;
        p_circleHeight = ABDir.y;

        //Ignore y for maths
        ABDir.y = 0.0f;
        ABHalf.y = 0.0f;

        Vector3 ABPerp = Quaternion.Euler(0.0f, p_circleDirection == CIRCLE_DIR.CLOCKWISE ? 90.0f : -90.0f, 0.0f) * ABHalf;

        Vector3 centerOffset = Mathf.Tan((-p_circleAngle + 180) * Mathf.Deg2Rad / 2.0f) * ABPerp;

        p_circleCenter = p_nodePrimary.transform.position + ABHalf + centerOffset;

        p_centerADir = p_nodePrimary.transform.position - p_circleCenter;

        p_length = 2.0f * Mathf.PI * p_centerADir.magnitude * p_circleAngle / 360.0f;
    }
    #endregion

    #region Getting positions
    /// <summary>
    /// Get a position based off the spline and what percent
    /// </summary>
    /// <param name="p_percent">How far along spline are we?</param>
    /// <param name="p_nodePrimary">Primary node in spline</param>
    /// <param name="p_straightDir">Straight spline direction</param>
    /// <returns>Position based off spline calcualtions for straight line</returns>
    public static Vector3 GetStraightPosition(float p_percent, Pathing_Node p_nodePrimary, Vector3 p_straightDir)
    {
        return p_nodePrimary.transform.position + p_straightDir * p_percent;
    }

    /// <summary>
    /// Get a position based off the spline and what percent
    /// </summary>
    /// <param name="p_percent">How far along spline are we?</param>
    /// <param name="p_bezierPointA">Calcualted Bezier Point A</param>
    /// <param name="p_bezierPointB">Calcualted Bezier Point B</param>
    /// <param name="p_bezierControlA">Calcualted Bezier Control A</param>
    /// <param name="p_bezierControlB">Calcualted Bezier Control B</param>
    /// <returns>Position based off spline calcualtions for bezier curve</returns>
    public static Vector3 GetBezierPosition(float p_percent, Vector3 p_bezierPointA, Vector3 p_bezierPointB, Vector3 p_bezierControlA, Vector3 p_bezierControlB)
    {
        return Mathf.Pow(1f - p_percent, 3f) * p_bezierPointA + 3f * Mathf.Pow(1f - p_percent, 2f) * p_percent * p_bezierControlA + 3f * (1f - p_percent) * Mathf.Pow(p_percent, 2f) * p_bezierControlB + Mathf.Pow(p_percent, 3f) * p_bezierPointB;
    }

    /// <summary>
    /// Get a position based off the spline and what percent
    /// </summary>
    /// <param name="p_percent">How far along spline are we?</param>
    /// <param name="p_circleDir">Waht direction is the circle, clockwise or anit?</param>
    /// <param name="p_circleAngle">How many degrees is the circle</param>
    /// <param name="p_circleHeight">Height of the given circle</param>
    /// <param name="p_circleCenter">Center position of hte circle</param>
    /// <param name="p_centerADir">Vector outwards from circle</param>
    /// <returns>Position based off spline calcualtions for circle</returns>
    public static Vector3 GetCirclePosition(float p_percent, CIRCLE_DIR p_circleDir, float p_circleAngle, float p_circleHeight, Vector3 p_circleCenter, Vector3 p_centerADir)
    {
        Quaternion transformRot = Quaternion.Euler(0.0f, p_circleDir == CIRCLE_DIR.CLOCKWISE ? p_circleAngle * p_percent : -p_circleAngle * p_percent, 0.0f);
        Vector3 position = p_circleCenter + (transformRot * p_centerADir);

        position.y += p_circleHeight * p_percent;

        return position;
    }
    #endregion

    #region Getting Forward
    /// <summary>
    /// Get a forward based off the spline and what percent
    /// </summary>
    /// <param name="p_percent">How far along spline are we?</param>
    /// <param name="p_straightDir">Straight spline direction</param>
    /// <returns>Forward based off spline calcualtions for straight line</returns>
    public static Vector3 GetStraightForward(float p_percent, Vector3 p_straightDir)
    {
        Vector3 forward = p_straightDir;
        forward.y = 0.0f;
        return forward.normalized;
    }

    /// <summary>
    /// Get a forward based off the spline and what percent
    /// </summary>
    /// <param name="p_percent">How far along spline are we?</param>
    /// <param name="p_bezierPointA">Calcualted Bezier Point A</param>
    /// <param name="p_bezierPointB">Calcualted Bezier Point B</param>
    /// <param name="p_bezierControlA">Calcualted Bezier Control A</param>
    /// <param name="p_bezierControlB">Calcualted Bezier Control B</param>
    /// <returns>Forward based off spline calcualtions for bezier curve</returns>
    public static Vector3 GetBezierForward(float p_percent, Vector3 p_bezierPointA, Vector3 p_bezierPointB, Vector3 p_bezierControlA, Vector3 p_bezierControlB)
    {
        Vector3 currentPos = GetBezierPosition(p_percent, p_bezierPointA, p_bezierPointB, p_bezierControlA, p_bezierControlB);
        Vector3 offsetPos = GetBezierPosition(p_percent + 0.01f, p_bezierPointA, p_bezierPointB, p_bezierControlA, p_bezierControlB);
        Vector3 forward = (offsetPos - currentPos);
        forward.y = 0.0f;
        return forward.normalized;
    }

    /// <summary>
    /// Get a forward based off the spline and what percent
    /// </summary>
    /// <param name="p_percent">How far along spline are we?</param>
    /// <param name="p_circleDir">Waht direction is the circle, clockwise or anit?</param>
    /// <param name="p_circleAngle">How many degrees is the circle</param>
    /// <param name="p_circleHeight">Height of the given circle</param>
    /// <param name="p_circleCenter">Center position of hte circle</param>
    /// <param name="p_centerADir">Vector outwards from circle</param>
    /// <returns>Forward based off spline calcualtions for circle</returns>
    public static Vector3 GetCircleForward(float p_percent, CIRCLE_DIR p_circleDir, float p_circleAngle, float p_circleHeight, Vector3 p_circleCenter, Vector3 p_centerADir)
    {
        Vector3 percentPos = GetCirclePosition(p_percent, p_circleDir, p_circleAngle, p_circleHeight, p_circleCenter, p_centerADir);
        Vector3 desiredDir = p_circleCenter - percentPos;
        desiredDir = new Vector3(desiredDir.z, 0.0f, -desiredDir.x);
        Vector3 forward = desiredDir * (p_circleDir == CIRCLE_DIR.CLOCKWISE ? -1 : 1);
        forward.y = 0.0f;
        return forward.normalized;
    }
    #endregion
}
