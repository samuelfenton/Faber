using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class SplinePhysics : MonoBehaviour
{
    public const float MIN_SPLINE_PERCENT = -0.001f;
    public const float MAX_SPLINE_PERCENT = 1.001f;

    public const float DETECITON_RANGE = 0.1f;
    public const float COLLISION_OFFSET_MODIFIER = 0.98f;
    public const float COLLISION_OFFSET_MODIFIER_HALF = COLLISION_OFFSET_MODIFIER/2.0f;

    public const float GRAVITY = -24.0f;

    [Header("Spline settings")]
    public Pathing_Node m_nodeA = null;
    public Pathing_Node m_nodeB = null;

    [Range(0, 1)]
    public float m_currentSplinePercent = 0.0f;

    [HideInInspector]
    public Pathing_Spline m_currentSpline = null;

    [Header("Generated values")]
    public Vector2 m_splineVelocity = Vector2.zero;

    public bool m_upCollision = false;
    public bool m_downCollision = false;
    public bool m_forwardCollision = false;
    public bool m_backCollision = false;

    protected Entity m_parentEntity = null;

    protected BoxCollider m_boxCollider = null;
    protected Rigidbody m_rigidBody = null;

    public virtual void Init()
    {
        m_parentEntity = GetComponent<Entity>();

        m_boxCollider = GetComponent<BoxCollider>();
        m_rigidBody = GetComponent<Rigidbody>();

        if (m_parentEntity == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(name + " has no entity attached, considering removing the spline physcis, or add one");
#endif
            return;
        }

        if (m_nodeA == null || m_nodeB == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(name + " doesnt have both nodes set in the inspector");
#endif
            return;
        }

        Pathing_Spline.SPLINE_POSITION splinePosition = m_nodeA.DetermineNodePosition(m_nodeB);

        if (splinePosition == Pathing_Spline.SPLINE_POSITION.MAX_LENGTH)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(name + " nodes are invalid");
#endif
            return;
        }

        m_currentSpline = m_nodeA.m_pathingSplines[(int)splinePosition];

        if (m_currentSpline == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(name + " spline is invalid");
#endif
            return;
        }

        transform.position = m_currentSpline.GetPosition(m_currentSplinePercent);
    }

    /// <summary>
    /// Updates characters physics
    /// Set rotation to be based off spline position
    /// Clamp on spline
    /// Check for ground collisions
    /// </summary>
    public void ResolvePhysics()
    {
        //Apply Gravity
        if (m_parentEntity.m_gravity)
            m_splineVelocity.y += GRAVITY * Time.deltaTime;

        UpdateCollisions();
        
        //Apply horizontal change
        if (m_parentEntity.AllignedToSpline())
        {
            Vector3 desiredForwards = m_currentSpline.GetForwardDir(m_currentSplinePercent);
            transform.rotation = Quaternion.LookRotation(desiredForwards, Vector3.up);

            m_currentSplinePercent += m_currentSpline.ChangeInPercent(m_splineVelocity.x * Time.deltaTime);
        }
        else
        {
            Vector3 desiredForwards = Quaternion.Euler(0.0f, 180.0f, 0.0f) * m_currentSpline.GetForwardDir(m_currentSplinePercent);
            transform.rotation = Quaternion.LookRotation(desiredForwards, Vector3.up);

            m_currentSplinePercent -= m_currentSpline.ChangeInPercent(m_splineVelocity.x * Time.deltaTime);
        }

        //Apply vertical change
        Vector3 currentPosition = transform.position;
        currentPosition.y += m_splineVelocity.y * Time.deltaTime;
        transform.position = currentPosition;


        //Setup transform
        //Lock spline percent between close enough to 0 - 1
        m_currentSplinePercent = Mathf.Clamp(m_currentSplinePercent, MIN_SPLINE_PERCENT, MAX_SPLINE_PERCENT);

        //Update to latest spline percentage
        currentPosition = m_currentSpline.GetPosition(m_currentSplinePercent);
        currentPosition.y = transform.position.y; //Keep y value as spline position ignores this

        transform.position = currentPosition;
    }

    /// <summary>
    /// Check for collisions in horizontal and vertical axis
    /// </summary>
    public void UpdateCollisions()
    {
        Vector3 forward = transform.forward;
        Vector3 up = transform.up;

        Vector3 centerPos = transform.position + forward * m_boxCollider.center.z + up * m_boxCollider.center.y;

        //UPWARDS
        m_upCollision = CastCollision(up, centerPos, forward * m_boxCollider.bounds.extents.z, m_boxCollider.bounds.extents.y + DETECITON_RANGE);

        if (m_upCollision)//Check Upwards
        {
            if (m_splineVelocity.y > 0.0f)//Moving up, stop this 
                m_splineVelocity.y = 0.0f;
        }

        //DOWNWARDS
        m_downCollision = CastCollision(-up, centerPos, forward * m_boxCollider.bounds.extents.z, m_boxCollider.bounds.extents.y + DETECITON_RANGE);

        if (!m_downCollision)
        {
            float splineOverlap = m_currentSpline.GetPosition(m_currentSplinePercent).y - transform.position.y;
            if (splineOverlap >= 0.0f)
                m_downCollision = true;
        }

        if (m_downCollision)//Downwards
        {
            if (m_splineVelocity.y < 0.0f)//Moving up, stop this 
                m_splineVelocity.y = 0.0f;
        }

        //FOWARDS
        m_forwardCollision = CastCollision(forward, centerPos, up * m_boxCollider.bounds.extents.y, m_boxCollider.bounds.extents.z + DETECITON_RANGE);
        if (m_forwardCollision)//Forwards
        {
            if (m_splineVelocity.x > 0.0f)//Moving up, stop this 
                m_splineVelocity.x = 0.0f;
        }

        //BACKWARDS
        m_backCollision = CastCollision(-forward, centerPos, up * m_boxCollider.bounds.extents.y, m_boxCollider.bounds.extents.z + DETECITON_RANGE);
        if (m_backCollision)//Backwards
        {
            if (m_splineVelocity.x < 0.0f)//Moving up, stop this 
                m_splineVelocity.x = 0.0f;
        }
    }

    /// <summary>
    /// Check for collisions vertically
    /// Creates three raycasts, forward, forward center, center, back center, back
    /// </summary>
    /// <param name="p_direction">Casting up or down</param>
    /// <param name="p_centerPos">What is current center position of charater</param>
    /// <param name="p_castFromModifer">Modifer from center cast to offset most</param>
    /// <param name="p_boundingDistance">Distance to edge of collider bounds</param>
    /// <returns>True when any collisions occur</returns>
    private bool CastCollision(Vector3 p_direction, Vector3 p_centerPos, Vector3 p_castFromModifer, float p_boundingDistance)
    {
        Vector3 castFromModifier = p_castFromModifer * COLLISION_OFFSET_MODIFIER;
        Vector3 castFromModifierHalf = p_castFromModifer * COLLISION_OFFSET_MODIFIER_HALF;

        //Large Offset raycast
        if (Physics.Raycast(p_centerPos + castFromModifier, p_direction, p_boundingDistance, CustomLayers.m_enviroment))
            return true;

        //Forward Center raycast
        if (Physics.Raycast(p_centerPos + castFromModifierHalf, p_direction, p_boundingDistance, CustomLayers.m_enviroment))
            return true;


        //Center raycast
        if (Physics.Raycast(p_centerPos, p_direction, p_boundingDistance, CustomLayers.m_enviroment))
            return true;


        //Back Center raycast
        if (Physics.Raycast(p_centerPos - castFromModifierHalf, p_direction, p_boundingDistance, CustomLayers.m_enviroment))
            return true;


        //Back raycast
        if (Physics.Raycast(p_centerPos - castFromModifier, p_direction, p_boundingDistance, CustomLayers.m_enviroment))
            return true;


        return false;
    }

    /// <summary>
    /// Swap facing direction
    /// </summary>
    public virtual void SwapFacingDirection()
    {
        if (m_parentEntity.AllignedToSpline()) //Currently alligned, so face backwards
        {
            Vector3 desiredForwards = m_currentSpline.GetForwardDir(m_currentSplinePercent);
            transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f) * Quaternion.LookRotation(desiredForwards, Vector3.up);
        }
        else
        {
            Vector3 desiredForwards = m_currentSpline.GetForwardDir(m_currentSplinePercent);
            transform.rotation = Quaternion.LookRotation(desiredForwards, Vector3.up);
        }
    }

    /// <summary>
    /// Hard set the value of velocity
    /// </summary>
    /// <param name="p_val">velocity</param>
    public void HardSetVelocity(float p_val)
    {
        m_splineVelocity.x = p_val;
    }

    /// <summary>
    /// Hard set the value of velocity
    /// </summary>
    /// <param name="p_val">velocity</param>
    public void HardSetUpwardsVelocity(float p_val)
    {
        m_splineVelocity.y = p_val;
    }
}
