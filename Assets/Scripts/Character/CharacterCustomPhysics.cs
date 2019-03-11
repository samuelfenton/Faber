using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCustomPhysics : MonoBehaviour
{
    private const float GROUND_DETECTION = 0.5f; 

    private Character m_parentCharacter = null;

    public Navigation_Spline m_currentSpline = null;

    public float m_currentSplinePercent = 0.0f;

    [Header("Collision Details")]
    public bool m_upCollision = false;
    public bool m_downCollision = false;
    public bool m_forwardCollision = false;
    public bool m_backCollision = false;

    [Header("Character Collision")]
    protected Vector3 m_colliderExtents = Vector3.zero;
    protected float m_collisionDetection = 0.1f;

    private void Start()
    {
        m_parentCharacter = GetComponent<Character>();

        CapsuleCollider capculeCollider = GetComponent<CapsuleCollider>();

        m_colliderExtents.x = m_colliderExtents.z = capculeCollider.radius;
        m_colliderExtents.y = capculeCollider.height / 2.0f;
    }

    public void UpdatePhysics()
    {
        UpdateCollisions();

        //Setup forwards direction
        Vector3 desiredForwards = m_currentSpline.GetForwardsDir(transform.position);
        float relativeDot = Vector3.Dot(desiredForwards, transform.forward);
        if (relativeDot > 0)
        {
            transform.rotation = Quaternion.LookRotation(desiredForwards, Vector3.up);
            m_currentSplinePercent += m_currentSpline.GetSplinePercent(m_parentCharacter.m_localVelocity.x * Time.deltaTime);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(-desiredForwards, Vector3.up);
            m_currentSplinePercent += m_currentSpline.GetSplinePercent(-m_parentCharacter.m_localVelocity.x * Time.deltaTime);
        }
        //Lock spline percent between close enough to 0 - 1
        m_currentSplinePercent = Mathf.Clamp(m_currentSplinePercent, -0.01f, 1.01f);

        Vector3 splinePosition = m_currentSpline.GetSplinePosition(m_currentSplinePercent);

        //Check for ground collisions when falling
        Vector3 position = transform.position;

        position.x = splinePosition.x;
        position.z = splinePosition.z;
        position.y += m_parentCharacter.m_localVelocity.y * Time.deltaTime;

        float distanceToGround = transform.position.y - splinePosition.y;

        //Override down collision dependant on spline collision
        m_downCollision = distanceToGround < GROUND_DETECTION;

        if (m_parentCharacter.m_localVelocity.y <= 0 && distanceToGround < 0)
        {
            //Set y-pos
            position.y = splinePosition.y;

            //Grounded change y-component of velocity
            m_parentCharacter.m_localVelocity.y = 0.0f;
        }

        transform.position = position;

    }

    public void UpdateCollisions()
    {
        Vector3 centerPos = transform.position + Vector3.up * m_colliderExtents.y;

        m_upCollision = CollidingVertical(transform.up, centerPos);
        m_downCollision = CollidingVertical(-transform.up, centerPos);

        m_forwardCollision = CollidingHorizontal(transform.forward, centerPos);
        m_backCollision = CollidingHorizontal(-transform.forward, centerPos);

        UpdateCollisionVelocity();
    }

    public void UpdateCollisionVelocity()
    {
        if (m_parentCharacter.m_localVelocity.y > 0)//check upwards
        {
            if (m_upCollision)
                m_parentCharacter.m_localVelocity.y = 0;
        }
        if (m_parentCharacter.m_localVelocity.x > 0)//Forwards
        {
            if (m_forwardCollision)
                m_parentCharacter.m_localVelocity.x = 0;
        }
        else if (m_parentCharacter.m_localVelocity.x < 0)//Backwards
        {
            if (m_backCollision)
                m_parentCharacter.m_localVelocity.x = 0;
        }
    }

    public bool CollidingVertical(Vector3 p_direction, Vector3 p_centerPos)
    {
        //Forward raycast
        if (Physics.Raycast(p_centerPos + Vector3.forward * m_colliderExtents.x, p_direction, m_colliderExtents.y + m_collisionDetection, LayerController.m_walkable))
            return true; //Early breakout

        //Back raycast
        if (Physics.Raycast(p_centerPos* m_colliderExtents.x, p_direction, m_colliderExtents.y + m_collisionDetection, LayerController.m_walkable))
            return true; //Early breakout

        //Center raycast
        if (Physics.Raycast(p_centerPos - Vector3.forward * m_colliderExtents.x, p_direction, m_colliderExtents.y + m_collisionDetection, LayerController.m_walkable))
            return true; //Early breakout

        return false;
    }

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
