using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
[ExecuteAlways]
public class SplinePhysics : MonoBehaviour
{
    public enum COLLISION_TYPE {ENVIROMENT, HURT_BOX }

    public const float MIN_SPLINE_PERCENT = -0.001f;
    public const float MAX_SPLINE_PERCENT = 1.001f;

    public const float DETECTION_RANGE = 0.2f;
    public const float COLLISION_OFFSET_MODIFIER = 0.95f;
    public const float COLLISION_OFFSET_MODIFIER_HALF = COLLISION_OFFSET_MODIFIER/2.0f;

    public const float GRAVITY = -24.0f;

    [Header("Physics settings")]
    public bool m_gravity = true;

    [Header("Spline settings")]
    public Pathing_Node m_nodeA = null;
    public Pathing_Node m_nodeB = null;

    [Range(0, 1)]
    public float m_currentSplinePercent = 0.0f;

    [HideInInspector]
    public Pathing_Spline m_currentSpline = null;

    [Header("Generated values")]
    public Vector2 m_splineLocalVelocity = Vector2.zero;

    [HideInInspector]
    public bool m_upCollision = false;
    [HideInInspector]
    public bool m_downCollision = false;
    [HideInInspector]
    public bool m_forwardCollision = false;
    [HideInInspector]
    public bool m_backCollision = false;

    [HideInInspector]
    public COLLISION_TYPE m_upCollisionType = COLLISION_TYPE.ENVIROMENT;
    [HideInInspector]
    public COLLISION_TYPE m_downCollisionType = COLLISION_TYPE.ENVIROMENT;
    [HideInInspector]
    public COLLISION_TYPE m_forwardCollisionType = COLLISION_TYPE.ENVIROMENT;
    [HideInInspector]
    public COLLISION_TYPE m_backCollisionType = COLLISION_TYPE.ENVIROMENT;

    protected Entity m_parentEntity = null;

    protected BoxCollider m_boxCollider = null;
    protected Rigidbody m_rigidBody = null;

    public bool m_showDebugColliders = false;

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
        if (m_gravity)
            m_splineLocalVelocity.y += GRAVITY * Time.deltaTime;

        UpdateCollisions();

        //Apply horizontal change
        if (m_parentEntity.AllignedToSpline())
        {
            Vector3 desiredForwards = m_currentSpline.GetForwardDir(m_currentSplinePercent);
            transform.rotation = Quaternion.LookRotation(desiredForwards, Vector3.up);

            m_currentSplinePercent += m_currentSpline.ChangeInPercent(m_splineLocalVelocity.x * Time.deltaTime);
        }
        else
        {
            Vector3 desiredForwards = Quaternion.Euler(0.0f, 180.0f, 0.0f) * m_currentSpline.GetForwardDir(m_currentSplinePercent);
            transform.rotation = Quaternion.LookRotation(desiredForwards, Vector3.up);

            m_currentSplinePercent -= m_currentSpline.ChangeInPercent(m_splineLocalVelocity.x * Time.deltaTime);
        }

        Vector3 newPosition = transform.position;
        
        //Apply vertical change
        newPosition.y += m_splineLocalVelocity.y * Time.deltaTime;
        
        //Setup transform
        //Lock spline percent between close enough to 0 - 1
        m_currentSplinePercent = Mathf.Clamp(m_currentSplinePercent, MIN_SPLINE_PERCENT, MAX_SPLINE_PERCENT);

        //Update to latest spline percentage
        Vector3 splinePosition = m_currentSpline.GetPosition(m_currentSplinePercent);
        if(splinePosition.y > newPosition.y || (m_downCollision && m_splineLocalVelocity.y <= 0.0f))
            newPosition = splinePosition; //Keep y value as spline position ignores this
        else
        {
            newPosition.x = splinePosition.x;
            newPosition.z = splinePosition.z;
        }

        transform.position = newPosition;
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
        m_upCollision = CastCollision(up, centerPos, forward * m_boxCollider.bounds.extents.z, m_boxCollider.bounds.extents.y + DETECTION_RANGE, out m_upCollisionType);

