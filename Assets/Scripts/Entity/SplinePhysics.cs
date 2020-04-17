using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePhysics : MonoBehaviour
{
    public const float MIN_SPLINE_PERCENT = -0.05f;
    public const float MAX_SPLINE_PERCENT = 1.05f;

    protected const float GROUND_DETECTION = 0.5f;
    public const float GRAVITY = -9.8f;

    [Header("Spline settings")]
    public Pathing_Spline m_currentSpline = null;
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

    protected Vector3 m_colliderExtents = Vector3.zero;
    protected float m_collisionDetection = 0.1f;

    protected Entity m_parentEntity = null;

    protected virtual void Start()
    {
        m_parentEntity = GetComponent<Entity>();

        if(m_parentEntity==null)
        {
#if UNITY_EDITOR
            Debug.Log(name + " has no entity attached, considering removing the spline physcis, or add one");
#endif
            return;
        }

        if (m_currentSpline == null)//Safety breakout
        {
#if UNITY_EDITOR
            Debug.Log(name + " does not have a spline set in spline physics");
#endif
            return;
        }

        if (m_currentSpline != null)
        {
            transform.position = m_currentSpline.GetPosition(m_currentSplinePercent);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// DEV_MODE, places character on current spline based on its spline percent
    /// </summary>
    ///
    private void OnValidate()
    {
        //Update position in scene 
        if (m_currentSpline != null)
        {
            transform.position = m_currentSpline.GetPosition(m_currentSplinePercent);
        }
    }
#endif

    /// <summary>
    /// Updates characters physics
    /// Set rotation to be based off spline position
    /// Clamp on spline
    /// Check for ground collisions
    /// </summary>
    public void UpdatePhysics()
    {
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

        if (m_parentEntity.m_localVelocity.y <= 0 && distanceToGround <= 0)
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
        if (Physics.Raycast(p_centerPos + Vector3.forward * m_colliderExtents.x, p_direction, m_colliderExtents.y + m_collisionDetection, CustomLayers.m_walkable))
            return true; //Early breakout

        //Back raycast
        if (Physics.Raycast(p_centerPos * m_colliderExtents.x, p_direction, m_colliderExtents.y + m_collisionDetection, CustomLayers.m_walkable))
            return true; //Early breakout

        //Center raycast
        if (Physics.Raycast(p_centerPos - Vector3.forward * m_colliderExtents.x, p_direction, m_colliderExtents.y + m_collisionDetection, CustomLayers.m_walkable))
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
        if (Physics.Raycast(p_centerPos + Vector3.up * m_colliderExtents.x, p_direction, m_colliderExtents.z + m_collisionDetection, CustomLayers.m_walkable))
            return true; //Early breakout

        //Center raycast
        if (Physics.Raycast(p_centerPos * m_colliderExtents.x, p_direction, m_colliderExtents.z + m_collisionDetection, CustomLayers.m_walkable))
            return true; //Early breakout

        //Bottom raycast, starting offset has been modified, to all moving up inclines
        if (Physics.Raycast(p_centerPos - Vector3.up * m_colliderExtents.x, p_direction, m_colliderExtents.z + m_collisionDetection, CustomLayers.m_walkable))
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
