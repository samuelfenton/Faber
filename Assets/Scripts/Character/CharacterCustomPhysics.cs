using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCustomPhysics : MonoBehaviour
{
    private Character m_parentCharacter = null;

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

    public void UpdateCollisions()
    {
        m_upCollision = CollidingVertical(m_colliderExtents.y, transform.up, transform.forward * m_colliderExtents.x * 0.9f);
        m_downCollision = CollidingVertical(m_colliderExtents.y, -transform.up, transform.forward * m_colliderExtents.x * 0.9f);

        m_forwardCollision = CollidingHorizontal(m_colliderExtents.x, transform.forward, transform.up * m_colliderExtents.y);
        m_backCollision = CollidingHorizontal(m_colliderExtents.x, -transform.forward, transform.up * m_colliderExtents.y);

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

    public bool CollidingVertical(float p_colliderExtent, Vector3 p_direction, Vector3 p_startingOffset)
    {
        //Forward raycast
        if (Physics.Raycast(transform.position + p_startingOffset, p_direction, p_colliderExtent + m_collisionDetection, LayerController.m_enviromentInclined | LayerController.m_enviromentWalkable))
            return true; //Early breakout

        //Back raycast
        if (Physics.Raycast(transform.position - p_startingOffset, p_direction, p_colliderExtent + m_collisionDetection, LayerController.m_enviromentInclined | LayerController.m_enviromentWalkable))
            return true; //Early breakout

        //Center raycast
        if (Physics.Raycast(transform.position, p_direction, p_colliderExtent + m_collisionDetection, LayerController.m_enviromentInclined | LayerController.m_enviromentWalkable))
            return true; //Early breakout

        return false;
    }

    public bool CollidingHorizontal(float p_colliderExtent, Vector3 p_direction, Vector3 p_startingOffset)
    {
        //Center raycast
        if (Physics.Raycast(transform.position, p_direction, p_colliderExtent + m_collisionDetection, LayerController.m_enviromentInclined | LayerController.m_enviromentWalkable))
            return true; //Early breakout

        //Top raycast
        if (Physics.Raycast(transform.position + p_startingOffset, p_direction, p_colliderExtent + m_collisionDetection, LayerController.m_enviromentInclined | LayerController.m_enviromentWalkable))
            return true; //Early breakout

        //Bottom raycast, starting offset has been modified, to all moving up inclines
        if (Physics.Raycast(transform.position + p_startingOffset, p_direction, p_colliderExtent + m_collisionDetection, LayerController.m_enviromentInclined | LayerController.m_enviromentWalkable))
            return true; //Early breakout

        return false;
    }

    public void GroundCollisionCheck()
    {
        RaycastHit hit;

        //Checking y for center
        if (Physics.Raycast(transform.position, -m_parentCharacter.m_characterModel.transform.up, out hit, m_colliderExtents.y, LayerController.m_enviromentInclined | LayerController.m_enviromentWalkable))
            GroundYSet(hit);
        //Checking y for front
        if (Physics.Raycast(transform.position + transform.forward * m_colliderExtents.x, -m_parentCharacter.m_characterModel.transform.up, out hit, m_colliderExtents.y, LayerController.m_enviromentInclined | LayerController.m_enviromentWalkable))
            GroundYSet(hit);
        //Checking y for back
        if (Physics.Raycast(transform.position - transform.forward * m_colliderExtents.x, -m_parentCharacter.m_characterModel.transform.up, out hit, m_colliderExtents.y, LayerController.m_enviromentInclined | LayerController.m_enviromentWalkable))
            GroundYSet(hit);
    }

    private void GroundYSet(RaycastHit p_hit)
    {
        //Set y-pos
        Vector3 position = transform.position;
        position.y = p_hit.point.y + m_colliderExtents.y;
        transform.position = position;

        //Grounded change y-component of velocity
        m_parentCharacter.m_localVelocity.y = 0.0f;
    }
}
