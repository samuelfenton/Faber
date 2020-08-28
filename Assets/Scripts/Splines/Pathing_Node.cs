using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[ExecuteAlways]
public class Pathing_Node : MonoBehaviour
{
    [System.Serializable]
    public struct Spline_Details
    {
        public Pathing_Spline.SPLINE_POSITION m_splinePosition;

        public Pathing_Node m_conjoinedNode;

        public Pathing_Spline.SPLINE_TYPE m_splineType;

        [Header("Circle values")]
        public Pathing_Spline.CIRCLE_DIR m_circleDir;
        public float m_circleAngle;

        [Header("Bezier Settings")]
        public float m_bezierStrength;

        /// <summary>
        /// Pass details form one node to another
        /// </summary>
        /// <param name="p_otherSplinePosition">What position is this spline on other node</param>
        /// <param name="p_details">Spline details from copied node</param>
        public void CopyDetails(Pathing_Spline.SPLINE_POSITION p_otherSplinePosition, Spline_Details p_details)
        {
            m_splineType = p_details.m_splineType;

            //Determine Dir 
            bool isForwardNode = m_splinePosition == Pathing_Spline.SPLINE_POSITION.FOWARD || m_splinePosition == Pathing_Spline.SPLINE_POSITION.FORWARD_RIGHT || m_splinePosition == Pathing_Spline.SPLINE_POSITION.FORWARD_LEFT;
            bool isOtherForwardNode = p_otherSplinePosition == Pathing_Spline.SPLINE_POSITION.FOWARD || p_otherSplinePosition == Pathing_Spline.SPLINE_POSITION.FORWARD_RIGHT || p_otherSplinePosition == Pathing_Spline.SPLINE_POSITION.FORWARD_LEFT;

            if (isForwardNode != isOtherForwardNode) //Spline positions are opposite, same circleDir
            {
                m_circleDir = p_details.m_circleDir;
            }
            else
            {
                m_circleDir = p_details.m_circleDir == Pathing_Spline.CIRCLE_DIR.CLOCKWISE ? Pathing_Spline.CIRCLE_DIR.COUNTER_CLOCKWISE : Pathing_Spline.CIRCLE_DIR.CLOCKWISE;
            }

            m_circleAngle = p_details.m_circleAngle;
            m_bezierStrength = p_details.m_bezierStrength;
        }

        /// <summary>
        /// Does the current spline have both 
        /// </summary>
        /// <param name="p_currentNode"></param>
        /// <returns></returns>
        public bool IsValidSpline(Pathing_Node p_currentNode)
        {
            return m_conjoinedNode != null && m_conjoinedNode.ContainsConjoinedNode(p_currentNode);
        }
    };

    public enum TRIGGER_DIRECTION { FORWARDS, BACKWARDS }

    [SerializeField]
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

    #region Spline building stuff

    private void OnValidate()
    {
        VerifySplineConnections();
    }

    private void VerifySplineConnections()
    {
        for (int splineDetailsIndex = 0; splineDetailsIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineDetailsIndex++)
        {
            Spline_Details currentDetails = m_pathingSplineDetails[splineDetailsIndex];

            if (currentDetails.IsValidSpline(this))
            {
                Pathing_Node conjoinedNode = currentDetails.m_conjoinedNode;

                Pathing_Spline.SPLINE_POSITION nodeBPosition = conjoinedNode.DetermineNodePosition(this);

                conjoinedNode.m_pathingSplineDetails[(int)nodeBPosition].CopyDetails((Pathing_Spline.SPLINE_POSITION)splineDetailsIndex, currentDetails);

                CreateSpline(currentDetails, (Pathing_Spline.SPLINE_POSITION)splineDetailsIndex);
            }
            else if (m_pathingSplines[splineDetailsIndex] != null) //Invalid setup, but spline exists
            {
                m_pathingSplines[splineDetailsIndex].SplineRemoved();
            }
        }

        //Add to adjacent list
        m_adjacentSplines.Clear();
        for (int splineIndex = 0; splineIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineIndex++)
        {
            if (m_pathingSplines[splineIndex] != null)
            {
                m_adjacentSplines.Add(m_pathingSplines[splineIndex]);
            }
        }
    }

