using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing_Spline : MonoBehaviour
{
    private const int STEPS = 10;

    public Pathing_Node m_nodeA = null;
    public Pathing_Node m_nodeB = null;

    public enum SPLINE_TYPE {STRAIGHT, BEZIER, CIRCLE }
    public SPLINE_TYPE m_splineType = SPLINE_TYPE.BEZIER;

    [Header("Bezier Settings")]
    public float m_controlPointStrength = 1.0f;
    public enum CIRCLE_DIR {CLOCKWISE, COUNTER_CLOCKWISE }
    [Header("Circle values")]
    public CIRCLE_DIR m_circleDir = CIRCLE_DIR.CLOCKWISE;
    public float m_circleAngle = 90.0f;


    //Stored varibles for efficiency sake
    [HideInInspector]
    public float m_splineLength = 1.0f;
        
    //Straight
    private Vector3 m_straightDir = Vector3.zero;

    //Bezier
    private Vector3 m_bezierPointA = Vector3.zero;
    private Vector3 m_bezierPointB = Vector3.zero;
    private Vector3 m_bezierControlA = Vector3.zero;
    private Vector3 m_bezierControlB = Vector3.zero;

    //Circle
    private float m_circleHeight = 0.0f;
    private Vector3 m_circleCenter = Vector3.zero;
    private Vector3 m_centerADir = Vector3.zero;

    private void Start()
    {
        InitSpline();
    }

    /// <summary>
    /// Initilasie the spline for settings 
    /// </summary>
    public void InitSpline()
    {
        switch (m_splineType)
        {
            case SPLINE_TYPE.STRAIGHT:
                InitStraight();
                break;
            case SPLINE_TYPE.BEZIER:
                InitBezier();
                break;
            case SPLINE_TYPE.CIRCLE:
                InitCircle();
                break;
            default:
                break;
        }
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

    #region Inits

    /// <summary>
    /// Initliase the straight spline
    /// </summary>
    private void InitStraight()
    {
        m_straightDir = m_nodeB.transform.position - m_nodeA.transform.position;
        m_splineLength = m_straightDir.magnitude;
    }

    /// <summary>
    /// Setup varibles used in bezier calculations
    /// </summary>
    private void InitBezier()
    {
        m_bezierPointA = m_nodeA.transform.position;
        m_bezierPointB = m_nodeB.transform.position;
        m_bezierControlA = m_bezierPointA + m_nodeA.transform.forward * m_controlPointStrength;
        m_bezierControlB = m_bezierPointB - m_nodeB.transform.forward * m_controlPointStrength;

        //See example at https://stackoverflow.com/questions/29438398/cheap-way-of-calculating-cubic-bezier-length
        float chord = (m_bezierPointB - m_bezierPointA).magnitude;
        float cont_net = (m_bezierPointA - m_bezierControlA).magnitude + (m_bezierControlB - m_bezierControlA).magnitude + (m_bezierPointB - m_bezierControlB).magnitude;

        m_splineLength = (cont_net + chord) / 2;
    }

    /// <summary>
    /// Setup varibles in circle calculations
    /// </summary>
    private void InitCircle()
    {
        Vector3 ABDir = m_nodeB.transform.position - m_nodeA.transform.position;
        Vector3 ABHalf = ABDir / 2.0f;
        m_circleHeight = ABDir.y;

        //Ignore y for maths
        ABDir.y = 0.0f;
        ABHalf.y = 0.0f;

        Vector3 ABPerp = Quaternion.Euler(0.0f, m_circleDir == CIRCLE_DIR.CLOCKWISE ? 90.0f : -90.0f, 0.0f) * ABHalf;

        Vector3 centerOffset = Mathf.Tan((-m_circleAngle + 180)  * Mathf.Deg2Rad / 2.0f) * ABPerp;

        m_circleCenter = m_nodeA.transform.position + ABHalf + centerOffset;

        m_centerADir =  m_nodeA.transform.position - m_circleCenter;

        m_splineLength = 2.0f * Mathf.PI * m_centerADir.magnitude * m_circleAngle/360.0f;
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
            InitStraight();

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
        Vector3 forward = desiredDir * (m_circleDir == CIRCLE_DIR.CLOCKWISE ? -1:1);
        forward.y = 0.0f;
        return forward.normalized;
    }
    #endregion

    #region Editor Specific
#if UNITY_EDITOR

    /// <summary>
    /// Purely cosmetic, update position of spline in editor
    /// </summary>
    public void UpdatePosition()
    {
        //Draw the line from A to B
        if (m_nodeA == null || m_nodeB == null)
            return;

        //For visiblility dont be right in middle
        transform.position = GetPosition(0.3f); 
    }

    /// <summary>
    /// Draw the correct line for the given spline
    /// </summary>
    private void OnDrawGizmos()
    {
        //Draw the line from A to B
        if (m_nodeA == null || m_nodeB == null)
            return;

        //ensure data is up to date
        InitSpline();

        Gizmos.color = Color.blue;

        float percentStep = 1.0f / STEPS;
        float currentPercent = percentStep;

        Vector3 previous = m_nodeA.transform.position;
        //Loop through approximating circle, every (m_totalDegrees / DEBUG_STEPS) degrees
        for (int i = 1; i <= STEPS; i++)
        {
            Vector3 next = GetPosition(currentPercent);

            Gizmos.DrawLine(previous, next);

            previous = next;
            currentPercent += percentStep;
        }
    }
#endif
    #endregion
}
