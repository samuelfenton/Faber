using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Pathing_Node : MonoBehaviour
{
    [System.Serializable]
    public struct Spline_Details
    {
        [SerializeField]
        public Pathing_Spline.SPLINE_POSITION m_splinePosition;

        [SerializeField]
        public Pathing_Node m_nodePrimary;
        [SerializeField]
        public Pathing_Node m_nodeSecondary;

        [SerializeField]
        public Pathing_Spline.SPLINE_TYPE m_splineType;
        [SerializeField]
        public Pathing_Spline.CIRCLE_DIR m_circleDir;
        [SerializeField]
        public float m_circleAngle;
        [SerializeField]
        public float m_bezierStrength;
        [SerializeField]
        public bool m_createdFlag;

        public Spline_Details(Pathing_Spline.SPLINE_POSITION p_splinePosition)
        {
            m_splinePosition = p_splinePosition;
            m_nodePrimary = null;
            m_nodeSecondary = null;
            m_splineType = Pathing_Spline.SPLINE_TYPE.NOT_IN_USE;
            m_circleDir = Pathing_Spline.CIRCLE_DIR.CLOCKWISE;
            m_circleAngle = 0.0f;
            m_bezierStrength = 0.0f;
            m_createdFlag = false;
        }

        /// <summary>
        /// Does the current spline have both 
        /// </summary>
        /// <returns></returns>
        public bool IsValidSpline()
        {
            return m_nodePrimary != null && m_nodeSecondary != null && m_splinePosition != Pathing_Spline.SPLINE_POSITION.MAX_LENGTH && m_splineType != Pathing_Spline.SPLINE_TYPE.NOT_IN_USE;
        }
    };

    public enum TRIGGER_DIRECTION { FORWARDS, BACKWARDS }

    [HideInInspector]
    public Spline_Details[] m_pathingSplineDetails = new Spline_Details[(int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH];

    [HideInInspector]
    public Pathing_Spline[] m_pathingSplines = new Pathing_Spline[(int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH];

    [HideInInspector]
    public List<Pathing_Spline> m_adjacentSplines = new List<Pathing_Spline>();

    [HideInInspector]
    public Dictionary<Entity, TRIGGER_DIRECTION> m_activeColliders = new Dictionary<Entity, TRIGGER_DIRECTION>();

    //Plane Equations
    private Vector3 m_planeForwardVector = Vector3.zero;

    /// <summary>
    /// Setup colision plane
    /// </summary>
    public void InitNode()
    {
        m_planeForwardVector = transform.forward;
        m_planeForwardVector.y = 0;
        m_planeForwardVector = m_planeForwardVector.normalized;

        for (int splineIndex = 0; splineIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineIndex++)
        {
            if (m_pathingSplines[splineIndex] != null)
                m_adjacentSplines.Add(m_pathingSplines[splineIndex]);
        }
    }

    /// <summary>
    /// USed to set starting values
    /// </summary>
    private void Reset()
    {
        m_pathingSplineDetails = new Spline_Details[(int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH];
        for (int splineIndex = 0; splineIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineIndex++)
        {
            //Forced assignment 
            m_pathingSplineDetails[splineIndex].m_splinePosition = (Pathing_Spline.SPLINE_POSITION)splineIndex;
        }
    }

    /// <summary>
    /// Character is moving through the collider itself
    /// Determine exact moment of moving through using plane formula
    /// ax + by + cz + d where a,b,c,d are determiened from the plane equation
    /// </summary>
    private void Update()
    {
        //Get all keys
        Entity[] transferingEntities = new Entity[m_activeColliders.Count];
        m_activeColliders.Keys.CopyTo(transferingEntities, 0);

        for (int entityIndex = 0; entityIndex < transferingEntities.Length; entityIndex++)
        {
            Entity entity = transferingEntities[entityIndex];

            TRIGGER_DIRECTION previousDirection = m_activeColliders[entity];
            if (ValidNode()) //Valid node
            {
                TRIGGER_DIRECTION currentDirection = GetTriggerDir(entity.transform.position);

                if (previousDirection != currentDirection) //Updating dir
                {
                    Entity.TURNING_DIR desiredTurnignDir = entity.GetDesiredTurning(this);

                    Pathing_Spline nextSpline = null;

                    if (currentDirection == TRIGGER_DIRECTION.FORWARDS) //Previously on backside side, entering forwards splines
                    {
                        nextSpline = GetTransferSpline(desiredTurnignDir, m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.FOWARD], m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.FORWARD_RIGHT], m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.FORWARD_LEFT]);
                    }
                    else //Previously on forward side, entering backwards splines
                    {
                        nextSpline = GetTransferSpline(desiredTurnignDir, m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.BACKWARD], m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.BACKWARD_RIGHT], m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.BACKWARD_LEFT]);
                    }

                    if (nextSpline != null)
                        entity.SwapSplines(this, nextSpline);

                    m_activeColliders[entity] = currentDirection;
                }
            }
            else //Not a valid node, so stop movemnt
            {
                float relativeVelocity = Vector3.Dot(transform.forward, entity.transform.forward);

                if (relativeVelocity >= 0.0f) //Moving is same relative space, so positive x velocity is forwards
                {
                    if (previousDirection == TRIGGER_DIRECTION.BACKWARDS) //Was moving from forwards, stop any negitive velocity
                        entity.m_localVelocity.x = Mathf.Min(0.0f, entity.m_localVelocity.x);
                    else
                        entity.m_localVelocity.x = Mathf.Max(0.0f, entity.m_localVelocity.x);
                }
                else //Same as before but flip logic
                {
                    if (previousDirection == TRIGGER_DIRECTION.BACKWARDS)
                        entity.m_localVelocity.x = Mathf.Max(0.0f, entity.m_localVelocity.x);
                    else
                        entity.m_localVelocity.x = Mathf.Min(0.0f, entity.m_localVelocity.x);

                }
            }
        }
    }

    /// <summary>
    /// When an entity first enters a node, add them to the lsit for future checks
    /// </summary>
    /// <param name="p_other">Collider that will be added if its an entity</param>
    private void OnTriggerEnter(Collider p_other)
    {
        Entity collidingEntity = p_other.GetComponent<Entity>();

        if (collidingEntity != null)
        {
            if (m_activeColliders.ContainsKey(collidingEntity)) //Remove to update new state
            {
                m_activeColliders.Remove(collidingEntity);
            }

            //Add in
            m_activeColliders.Add(collidingEntity, GetTriggerDir(collidingEntity.transform.position));
        }
    }

    /// <summary>
    /// Chaarcter has left collider, dont worry about it any more
    /// </summary>
    /// <param name="p_other">Collider of character</param>
    private void OnTriggerExit(Collider p_other)
    {
        Entity collidingEntity = p_other.GetComponent<Entity>();

        if (collidingEntity != null && m_activeColliders.ContainsKey(collidingEntity))
            m_activeColliders.Remove(collidingEntity);
    }

    /// <summary>
    /// Based off location of entity determing if entering or exiting
    /// </summary>
    /// <param name="p_position">Position of entity</param>
    /// <returns>Entering when moving close to trigger forward</returns>
    private TRIGGER_DIRECTION GetTriggerDir(Vector3 p_position)
    {
        Vector3 centerToPos = p_position - transform.position;
        centerToPos.y = 0.0f; //Dont need to worry about y

        centerToPos = centerToPos.normalized;

        return Vector3.Dot(m_planeForwardVector, centerToPos) >= 0.0f ? TRIGGER_DIRECTION.FORWARDS : TRIGGER_DIRECTION.BACKWARDS;
    }

    /// <summary>
    /// Get the best option for next spline based off desired direction
    /// </summary>
    /// <param name="p_desireDirection">Desired direction to move</param>
    /// <param name="p_center">The center spline</param>
    /// <param name="p_right">The right spline</param>
    /// <param name="p_left">The left spline</param>
    /// <returns>Best option spline</returns>
    private Pathing_Spline GetTransferSpline(Entity.TURNING_DIR p_desireDirection, Pathing_Spline p_center, Pathing_Spline p_right, Pathing_Spline p_left)
    {
        //Try get desired
        if (p_desireDirection == Entity.TURNING_DIR.CENTER && p_center != null)
            return p_center;
        if (p_desireDirection == Entity.TURNING_DIR.RIGHT && p_right != null)
            return p_right;
        if (p_desireDirection == Entity.TURNING_DIR.LEFT && p_left != null)
            return p_left;

        //Get defaults
        if (p_center != null)
            return p_center;
        if (p_right != null)
            return p_right;
        return p_left;
    }
    /// <summary>
    /// Valid node when theres at least one forward spline and one backwards spline
    /// </summary>
    /// <returns>true when above is valid</returns>
    public bool ValidNode()
    {
        return (m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.FOWARD] != null || m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.FORWARD_RIGHT] != null || (m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.FORWARD_LEFT] != null)
            && (m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.BACKWARD] != null || m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.BACKWARD_RIGHT] != null|| m_pathingSplines[(int)Pathing_Spline.SPLINE_POSITION.BACKWARD_LEFT] != null));
    }

    /// <summary>
    /// Given a conjoined node, get its position in this node
    /// </summary>
    /// <param name="p_node">Node to look for</param>
    /// <returns>Node position in the spline details, otherwise default to MAX_LENGTH</returns>
    public Pathing_Spline.SPLINE_POSITION DetermineNodePosition(Pathing_Node p_node)
    {
        for (int splineIndex = 0; splineIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineIndex++)
        {
            if (m_pathingSplineDetails[splineIndex].m_nodePrimary == p_node || m_pathingSplineDetails[splineIndex].m_nodeSecondary == p_node)
                return (Pathing_Spline.SPLINE_POSITION)splineIndex;
        }

        return Pathing_Spline.SPLINE_POSITION.MAX_LENGTH;
    }

    /// <summary>
    /// Does this node, also have link to coinjoined node
    /// That is any of the ndoe details has this node
    /// </summary>
    /// <param name="p_node">Ndoe ot test against</param>
    /// <returns>true when any spline details contains this p_conjoinedNode</returns>
    public bool ContainsConjoinedNode(Pathing_Node p_node)
    {
        for (int splineIndex = 0; splineIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineIndex++)
        {
            if (m_pathingSplineDetails[splineIndex].m_nodePrimary == p_node || m_pathingSplineDetails[splineIndex].m_nodeSecondary == p_node)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Does this pathing node, contain a link to a given spline
    /// That is, is any spline in m_pathingSplines the given spline
    /// </summary>
    /// <param name="p_spline">Spline to test for</param>
    /// <returns>True when theres at least one instance of the given spline</returns>
    public bool ContainsSpline(Pathing_Spline p_spline)
    {
        if (p_spline == null)
            return false;

        for (int splineIndex = 0; splineIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineIndex++)
        {
            if (m_pathingSplines[splineIndex] == p_spline)
                return true;
        }

        return false;
    }

    #region Editor Specific
    private const int SPLINE_STEPS = 10;

    private void OnDrawGizmos()
    {
        for (int splineIndex = 0; splineIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineIndex++)
        {
            DrawSpline(m_pathingSplineDetails[splineIndex]);
        }
    }

    /// <summary>
    /// Draw a spline
    /// </summary>
    /// <param name="p_splineDetails">Details describing the spline</param>
    private void DrawSpline(Spline_Details p_splineDetails)
    {
        if (!p_splineDetails.IsValidSpline()) //Validate Data
            return;

        Gizmos.color = Color.blue;

        //Setup spline variables
        float length = 0.0f;

        //Straight
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

        switch(p_splineDetails.m_splineType)
        {
            case Pathing_Spline.SPLINE_TYPE.STRAIGHT:
                Pathing_Spline.RebuildStraight(p_splineDetails.m_nodePrimary, p_splineDetails.m_nodeSecondary, out straightDir, out length);
                break;
            case Pathing_Spline.SPLINE_TYPE.BEZIER:
                Pathing_Spline.RebuildBezier(p_splineDetails.m_nodePrimary, p_splineDetails.m_nodeSecondary, p_splineDetails.m_bezierStrength, out bezierPointA, out bezierPointB, out bezierControlA, out bezierControlB, out length);
                break;
            case Pathing_Spline.SPLINE_TYPE.CIRCLE:
                Pathing_Spline.RebuildCircle(p_splineDetails.m_nodePrimary, p_splineDetails.m_nodeSecondary, p_splineDetails.m_circleDir, p_splineDetails.m_circleAngle, out circleHeight, out circleCenter, out centerADir, out length);
                break;
        }
        
        float percentStep = 1.0f / SPLINE_STEPS;
        float currentPercent = percentStep;
        
        Vector3 previous = p_splineDetails.m_nodePrimary.transform.position;
        Vector3 next = Vector3.zero; 

        //Loop through approximating circle, every (m_totalDegrees / DEBUG_STEPS) degrees
        for (int i = 1; i <= SPLINE_STEPS; i++)
        {
            switch (p_splineDetails.m_splineType)
            {
                case Pathing_Spline.SPLINE_TYPE.STRAIGHT:
                    next = Pathing_Spline.GetStraightPosition(currentPercent, p_splineDetails.m_nodePrimary, straightDir);
                    break;
                case Pathing_Spline.SPLINE_TYPE.BEZIER:
                    next = Pathing_Spline.GetBezierPosition(currentPercent, bezierPointA, bezierPointB, bezierControlA, bezierControlB);
                    break;
                case Pathing_Spline.SPLINE_TYPE.CIRCLE:
                    next = Pathing_Spline.GetCirclePosition(currentPercent, p_splineDetails.m_circleDir, p_splineDetails.m_circleAngle, circleHeight, circleCenter, centerADir);
                    break;
            }

            Gizmos.DrawLine(previous, next);

            previous = next;
            currentPercent += percentStep;
        }
    }
    #endregion
}
