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

    /// <summary>
    /// Setup colision plane
    /// </summary>
    private void Start()
    {
    }

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

        //Manage transfering entities
        foreach (KeyValuePair<Entity, TRIGGER_DIRECTION> entityPair in m_activeColliders)
        {
            Entity entity = entityPair.Key;

            TRIGGER_DIRECTION previousDirection = entityPair.Value;
            TRIGGER_DIRECTION currentDir = GetTriggerDir(entity);

            if(currentDir != previousDirection)//Moved to other side
            {
                Entity.TURNING_DIR desiredTurnignDir = entity.GetDesiredTurning(this);
                if (currentDir == TRIGGER_DIRECTION.FORWARDS) //Deal with forward splines
                {
                    entity.SwapSplines(GetTransferSpline(desiredTurnignDir, m_forwardSpline, m_forwardRightSpline, m_forwardLeftSpline));
                }
                else //Exiting, deal with backward splines
                {
                    entity.SwapSplines(GetTransferSpline(desiredTurnignDir, m_backwardSpline, m_backwardRightSpline, m_backwardLeftSpline));
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
            m_activeColliders.Add(collidingEntity, GetTriggerDir(collidingEntity));
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
    /// <param name="p_entity">Entity to compare with</param>
    /// <returns>Entering when moving close to trigger forward</returns>
    private TRIGGER_DIRECTION GetTriggerDir(Entity p_entity)
    {
        Vector3 entityTriggerDir = transform.position - p_entity.transform.position;

        //if direction is the sames, that is dot is positive, means entity is on back side
        return Vector3.Dot(entityTriggerDir.normalized, transform.forward) >= 0.0f ? TRIGGER_DIRECTION.BACKWARDS : TRIGGER_DIRECTION.FORWARDS;
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
            return p_center;
        if (p_desireDirection == Entity.TURNING_DIR.LEFT && p_left != null)
            return p_center;

        //Get defaults
        if (p_center != null)
            return p_center;
        if (p_right != null)
            return p_right;
        return p_left;
    }
}
