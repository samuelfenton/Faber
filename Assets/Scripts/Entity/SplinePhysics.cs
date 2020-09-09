using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SplinePhysics : MonoBehaviour
{
    public const float MIN_SPLINE_PERCENT = -0.05f;
    public const float MAX_SPLINE_PERCENT = 1.05f;

    protected const float GROUND_DETECTION = 0.5f;
    public const float GRAVITY = -24.0f;

    [Header("Spline settings")]
    public Pathing_Node m_nodeA = null;
    public Pathing_Node m_nodeB = null;

    [Range(0, 1)]
    public float m_currentSplinePercent = 0.0f;

    [HideInInspector]
    public bool m_upCollision = false;
    [HideInInspector]
    public bool m_downCollision = false;
    [HideInInspector]
    public bool m_forwardCollision = false;
    [HideInInspector]
    public bool m_backCollision = false;

    [HideInInspector]
    public Pathing_Spline m_currentSpline = null;
    protected Vector3 m_colliderExtents = Vector3.zero;
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
#if UNITY_EDITOR|| DEVELOPMENT_BUILD
            Debug.LogWarning(name + " doesnt have both nodes set in the inspector");
#endif
            return;
        }

        Pathing_Spline.SPLINE_POSITION splinePosition = m_nodeA.DetermineNodePosition(m_nodeB);

        if (splinePosition == Pathing_Spline.SPLINE_POSITION.MAX_LENGTH)
        {
#if UNITY_EDITOR|| DEVELOPMENT_BUILD
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
            m_parentEntity.m_localVelocity.y += GRAVITY * Time.deltaTime;

        //check for collisions and modify velocity as needed
        UpdateCollisions();

        //Setup forwards direction
        Vector3 desiredForwards = m_currentSpline.GetForwardDir(m_currentSplinePercent);

        float relativeDot = Vector3.Dot(desiredForwards, transform.forward);
        if (relativeDot > 0)
        {
            transform.rotation = Quaternion.LookRotation(desiredForwards, Vector3.up);
            m_currentSplinePercent += m_currentSpline.ChangeinPercent(m_parentEntity.m_localVelocity.x * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(-desiredForwards, Vector3.up);
            m_currentSplinePercent += m_currentSpline.ChangeinPercent(-m_parentEntity.m_localVelocity.x * Time.deltaTime);
        }
        //Lock spline percent between close enough to 0 - 1
        m_currentSplinePercent = Mathf.Clamp(m_currentSplinePercent, MIN_SPLINE_PERCENT, MAX_SPLINE_PERCENT);

        //Setup transform
        Vector3 newPosition = m_currentSpline.GetPosition(m_currentSplinePercent); //Spline position with no y considered

        newPosition.y = transform.position.y + m_parentEntity.m_localVelocity.y * Time.deltaTime; //Adding in y value

        float distanceToGround = GetSplineDistance();

        //Override down collision dependant on spline collision
        m_downCollision = m_downCollision || distanceToGround < GROUND_DETECTION;

        if (m_parentEntity.m_localVelocity.y <= 0.0f && distanceToGround <= 0.0f)
        {
            //Set y-pos
            newPosition.y = transform.position.y - distanceToGround;

            //Grounded change y-component of velocity
            m_parentEntity.m_localVelocity.y = 0.0f;
        }

        transform.position = newPosition;
    }

    /// <summary>
    /// Check for collisions in horizontal and vertical axis
    /// </summary>
    public void UpdateCollisions()
    {
        Vector3 centerPos = transform.position + Vector3.up * m_colliderExtents.y;

        m_upCollision = CollidingVertical(transform.up, centerPos);
        m_downCollision = CollidingVertical(-transform.up, centerPos);

        m_forwardCollision = CollidingHorizontal(transform.forward, centerPos);
        m_backCollision = CollidingHorizontal(-transform.forward, centerPos);

        UpdateCollisionVelocity();
    }

    /// <summary>
    /// Update local velocity based off collisions
    /// </summary>
    public void UpdateCollisionVelocity()
    {
        if (m_upCollision && m_parentEntity.m_localVelocity.y > 0)//Check Upwards
        {
            m_parentEntity.m_localVelocity.y = 0;
        }
        else if (m_downCollision && m_parentEntity.m_localVelocity.y < 0)//Downwards
        {
            m_parentEntity.m_localVelocity.y = 0;
        }
        if (m_forwardCollision && m_parentEntity.m_localVelocity.x > 0)//Forwards
        {
            m_parentEntity.m_localVelocity.x = 0;
        }
        else if (m_backCollision && m_parentEntity.m_localVelocity.x < 0)//Backwards
        {
            m_parentEntity.m_localVelocity.x = 0;
        }
    }

    /// <summary>
    /// Check for collisions vertically
    /// Creates three raycasts, front center and back
    /// </summary>
    /// <param name="p_direction">Casting up or down</param>
    /// <param name="p_centerPos">What is current center position of charater</param>
    /// <returns>True when any collisions occur</returns>
    public bool CollidingVertical(Vector3 p_direction, Vector3 p_centerPos)
    {
        //Forward raycast
        if (Physics.Raycast(p_centerPos + transform.forward * m_colliderExtents.z, p_direction, m_colliderExtents.y + m_collisionDetection, CustomLayers.m_enviroment))
            return true; //Early breakout

        //Back raycast
        if (Physics.Raycast(p_centerPos, p_direction, m_colliderExtents.y + m_collisionDetection, CustomLayers.m_enviroment))
            return true; //Early breakout

        //Center raycast
        if (Physics.Raycast(p_centerPos - transform.forward * m_colliderExtents.z, p_direction, m_colliderExtents.y + m_collisionDetection, CustomLayers.m_enviroment))
            return true; //Early breakout

        return false;
    }

    /// <summary>
    /// Check for collisions horizontally
    /// Creates three raycasts, top center and bottom
    /// </summary>
    /// <param name="p_direction">Casting forward or backwards</param>
    /// <param name="p_centerPos">What is current center position of charater</param>
    /// <returns>True when any collisions occur</returns>
    public bool CollidingHorizontal(Vector3 p_direction, Vector3 p_centerPos)
    {
        //Top raycast
        if (Physics.Raycast(p_centerPos + transform.up * m_colliderExtents.y, p_direction, m_colliderExtents.z + m_collisionDetection, CustomLayers.m_enviroment))
            return true; //Early breakout

        //Center raycast
        if (Physics.Raycast(p_centerPos, p_direction, m_colliderExtents.z + m_collisionDetection, CustomLayers.m_enviroment))
            return true; //Early breakout

        //Bottom raycast, starting offset has been modified, to all moving up inclines
        if (Physics.Raycast(p_centerPos - transform.up * m_colliderExtents.y, p_direction, m_colliderExtents.z + m_collisionDetection, CustomLayers.m_enviroment))
            return true; //Early breakout

        return false;
    }

    /// <summary>
    /// Get the distance between an entity and the spline its currently on
    /// </summary>
    /// <returns>Distance to spline, negative means is below the spline</returns>
    public float GetSplineDistance()
    {
        Vector3 splinePosition = m_currentSpline.GetPosition(m_currentSplinePercent);

        return transform.position.y - splinePosition.y;
    }
}
