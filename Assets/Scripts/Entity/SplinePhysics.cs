using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SplinePhysics : MonoBehaviour
{
    public const float MIN_SPLINE_PERCENT = -0.001f;
    public const float MAX_SPLINE_PERCENT = 1.001f;

    protected const float GROUND_DETECTION = 0.1f;
    public const float GRAVITY = -24.0f;

    [Header("Spline settings")]
    public Pathing_Node m_nodeA = null;
    public Pathing_Node m_nodeB = null;

    [Range(0, 1)]
    public float m_currentSplinePercent = 0.0f;

    [HideInInspector]
    public Pathing_Spline m_currentSpline = null;

    [Header("Spline Collisions")]
    public Vector2 m_splineColliderOffset = Vector2.zero;
    public Vector2 m_splineColliderExtents = Vector2.one;

    [Header("Debug Options")]
    public bool m_showColliders = true;

    [Header("Generated values")]
    public Vector3 m_localVelocity = Vector3.zero;

    //Treated as globals, where the model facing when possible
    public bool m_upCollision = false;
    public bool m_downCollision = false;
    public bool m_forwardCollision = false;
    public bool m_backCollision = false;

    protected float m_collisionDetection = 0.3f;

    protected Entity m_parentEntity = null;

    public virtual void Init()
    {
        m_parentEntity = GetComponent<Entity>();

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
    public void UpdatePhysics()
    {
        //Apply Gravity
        if (m_parentEntity.m_gravity)
            m_localVelocity.y += GRAVITY * Time.deltaTime;

        //Update velocity
        if (m_localVelocity.x < 0.0f)
        {
            SwapFacingDirection();
        }

        //check for collisions and modify velocity as needed
        UpdateCollisions();

        //Setup forwards direction
        if (m_parentEntity.AllignedToSpline())
        {
            m_currentSplinePercent += m_currentSpline.ChangeInPercent(m_localVelocity.x * Time.deltaTime);
        }
        else
        {
            m_currentSplinePercent -= m_currentSpline.ChangeInPercent(m_localVelocity.x * Time.deltaTime);
        }
        
        Vector3 desiredForwards = m_currentSpline.GetForwardDir(m_currentSplinePercent);
        transform.rotation = Quaternion.LookRotation(desiredForwards, Vector3.up);

        //Lock spline percent between close enough to 0 - 1
        m_currentSplinePercent = Mathf.Clamp(m_currentSplinePercent, MIN_SPLINE_PERCENT, MAX_SPLINE_PERCENT);

        //Setup transform
        Vector3 splinePosition = m_currentSpline.GetPosition(m_currentSplinePercent);

        Vector3 newPosition = splinePosition; //Spline position with no y considered
        newPosition.y = transform.position.y + m_localVelocity.y * Time.deltaTime; //Adding in y value

        float distanceToGround = transform.position.y - splinePosition.y;

        //Is entity on spline
        if (m_localVelocity.y <= 0.0f && distanceToGround <= GROUND_DETECTION)
        {
            //Grounded change y-component of velocity
            newPosition.y = splinePosition.y;
            m_localVelocity.y = 0.0f;
            m_downCollision = true;
        }

        transform.position = newPosition;
    }

    /// <summary>
    /// Check for collisions in horizontal and vertical axis
    /// </summary>
    public void UpdateCollisions()
    {
        Vector3 forward = transform.forward ;
        Vector3 up = transform.up;

        Vector3 centerPos = transform.position + forward * m_splineColliderExtents.x + up * m_splineColliderExtents.y;

        m_upCollision = CastCollision(up, centerPos, forward * m_splineColliderExtents.x, m_splineColliderExtents.y + m_collisionDetection);
        m_downCollision = CastCollision(-up, centerPos, forward * m_splineColliderExtents.x, m_splineColliderExtents.y + m_collisionDetection);

        m_forwardCollision = CastCollision(forward, centerPos, up * m_splineColliderExtents.y, m_splineColliderExtents.x + m_collisionDetection);
        m_backCollision = CastCollision(-forward, centerPos, up * m_splineColliderExtents.y, m_splineColliderExtents.x + m_collisionDetection);

        UpdateCollisionVelocity();
    }

    /// <summary>
    /// Update local velocity based off collisions
    /// </summary>
    public void UpdateCollisionVelocity()
    {
        if (m_upCollision && m_localVelocity.y > 0)//Check Upwards
        {
            m_localVelocity.y = 0;
        }
        else if (m_downCollision && m_localVelocity.y < 0)//Downwards
        {
            m_localVelocity.y = 0;
        }
        if (m_forwardCollision && m_localVelocity.x > 0)//Forwards
        {
            m_localVelocity.x = 0;
        }
        else if (m_backCollision && m_localVelocity.x < 0)//Backwards
        {
            m_localVelocity.x = 0;
        }
    }

    /// <summary>
    /// Check for collisions vertically
    /// Creates three raycasts, forward, forward center, center, back center, back
    /// </summary>
    /// <param name="p_direction">Casting up or down</param>
    /// <param name="p_centerPos">What is current center position of charater</param>
    /// <param name="p_castFromModifer">Modifer from center cast to offset most</param>
    /// <param name="p_detectionRange">Distance to cast</param>
    /// <returns>True when any collisions occur</returns>
    public bool CastCollision(Vector3 p_direction, Vector3 p_centerPos, Vector3 p_castFromModifer, float p_detectionRange)
    {
        Vector3 castFromModifierHalf = p_castFromModifer / 2.0f;

        //Large Offset raycast
        if (Physics.Raycast(p_centerPos + p_castFromModifer, p_direction, p_detectionRange, CustomLayers.m_enviroment))
            return true; //Early breakout

        //Forward Center raycast
        if (Physics.Raycast(p_centerPos + castFromModifierHalf, p_direction, p_detectionRange, CustomLayers.m_enviroment))
            return true; //Early breakout

        //Center raycast
        if (Physics.Raycast(p_centerPos, p_direction, p_detectionRange, CustomLayers.m_enviroment))
            return true; //Early breakout

        //Back Center raycast
        if (Physics.Raycast(p_centerPos - castFromModifierHalf, p_direction, p_detectionRange, CustomLayers.m_enviroment))
            return true; //Early breakout

        //Back raycast
        if (Physics.Raycast(p_centerPos - p_castFromModifer, p_direction, p_detectionRange, CustomLayers.m_enviroment))
            return true; //Early breakout

        return false;
    }

    /// <summary>
    /// Swap facing direction
    /// </summary>
    public virtual void SwapFacingDirection()
    {
        m_localVelocity.x = -m_localVelocity.x;
        transform.Rotate(Vector3.up, 180.0f);
    }

    /// <summary>
    /// Hard set the value of velocity
    /// </summary>
    /// <param name="p_val">velocity</param>
    public void HardSetVelocity(float p_val)
    {
        m_localVelocity.x = p_val;
    }

    /// <summary>
    /// Hard set the value of velocity
    /// </summary>
    /// <param name="p_val">velocity</param>
    public void HardSetUpwardsVelocity(float p_val)
    {
        m_localVelocity.y = p_val;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    protected virtual void OnDrawGizmosSelected()
    {
        if(m_showColliders)
        {
            Gizmos.color = new Color(255.0f, 255.0f, 0.0f, 1.0f);

            Vector3 forward = transform.forward;
            Vector3 up = transform.up;

            Vector3 startPosition = transform.position + forward * m_splineColliderOffset.x + up * m_splineColliderOffset.y;

            Gizmos.DrawLine(startPosition - forward * m_splineColliderExtents.x + up * m_splineColliderExtents.y, startPosition + forward * m_splineColliderExtents.x + up * m_splineColliderExtents.y);
            Gizmos.DrawLine(startPosition - forward * m_splineColliderExtents.x - up * m_splineColliderExtents.y, startPosition + forward * m_splineColliderExtents.x - up * m_splineColliderExtents.y);

            Gizmos.DrawLine(startPosition - forward * m_splineColliderExtents.x + up * m_splineColliderExtents.y, startPosition - forward * m_splineColliderExtents.x - up * m_splineColliderExtents.y);
            Gizmos.DrawLine(startPosition + forward * m_splineColliderExtents.x + up * m_splineColliderExtents.y, startPosition + forward * m_splineColliderExtents.x - up * m_splineColliderExtents.y);

        }
    }
#endif
}
