using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing_Spline : ScriptableObject
{
    public enum SPLINE_TYPE { STRAIGHT, BEZIER, CIRCLE }
    public enum CIRCLE_DIR { CLOCKWISE, COUNTER_CLOCKWISE }
    public enum SPLINE_POSITION {FOWARD = 0, FORWARD_RIGHT, FORWARD_LEFT, BACKWARD, BACKWARD_RIGHT, BACKWARD_LEFT, MAX_LENGTH} //Where on a node is the spline

    //Assigned from editor
    public Pathing_Node m_nodeA = null;
    public SPLINE_POSITION m_nodeAPosition = SPLINE_POSITION.MAX_LENGTH;

    public Pathing_Node m_nodeB = null;
    public SPLINE_POSITION m_nodeBPosition = SPLINE_POSITION.MAX_LENGTH;

    private SPLINE_TYPE m_splineType = SPLINE_TYPE.STRAIGHT;

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
    /// </summary>
    /// <param name="p_nodeA"></param>
    /// <param name="p_nodeB"></param>
    /// <param name="p_splineType"></param>
    /// <param name="p_circleDir"></param>
    /// <param name="p_circleAngle"></param>
    /// <param name="p_bezierStrength"></param>
    public void InitVaribles(Pathing_Node p_nodeA, SPLINE_POSITION p_nodeAPosition, Pathing_Node p_nodeB, SPLINE_POSITION p_nodeBPosition, SPLINE_TYPE p_splineType, CIRCLE_DIR p_circleDir, float p_circleAngle, float p_bezierStrength)
    {
        m_nodeA = p_nodeA;
        m_nodeAPosition = p_nodeAPosition;
        m_nodeB = p_nodeB;
        m_nodeBPosition = p_nodeBPosition;
        m_splineType = p_splineType;
        m_circleDir = p_circleDir;
        m_circleAngle = p_circleAngle;
        m_bezierStrength = p_bezierStrength;

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
                RebuildStraight();
                break;
            case SPLINE_TYPE.BEZIER:
                RebuildBezier();
                break;
            case SPLINE_TYPE.CIRCLE:
                RebuildCircle();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// If a spline has been removed ensure all "connections" are also removed
    /// </summary>
    public void SplineRemoved()
    {
        if(m_nodeA != null && m_nodeAPosition != SPLINE_POSITION.MAX_LENGTH)
        {
            m_nodeA.m_pathingSplines[(int)m_nodeAPosition] = null;
        }
        if (m_nodeB != null && m_nodeBPosition != SPLINE_POSITION.MAX_LENGTH)
        {
            m_nodeB.m_pathingSplines[(int)m_nodeBPosition] = null;
        }
    }

    /// <summary>
    /// Determine if the given details match up to the spline
    /// </summary>
    /// <param name="p_currentNode">Node to check against</param>
    /// <param name="p_splineDetails">Node details to check against</param>
    /// <returns>true when all the details matchup</returns>
    public bool SameAsDetails(Pathing_Node p_currentNode, Pathing_Node.Spline_Details p_splineDetails)
    {
        Pathing_Node conjoinedNode = p_splineDetails.m_conjoinedNode;

        //Check node details, could be back to front
        if(m_nodeA == p_currentNode && m_nodeB == conjoinedNode)
        {
            if (m_nodeAPosition != m_nodeA.DetermineNodePosition(m_nodeB) || m_nodeBPosition != m_nodeB.DetermineNodePosition(m_nodeA))
                return false;
        }
        else if(m_nodeB == p_currentNode && m_nodeA == conjoinedNode)
        {
            if (m_nodeAPosition != m_nodeB.DetermineNodePosition(m_nodeA) || m_nodeBPosition != m_nodeA.DetermineNodePosition(m_nodeB))
                return false;
        }
        else
        {
            return false;
        }

        //Circle Direction is opposite 

        if (m_circleDir == p_splineDetails.m_circleDir)
            return false;

        //Basic details
        if (m_splineType != p_splineDetails.m_splineType || m_circleAngle != p_splineDetails.m_circleAngle || m_bezierStrength != p_splineDetails.m_bezierStrength)
            return false;

        return true;

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
                return GetStraightPosition(p_percent);
            case SPLINE_TYPE.BEZIER:
                return GetBezierPosition(p_percent);
            case SPLINE_TYPE.CIRCLE:
                return GetCirclePosition(p_percent);
        }

        return Vector3.zero;
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
                return GetStraightForward(p_percent);
            case SPLINE_TYPE.BEZIER:
                return GetBezierForward(p_percent);
            case SPLINE_TYPE.CIRCLE:
                return GetCircleForward(p_percent);
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
        return p_node == m_nodeB ? 0.999f : 0.001f;
    }

    /// <summary>
    /// Given a distance calculate how far traveled percent wise
    /// </summary>
    /// <param name="p_distanceTravelled">Distance</param>
    /// <returns>What percent</returns>
    public float ChangeinPercent(float p_distanceTravelled)
    {
        return p_distanceTravelled / m_splineLength;
    }

    #region Rebuild

    /// <summary>
    /// Rebuild the straight spline
    /// </summary>
    private void RebuildStraight()
    {
        m_straightDir = m_nodeB.transform.position - m_nodeA.transform.position;
        m_splineLength = m_straightDir.magnitude;
    }

    /// <summary>
    /// Setup varibles used in bezier calculations
    /// </summary>
    private void RebuildBezier()
    {
        m_bezierPointA = m_nodeA.transform.position;
        m_bezierPointB = m_nodeB.transform.position;
        m_bezierControlA = m_bezierPointA + m_nodeA.transform.forward * m_bezierStrength;
        m_bezierControlB = m_bezierPointB - m_nodeB.transform.forward * m_bezierStrength;

        //See example at https://stackoverflow.com/questions/29438398/cheap-way-of-calculating-cubic-bezier-length
        float chord = (m_bezierPointB - m_bezierPointA).magnitude;
        float cont_net = (m_bezierPointA - m_bezierControlA).magnitude + (m_bezierControlB - m_bezierControlA).magnitude + (m_bezierPointB - m_bezierControlB).magnitude;

        m_splineLength = (cont_net + chord) / 2;
    }

    /// <summary>
    /// Setup varibles in circle calculations
    /// </summary>
    private void RebuildCircle()
    {
        Vector3 ABDir = m_nodeB.transform.position - m_nodeA.transform.position;
        Vector3 ABHalf = ABDir / 2.0f;
        m_circleHeight = ABDir.y;

        //Ignore y for maths
        ABDir.y = 0.0f;
        ABHalf.y = 0.0f;

        Vector3 ABPerp = Quaternion.Euler(0.0f, m_circleDir == CIRCLE_DIR.CLOCKWISE ? 90.0f : -90.0f, 0.0f) * ABHalf;

        Vector3 centerOffset = Mathf.Tan((-m_circleAngle + 180) * Mathf.Deg2Rad / 2.0f) * ABPerp;

        m_circleCenter = m_nodeA.transform.position + ABHalf + centerOffset;

        m_centerADir = m_nodeA.transform.position - m_circleCenter;

        m_splineLength = 2.0f * Mathf.PI * m_centerADir.magnitude * m_circleAngle / 360.0f;
    }
    #endregion

    #region Getting positions
    /// <summary>
    /// Get a position based off the spline and what percent
    /// </summary>
    /// <param name="p_percent">How far along spline are we?</param>
    /// <returns>Position based off spline calcualtions for straight line</returns>
    private Vector3 GetStraightPosition(float p_percent)
    {
        return m_nodeA.transform.position + m_straightDir * p_percent;
    }

    /// <summary>
    /// Get a position based off the spline and what percent
    /// </summary>
    /// <param name="p_percent">How far along spline are we?</param>
    /// <returns>Position based off spline calcualtions for bezier curve</returns>
    private Vector3 GetBezierPosition(float p_percent)
    {
        return Mathf.Pow(1f - p_percent, 3f) * m_bezierPointA + 3f * Mathf.Pow(1f - p_percent, 2f) * p_percent * m_bezierControlA + 3f * (1f - p_percent) * Mathf.Pow(p_percent, 2f) * m_bezierControlB + Mathf.Pow(p_percent, 3f) * m_bezierPointB;
    }

    /// <summary>
    /// Get a position based off the spline and what percent
    /// </summary>
    /// <param name="p_percent">How far along spline are we?</param>
    /// <returns>Position based off spline calcualtions for circle</returns>
    private Vector3 GetCirclePosition(float p_percent)
    {
        if (m_circleAngle % 360 == 0.0f) //Dont want it to go to infinite fo  just use stright
        {
            RebuildStraight();

            return GetStraightPosition(p_percent);
        }

        Quaternion transformRot = Quaternion.Euler(0.0f, m_circleDir == CIRCLE_DIR.CLOCKWISE ? m_circleAngle * p_percent : -m_circleAngle * p_percent, 0.0f);
        Vector3 position = m_circleCenter + (transformRot * m_centerADir);

        position.y = m_nodeA.transform.position.y + m_circleHeight * p_percent;

        return position;
    }
    #endregion

    #region Getting Forward
    /// <summary>
    /// Get a forward based off the spline and what percent
    /// </summary>
    /// <param name="p_percent">How far along spline are we?</param>
    /// <returns>Forward based off spline calcualtions for straight line</returns>
    private Vector3 GetStraightForward(float p_percent)
    {
        Vector3 forward = m_straightDir;
        forward.y = 0.0f;
        return forward.normalized;
    }

    /// <summary>
    /// Get a forward based off the spline and what percent
    /// </summary>
    /// <param name="p_percent">How far along spline are we?</param>
    /// <returns>Forward based off spline calcualtions for bezier curve</returns>
    private Vector3 GetBezierForward(float p_percent)
    {
        Vector3 currentPos = GetBezierPosition(p_percent);
        Vector3 offsetPos = GetBezierPosition(p_percent + 0.01f);
        Vector3 forward = (offsetPos - currentPos);
        forward.y = 0.0f;
        return forward.normalized;
    }

    /// <summary>
    /// Get a forward based off the spline and what percent
    /// </summary>
    /// <param name="p_percent">How far along spline are we?</param>
    /// <returns>Forward based off spline calcualtions for circle</returns>
    private Vector3 GetCircleForward(float p_percent)
    {
        Vector3 percentPos = GetCirclePosition(p_percent);
        Vector3 desiredDir = m_circleCenter - percentPos;
        desiredDir = new Vector3(desiredDir.z, 0.0f, -desiredDir.x);
        Vector3 forward = desiredDir * (m_circleDir == CIRCLE_DIR.CLOCKWISE ? -1 : 1);
        forward.y = 0.0f;
        return forward.normalized;
    }
    #endregion
}