        if (m_upCollision)//Check Upwards
        {
            if (m_splineLocalVelocity.y > 0.0f)//Moving up, stop this 
                m_splineLocalVelocity.y = 0.0f;
        }

        //DOWNWARDS
        m_downCollision = CastCollision(-up, centerPos, forward * m_boxCollider.bounds.extents.z, m_boxCollider.bounds.extents.y + DETECTION_RANGE, out m_downCollisionType);

        if (!m_downCollision) //Check if colliding with spline
        {
            float splineOverlap = m_currentSpline.GetPosition(m_currentSplinePercent).y - transform.position.y;
            if (splineOverlap >= -DETECTION_RANGE)
                m_downCollision = true;
        }

        if (m_downCollision)//Downwards
        {
            if (m_splineLocalVelocity.y < 0.0f)//Moving up, stop this 
                m_splineLocalVelocity.y = 0.0f;
        }

        //FOWARDS
        m_forwardCollision = CastCollision(forward, centerPos, up * m_boxCollider.bounds.extents.y, m_boxCollider.bounds.extents.z + DETECTION_RANGE, out m_forwardCollisionType);
        if (m_forwardCollision)//Forwards
        {
            if (m_splineLocalVelocity.x > 0.0f)//Moving up, stop this 
                m_splineLocalVelocity.x = 0.0f;
        }

