using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SplinePhysics : MonoBehaviour
{
    public const float MIN_SPLINE_PERCENT = -0.001f;
    public const float MAX_SPLINE_PERCENT = 1.001f;

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
    public Vector2 m_splineVelocity = Vector2.zero;

    //Treated as globals, where the model facing when possible
    [System.Serializable]
    public struct CollisionEvent
    {
        public static CollisionEvent Zero() 
        {
            CollisionEvent newCollisionEvent = new CollisionEvent(0.0f);
            newCollisionEvent.m_collision = false;
            return newCollisionEvent; 
        }
        public static CollisionEvent NegitiveInfinity()
        {
            CollisionEvent newCollisionEvent = new CollisionEvent(float.NegativeInfinity);
            newCollisionEvent.m_collision = false;
            return newCollisionEvent;
        }

        public CollisionEvent(float p_distance)
        {
            m_collision = true;
            m_overlapDist = p_distance;
        }

        public void Reset()
        {
            m_collision = false;
            m_overlapDist = 0.0f;
        }

        public bool m_collision;
        public float m_overlapDist;
    }

    public CollisionEvent m_upCollision = CollisionEvent.Zero();
    public CollisionEvent m_downCollision = CollisionEvent.Zero();
    public CollisionEvent m_forwardCollision = CollisionEvent.Zero();
    public CollisionEvent m_backCollision = CollisionEvent.Zero();

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
    public void ResolvePhysics()
    {
        //Apply Gravity
        if (m_parentEntity.m_gravity)
            m_splineVelocity.y += GRAVITY * Time.deltaTime;

        //Setup forwards direction, modify spline percent
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


        UpdateCollisions();
        ResolveCollisionsVelocity();

        //Setup transform
        //Lock spline percent between close enough to 0 - 1
        m_currentSplinePercent = Mathf.Clamp(m_currentSplinePercent, MIN_SPLINE_PERCENT, MAX_SPLINE_PERCENT);

        Vector3 currentPosition = m_currentSpline.GetPosition(m_currentSplinePercent);

        currentPosition.y = transform.position.y + m_splineVelocity.y * Time.deltaTime; //Adding in y value

        transform.position = currentPosition;
    }

    /// <summary>
    /// Check for collisions in horizontal and vertical axis
    /// </summary>
    public void UpdateCollisions()
    {
        Vector3 forward = transform.forward ;
        Vector3 up = transform.up;

        Vector3 centerPos = transform.position + forward * m_splineColliderOffset.x + up * m_splineColliderOffset.y;

        m_upCollision = CastCollision(up, centerPos, forward * m_splineColliderExtents.x, m_splineColliderExtents.y);
        m_downCollision = CastCollision(-up, centerPos, forward * m_splineColliderExtents.x, m_splineColliderExtents.y);

        m_forwardCollision = CastCollision(forward, centerPos, up * m_splineColliderExtents.y, m_splineColliderExtents.x);
        m_backCollision = CastCollision(-forward, centerPos, up * m_splineColliderExtents.y, m_splineColliderExtents.x);

        //Is entity on spline
        float splineOverlap = m_currentSpline.GetPosition(m_currentSplinePercent).y - transform.position.y;

        if (splineOverlap >= 0.0f && splineOverlap > m_downCollision.m_overlapDist)
        {
            //Grounded change y-component of velocity
            m_downCollision = new CollisionEvent(splineOverlap);
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
    private CollisionEvent CastCollision(Vector3 p_direction, Vector3 p_centerPos, Vector3 p_castFromModifer, float p_boundingDistance)
    {
        CollisionEvent collisionEvent = CollisionEvent.NegitiveInfinity();
        Vector3 castFromModifierHalf = p_castFromModifer / 2.0f;

        RaycastHit collisionHit;

        //Large Offset raycast
        if (Physics.Raycast(p_centerPos + p_castFromModifer, p_direction, out collisionHit, p_boundingDistance, CustomLayers.m_enviroment))
        {
            float overlapDistance = p_boundingDistance - collisionHit.distance;
            if (overlapDistance > collisionEvent.m_overlapDist) 
                collisionEvent = new CollisionEvent(overlapDistance);
        }

        //Forward Center raycast
        if (Physics.Raycast(p_centerPos + castFromModifierHalf, p_direction, out collisionHit, p_boundingDistance, CustomLayers.m_enviroment))
        {
            float overlapDistance = p_boundingDistance - collisionHit.distance;
            if (overlapDistance > collisionEvent.m_overlapDist)
                collisionEvent = new CollisionEvent(overlapDistance);
        }

        //Center raycast
        if (Physics.Raycast(p_centerPos, p_direction, out collisionHit, p_boundingDistance, CustomLayers.m_enviroment))
        {
            float overlapDistance = p_boundingDistance - collisionHit.distance;
            if (overlapDistance > collisionEvent.m_overlapDist)
                collisionEvent = new CollisionEvent(overlapDistance);
        }

        //Back Center raycast
        if (Physics.Raycast(p_centerPos - castFromModifierHalf, p_direction, out collisionHit, p_boundingDistance, CustomLayers.m_enviroment))
        {
            float overlapDistance = p_boundingDistance - collisionHit.distance;
            if (overlapDistance > collisionEvent.m_overlapDist)
                collisionEvent = new CollisionEvent(overlapDistance);
        }

        //Back raycast
        if (Physics.Raycast(p_centerPos - p_castFromModifer, p_direction, out collisionHit, p_boundingDistance, CustomLayers.m_enviroment))
        {
            float overlapDistance = p_boundingDistance - collisionHit.distance;
            if (overlapDistance > collisionEvent.m_overlapDist)
                collisionEvent = new CollisionEvent(overlapDistance);
        }

        return collisionEvent;
    }

    /// <summary>
    /// Update local velocity based off collisions
    /// </summary>
    private void ResolveCollisionsVelocity()
    {
        if (m_upCollision.m_collision)//Check Upwards
        {
            if (m_splineVelocity.y > 0.0f)//Moving up, stop this 
                m_splineVelocity.y = 0.0f;

            transform.position -= transform.up * m_upCollision.m_overlapDist;
        }
        else if (m_downCollision.m_collision)//Downwards
        {
            if (m_splineVelocity.y < 0.0f)//Moving up, stop this 
                m_splineVelocity.y = 0.0f;

            transform.position += transform.up * m_downCollision.m_overlapDist;
        }
        if (m_forwardCollision.m_collision)//Forwards
        {
            if (m_splineVelocity.x > 0.0f)//Moving up, stop this 
                m_splineVelocity.x = 0.0f;

            if (m_parentEntity.AllignedToSpline())
            {
                m_currentSplinePercent -= m_currentSpline.ChangeInPercent(m_forwardCollision.m_overlapDist);
            }
            else
            {
                m_currentSplinePercent += m_currentSpline.ChangeInPercent(m_forwardCollision.m_overlapDist);
            }
        }
        else if (m_backCollision.m_collision)//Backwards
        {
            if (m_splineVelocity.x < 0.0f)//Moving up, stop this 
                m_splineVelocity.x = 0.0f;

            if (m_parentEntity.AllignedToSpline())
            {
                m_currentSplinePercent += m_currentSpline.ChangeInPercent(m_forwardCollision.m_overlapDist);
            }
            else
            {
                m_currentSplinePercent -= m_currentSpline.ChangeInPercent(m_forwardCollision.m_overlapDist);
            }
        }
    }

    /// <summary>
    /// Swap facing direction
    /// </summary>
    public virtual void SwapFacingDirection()
    {
        if (m_parentEntity.AllignedToSpline()) //Currently alligned, so face backwards
        {
            Vector3 desiredForwards = m_currentSpline.GetForwardDir(m_currentSplinePercent);
            transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f) * Quaternion.LookRotation(desiredForwards, Vector3.up) ;
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
