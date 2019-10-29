using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePhysics : MonoBehaviour
{
    protected const float GROUND_DETECTION = 0.5f;
    protected const float GRAVITY = -9.8f;
    [Header("Physic settings")]
    public bool m_gravity = false;

    [Header("Spline settings")]
    [Range(0, 1)]
    public Navigation_Spline m_currentSpline = null;
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

    /// <summary>
    /// Initilise entity physics
    /// </summary>
    protected virtual void Start()
    {
        m_parentEntity = GetComponent<Entity>();

        if(m_parentEntity==null)
        {
#if UNITY_EDITOR
            Debug.Log(name + " has no entity attached, considering removing the spline physcis, or add one");
#endif
            Destroy(this);
            return;
        }

        if (m_currentSpline == null)//Safety breakout
        {
#if UNITY_EDITOR
            Debug.Log(name + " does not have a spline set in spline physics");
#endif
            Destroy(this);
            return;
        }
    }

#if UNITY_EDITOR
    //-------------------
    //DEV_MODE, places character on current spline based on its spline percent
    //-------------------
    private void OnValidate()
    {
        //Update position in scene 
        if (m_currentSpline != null)
        {
            m_currentSpline.Start();

            transform.position = m_currentSpline.GetSplinePosition(m_currentSplinePercent);
        }
    }
#endif

    //-------------------
    //Updates characters physics
    //  Set rotation to be based off spline position
    //  Clamp on spline
    //  Check for ground collisions
    //-------------------
    public void UpdatePhysics()
    {
        //Gravity
        m_parentEntity.m_localVelocity.y += GRAVITY * Time.deltaTime;

        UpdateCollisions();

        //Setup forwards direction
        Vector3 desiredForwards = m_currentSpline.GetForwardsDir(transform.position);
        float relativeDot = Vector3.Dot(desiredForwards, transform.forward);
        if (relativeDot > 0)
        {
            transform.rotation = Quaternion.LookRotation(desiredForwards, Vector3.up);
            m_currentSplinePercent += m_currentSpline.GetSplinePercent(m_parentEntity.m_localVelocity.x * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(-desiredForwards, Vector3.up);
            m_currentSplinePercent += m_currentSpline.GetSplinePercent(-m_parentEntity.m_localVelocity.x * Time.deltaTime);
        }

        //Lock spline percent between close enough to 0 - 1
        m_currentSplinePercent = Mathf.Clamp(m_currentSplinePercent, -0.01f, 1.01f);

        Vector3 splinePosition = m_currentSpline.GetSplinePosition(m_currentSplinePercent);

        //Check for ground collisions when falling
        Vector3 position = transform.position;

        position.x = splinePosition.x;
        position.z = splinePosition.z;
        position.y += m_parentEntity.m_localVelocity.y * Time.deltaTime;

        float distanceToGround = transform.position.y - splinePosition.y;

        //Override down collision dependant on spline collision
        m_downCollision = distanceToGround < GROUND_DETECTION;

        if (m_parentEntity.m_localVelocity.y <= 0 && distanceToGround < 0)
        {
            //Set y-pos
            position.y = splinePosition.y;

            //Grounded change y-component of velocity
            m_parentEntity.m_localVelocity.y = 0.0f;
        }

        transform.position = position;

    }

    //-------------------
    //Check for collisions in horizontal and vertical axis
    //-------------------
    public void UpdateCollisions()
    {
        Vector3 centerPos = transform.position + Vector3.up * m_colliderExtents.y;

        m_upCollision = CollidingVertical(transform.up, centerPos);
        m_downCollision = CollidingVertical(-transform.up, centerPos);

        m_forwardCollision = CollidingHorizontal(transform.forward, centerPos);
        m_backCollision = CollidingHorizontal(-transform.forward, centerPos);

        UpdateCollisionVelocity();
    }

    //-------------------
    //Update local velocity based off collisions
    //-------------------
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

    //-------------------
    //Check for collisions vertically
    //  Creates three raycasts, front center and back
    //
    //Param p_direction: Casting up or down
    //      p_centerPos: What is current center position of charater
    //
    //Return bool: if any collisions occur return true
    //-------------------
    public bool CollidingVertical(Vector3 p_direction, Vector3 p_centerPos)
    {
        //Forward raycast
        if (Physics.Raycast(p_centerPos + Vector3.forward * m_colliderExtents.x, p_direction, m_colliderExtents.y + m_collisionDetection, LayerController.m_walkable))
            return true; //Early breakout

        //Back raycast
        if (Physics.Raycast(p_centerPos * m_colliderExtents.x, p_direction, m_colliderExtents.y + m_collisionDetection, LayerController.m_walkable))
            return true; //Early breakout

        //Center raycast
        if (Physics.Raycast(p_centerPos - Vector3.forward * m_colliderExtents.x, p_direction, m_colliderExtents.y + m_collisionDetection, LayerController.m_walkable))
            return true; //Early breakout

        return false;
    }

    //-------------------
    //Check for collisions horizontally
    //  Creates three raycasts, top center and bottom
    //
    //Param p_direction: Casting forward or backwards
    //      p_centerPos: What is current center position of charater
    //
    //Return bool: if any collisions occur return true
    //-------------------
    public bool CollidingHorizontal(Vector3 p_direction, Vector3 p_centerPos)
    {
        //Top raycast
        if (Physics.Raycast(p_centerPos + Vector3.up * m_colliderExtents.x, p_direction, m_colliderExtents.z + m_collisionDetection, LayerController.m_walkable))
            return true; //Early breakout

        //Center raycast
        if (Physics.Raycast(p_centerPos * m_colliderExtents.x, p_direction, m_colliderExtents.z + m_collisionDetection, LayerController.m_walkable))
            return true; //Early breakout

        //Bottom raycast, starting offset has been modified, to all moving up inclines
        if (Physics.Raycast(p_centerPos - Vector3.up * m_colliderExtents.x, p_direction, m_colliderExtents.z + m_collisionDetection, LayerController.m_walkable))
            return true; //Early breakout

        return false;
    }
}