        //BACKWARDS
        m_backCollision = CastCollision(-forward, centerPos, up * m_boxCollider.bounds.extents.y, m_boxCollider.bounds.extents.z + DETECTION_RANGE, out m_backCollisionType);
        if (m_backCollision)//Backwards
        {
            if (m_splineLocalVelocity.x < 0.0f)//Moving up, stop this 
                m_splineLocalVelocity.x = 0.0f;
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
    private bool CastCollision(Vector3 p_direction, Vector3 p_centerPos, Vector3 p_castFromModifer, float p_boundingDistance, out COLLISION_TYPE p_collisionType)
    {
        Vector3 castFromModifier = p_castFromModifer * COLLISION_OFFSET_MODIFIER;
        Vector3 castFromModifierHalf = p_castFromModifer * COLLISION_OFFSET_MODIFIER_HALF;

        RaycastHit hit;

        //Forward Offset raycast
        if (Physics.Raycast(p_centerPos + castFromModifier, p_direction, out hit, p_boundingDistance, CustomLayers.m_enviromentMask | CustomLayers.m_hurtBoxMask))
        {
            p_collisionType = hit.collider.gameObject.layer == CustomLayers.m_enviromentLayer ? COLLISION_TYPE.ENVIROMENT : COLLISION_TYPE.HURT_BOX;//Use layer not mask, when comparing to game object layer
            return true;
        }

        //Forward Center raycast
        if (Physics.Raycast(p_centerPos + castFromModifierHalf, p_direction, out hit, p_boundingDistance, CustomLayers.m_enviromentMask | CustomLayers.m_hurtBoxMask))
        {
            p_collisionType = hit.collider.gameObject.layer == CustomLayers.m_enviromentLayer ? COLLISION_TYPE.ENVIROMENT : COLLISION_TYPE.HURT_BOX;//Use layer not mask, when comparing to game object layer
            return true;
        }

        //Center raycast
        if (Physics.Raycast(p_centerPos, p_direction, out hit, p_boundingDistance, CustomLayers.m_enviromentMask | CustomLayers.m_hurtBoxMask))
        {
            p_collisionType = hit.collider.gameObject.layer == CustomLayers.m_enviromentLayer ? COLLISION_TYPE.ENVIROMENT : COLLISION_TYPE.HURT_BOX;//Use layer not mask, when comparing to game object layer
            return true;
        }


        //Back Center raycast
        if (Physics.Raycast(p_centerPos - castFromModifierHalf, p_direction, out hit, p_boundingDistance, CustomLayers.m_enviromentMask | CustomLayers.m_hurtBoxMask))
        {
            p_collisionType = hit.collider.gameObject.layer == CustomLayers.m_enviromentLayer ? COLLISION_TYPE.ENVIROMENT : COLLISION_TYPE.HURT_BOX;//Use layer not mask, when comparing to game object layer
            return true;
        }


        //Back raycast
        if (Physics.Raycast(p_centerPos - castFromModifier, p_direction, out hit, p_boundingDistance, CustomLayers.m_enviromentMask | CustomLayers.m_hurtBoxMask))
        {
            p_collisionType = hit.collider.gameObject.layer == CustomLayers.m_enviromentLayer ? COLLISION_TYPE.ENVIROMENT : COLLISION_TYPE.HURT_BOX;//Use layer not mask, when comparing to game object layer
            return true;
        }

        p_collisionType = COLLISION_TYPE.ENVIROMENT; //Default
        return false;
    }

    /// <summary>
    /// Hard set the velocity
    /// </summary>
    /// <param name="p_val">velocity</param>
    public void HardSetVelocity(Vector2 p_val)
    {
        m_splineLocalVelocity = p_val;
    }

    /// <summary>
    /// Hard set the horizontal velocity
    /// </summary>
    /// <param name="p_val">velocity</param>
    public void HardSetHorizontalVelocity(float p_val)
    {
        m_splineLocalVelocity.x = p_val;
    }

    /// <summary>
    /// Hard set the vertical velocity
    /// </summary>
    /// <param name="p_val">velocity</param>
    public void HardSetUpwardsVelocity(float p_val)
    {
        m_splineLocalVelocity.y = p_val;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    protected virtual void OnDrawGizmosSelected()
    {
        if(m_showDebugColliders)
        {
            //Box collider
            BoxCollider boxCollider = GetComponent<BoxCollider>();

            CustomDebug.DrawSquare(boxCollider.bounds.center, boxCollider.bounds.size.z, boxCollider.bounds.size.y, transform.forward, Color.blue);

            //Collisions
            Vector3 colliderForwardOffset = transform.forward * (boxCollider.bounds.extents.z + DETECTION_RANGE / 2.0f);
            Vector3 colliderUpOffset = Vector3.up * (boxCollider.bounds.extents.y + DETECTION_RANGE / 2.0f);

            //Forward
            Color collisionColor = m_forwardCollision ? Color.red : Color.green;
            CustomDebug.DrawSquare(boxCollider.bounds.center - colliderForwardOffset, DETECTION_RANGE, boxCollider.bounds.size.y, transform.forward, collisionColor, CustomDebug.DEFAULT_LINE_THICKNESS_HALF);

            //Backward
            collisionColor = m_backCollision ? Color.red : Color.green;
            CustomDebug.DrawSquare(boxCollider.bounds.center + colliderForwardOffset, DETECTION_RANGE, boxCollider.bounds.size.y, transform.forward, collisionColor, CustomDebug.DEFAULT_LINE_THICKNESS_HALF);

            //Up
            collisionColor = m_upCollision ? Color.red : Color.green;
            CustomDebug.DrawSquare(boxCollider.bounds.center + colliderUpOffset, boxCollider.bounds.size.z, DETECTION_RANGE, transform.forward, collisionColor, CustomDebug.DEFAULT_LINE_THICKNESS_HALF);

            //Down
            collisionColor = m_downCollision ? Color.red : Color.green;
            CustomDebug.DrawSquare(boxCollider.bounds.center - colliderUpOffset, boxCollider.bounds.size.z, DETECTION_RANGE, transform.forward, collisionColor, CustomDebug.DEFAULT_LINE_THICKNESS_HALF);
        }

        if (!Application.isPlaying)
        {
            if (MOARDebugging.GetSplinePosition(m_nodeA, m_nodeB, m_currentSplinePercent, out Vector3 position))
            {
                transform.position = position;
            }
        }
    }
#endif
}
