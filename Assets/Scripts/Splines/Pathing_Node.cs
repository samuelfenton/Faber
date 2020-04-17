using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Pathing_Node : MonoBehaviour
{
    public enum TRIGGER_DIRECTION { FORWARDS, BACKWARDS }

    public Pathing_Spline m_forwardSpline = null;
    public Pathing_Spline m_forwardRightSpline = null;
    public Pathing_Spline m_forwardLeftSpline = null;
    public Pathing_Spline m_backwardSpline = null;
    public Pathing_Spline m_backwardRightSpline = null;
    public Pathing_Spline m_backwardLeftSpline = null;

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

        if (m_forwardSpline != null)
            m_adjacentSplines.Add(m_forwardSpline);
        if (m_forwardRightSpline != null)
            m_adjacentSplines.Add(m_forwardRightSpline);
        if (m_forwardLeftSpline != null)
            m_adjacentSplines.Add(m_forwardLeftSpline);
        if (m_backwardSpline != null)
            m_adjacentSplines.Add(m_backwardSpline);
        if (m_backwardRightSpline != null)
            m_adjacentSplines.Add(m_backwardRightSpline);
        if (m_backwardLeftSpline != null)
            m_adjacentSplines.Add(m_backwardLeftSpline);
    }

    /// <summary>
    /// Character is moving through the collider itself
    /// Determine exact moment of moving through using plane formula
    /// ax + by + cz + d where a,b,c,d are determiened from the plane equation
    /// </summary>
    private void Update() 
    {
#if UNITY_EDITOR
        if (m_forwardSpline != null)
            m_forwardSpline.UpdatePosition();
        if (m_forwardRightSpline != null)
            m_forwardRightSpline.UpdatePosition();
        if (m_forwardLeftSpline != null)
            m_forwardLeftSpline.UpdatePosition();
        if (m_backwardSpline != null)
            m_backwardSpline.UpdatePosition();
        if (m_backwardRightSpline != null)
            m_backwardRightSpline.UpdatePosition();
        if (m_backwardLeftSpline != null)
            m_backwardLeftSpline.UpdatePosition();
#endif
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

                    if (currentDirection == TRIGGER_DIRECTION.FORWARDS) //Previously on backside side, entering forwards splines
                    {
                        entity.SwapSplines(this, GetTransferSpline(desiredTurnignDir, m_forwardSpline, m_forwardRightSpline, m_forwardLeftSpline));
                    }
                    else //Previously on forward side, entering backwards splines
                    {
                        entity.SwapSplines(this, GetTransferSpline(desiredTurnignDir, m_backwardSpline, m_backwardRightSpline, m_backwardLeftSpline));
                    }

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

        if(collidingEntity!= null)
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
        if(p_desireDirection == Entity.TURNING_DIR.CENTER && p_center!=null)
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
        return (m_forwardSpline != null || m_forwardRightSpline != null || m_forwardLeftSpline != null) && (m_backwardSpline != null || m_backwardRightSpline != null || m_backwardLeftSpline != null);
    }
}