    private bool CreateSpline(Spline_Details p_relavantSplineDetails, Pathing_Spline.SPLINE_POSITION p_desiredPosition)
    {
        if (p_relavantSplineDetails.IsValidSpline(this))//Vaild spline details, attempt to make spline and ensure is valid
        {
            if(m_pathingSplines[(int)p_desiredPosition] != null)
            {
                DestroyImmediate(m_pathingSplines[(int)p_desiredPosition]);
            }

            Pathing_Spline newPathingSpline = ScriptableObject.CreateInstance("Pathing_Spline") as Pathing_Spline;

            Pathing_Node nodeA = this;
            Pathing_Node nodeB = p_relavantSplineDetails.m_conjoinedNode;

            Pathing_Spline.SPLINE_POSITION nodeAPosition = DetermineNodePosition(nodeB);
            Pathing_Spline.SPLINE_POSITION nodeBPosition = nodeB.DetermineNodePosition(nodeA);

            newPathingSpline.InitVaribles(nodeA, nodeAPosition, nodeB, nodeBPosition, p_relavantSplineDetails.m_splineType, p_relavantSplineDetails.m_circleDir, p_relavantSplineDetails.m_circleAngle, p_relavantSplineDetails.m_bezierStrength);

            m_pathingSplines[(int)p_desiredPosition] = newPathingSpline;
            nodeB.AssignSpline(newPathingSpline, nodeBPosition);

            return newPathingSpline;
        }


        return false;

    }

    public Pathing_Spline.SPLINE_POSITION DetermineNodePosition(Pathing_Node p_node)
    {
        for (int splineIndex = 0; splineIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineIndex++)
        {
            if (m_pathingSplineDetails[splineIndex].m_conjoinedNode == p_node)
                return (Pathing_Spline.SPLINE_POSITION)splineIndex;
        }

        return Pathing_Spline.SPLINE_POSITION.MAX_LENGTH;
    }

    public bool ContainsConjoinedNode(Pathing_Node p_conjoinedNode)
    {
        for (int splineIndex = 0; splineIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineIndex++)
        {
            if(m_pathingSplineDetails[splineIndex].m_conjoinedNode == p_conjoinedNode)
            {
                return true;
            }
        }
        return false;
    }

    public void AssignSpline(Pathing_Spline p_spline, Pathing_Spline.SPLINE_POSITION p_position)
    {
        if (p_position != Pathing_Spline.SPLINE_POSITION.MAX_LENGTH && p_spline != null)
        {
            m_pathingSplines[(int)p_position] = p_spline;
        }
    }

    #region Editor Specific
    #if UNITY_EDITOR
    private const int SPLINE_STEPS = 10;

    private void OnDrawGizmos()
    {
        for (int splineIndex = 0; splineIndex < (int)Pathing_Spline.SPLINE_POSITION.MAX_LENGTH; splineIndex++)
        {
            if (m_pathingSplines[splineIndex] != null)
                DrawSpline(splineIndex);
        }
    }

    /// <summary>
    /// Draw a spline
    /// </summary>
    /// <param name="p_pathingSplineIndex">Index of spline to draw</param>
    private void DrawSpline(int p_pathingSplineIndex)
    {
        if (!m_pathingSplineDetails[p_pathingSplineIndex].IsValidSpline(this)) //Invalid setup, but spline exists
        {
            m_pathingSplines[p_pathingSplineIndex].SplineRemoved();

            return;
        }

        Pathing_Spline splineToDraw = m_pathingSplines[p_pathingSplineIndex];

        Gizmos.color = Color.blue;

        float percentStep = 1.0f / SPLINE_STEPS;
        float currentPercent = percentStep;

        Vector3 previous = splineToDraw.m_nodeA.transform.position;

        //Loop through approximating circle, every (m_totalDegrees / DEBUG_STEPS) degrees
        for (int i = 1; i <= SPLINE_STEPS; i++)
        {
            Vector3 next = splineToDraw.GetPosition(currentPercent);

            Gizmos.DrawLine(previous, next);

            previous = next;
            currentPercent += percentStep;
        }
    }
#endif
    #endregion

    #endregion
}
