using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class Navigation_Trigger : MonoBehaviour
{
    [System.Serializable]
    public struct SplineInfo
    {
        public Navigation_Spline m_spline;
        public float m_splinePercent;
    }

    //Determines the plane of collision
    protected Vector3 m_globalEntranceVector = Vector3.zero;
    protected Vector4 m_planeEquation = Vector4.zero;

    public List<Navigation_Spline> m_adjacentSplines = new List<Navigation_Spline>();

    [HideInInspector]
    public Dictionary<Collider, TRIGGER_DIRECTION> m_activeColliders = new Dictionary<Collider, TRIGGER_DIRECTION>();

    /// <summary>
    /// Setup colision plane
    /// </summary>
    protected virtual void Start()
    {
        m_globalEntranceVector = transform.forward;

        Vector3 triggerPos = transform.position;
        float planeZ = (-triggerPos.x * m_globalEntranceVector.x) + (-triggerPos.y * m_globalEntranceVector.y) + (-triggerPos.z * m_globalEntranceVector.z);

        m_planeEquation = new Vector4(m_globalEntranceVector.x, m_globalEntranceVector.y, m_globalEntranceVector.z, planeZ);

        m_adjacentSplines = new List<Navigation_Spline>(); //Clear due to running in editor
    }

    /// <summary>
    /// Character is moving through the collider itself
    /// Determine exact moment of moving through using plane formula
    /// ax + by + cz + d where a,b,c,d are determiened from the plane equation
    /// 
    /// Change behaviour based on entering/exiting
    /// </summary>
    /// <param name="p_other">Collider of character</param>
    protected void OnTriggerStay(Collider p_other)
    {
        //Compare dot products of character and entrance vector to see if character is entering or leaving
        Entity collidingEntity = p_other.GetComponent<Entity>();
        if (collidingEntity != null)
        {
            //Determine what side of the plane the character is
            Vector3 characterPosition = collidingEntity.transform.position;
            float expandedDot = m_planeEquation.x * characterPosition.x + m_planeEquation.y * characterPosition.y + m_planeEquation.z * characterPosition.z + m_planeEquation.w;

            TRIGGER_DIRECTION triggerDirection = expandedDot >= 0.0f ? TRIGGER_DIRECTION.ENTERING : TRIGGER_DIRECTION.EXITING;

            if(m_activeColliders.ContainsKey(p_other))//Already entered
            {
                if(m_activeColliders[p_other] != triggerDirection) //Updating dir
                {
                    m_activeColliders[p_other] = triggerDirection;
                    HandleTrigger(collidingEntity, triggerDirection);
                }
            }
            else//New entry
            {
                m_activeColliders.Add(p_other, triggerDirection);
                HandleTrigger(collidingEntity, triggerDirection);
            }
        }
    }

    /// <summary>
    /// Chaarcter has left collider, dont worry about it any more
    /// </summary>
    /// <param name="p_other">Collider of character</param>
    protected void OnTriggerExit(Collider p_other)
    {
        if (m_activeColliders.ContainsKey(p_other))
            m_activeColliders.Remove(p_other);
    }


    protected void SwapSplines(Entity p_entity, Navigation_Spline p_newSpline, float p_newSplinePercent)
    {
        p_entity.m_splinePhysics.m_currentSpline = p_newSpline;
        p_entity.m_splinePhysics.m_currentSplinePercent = p_newSplinePercent;
    }

    public enum TRIGGER_DIRECTION {ENTERING, EXITING }
    protected virtual void HandleTrigger(Entity p_entity, TRIGGER_DIRECTION p_direction)
    {

    }

    public virtual void UpdateCollidier()
    {
    }

    public virtual bool ContainsSpine(Navigation_Spline p_spline)
    {
        return false;
    }

    public virtual bool HasForwardSpline()
    {
        return false;
    }

    public virtual bool HasBackwardsSpline()
    {
        return false;
    }
}