using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterCustomPhysics))]
[RequireComponent(typeof(CharacterStateMachine))]
public class Character : MonoBehaviour
{
    public Animator m_animator = null;
    public CharacterCustomPhysics m_characterCustomPhysics = null;
    protected CharacterStateMachine m_characterStateMachine  = null;

    [Tooltip("Set in inspector")]
    public GameController m_gameController = null;

    public GameObject m_characterModel = null;

    public Vector3 m_localVelocity = Vector3.zero;

    [Header("Character Stats")]
    public float m_maxHealth = 10.0f;
    public float m_currentHealth = 10.0f;

    protected virtual void Start()
    {
        m_animator = GetComponent<Animator>();
        m_characterCustomPhysics = GetComponent<CharacterCustomPhysics>();
        m_characterStateMachine = GetComponent<CharacterStateMachine>();

        m_characterStateMachine.InitStateMachine();

        m_currentHealth = m_maxHealth;
    }

    protected virtual void FixedUpdate()
    {
        //Set up velocity in each characters fixed update

        //Apply Velocity
        m_localVelocity.y += PhysicsController.m_gravity * Time.deltaTime;

        //Stop colliding with objects
        m_characterCustomPhysics.UpdateCollisions();

        Vector3 globalVelocity = new Vector3();
        globalVelocity = transform.forward * m_localVelocity.x + transform.up * m_localVelocity.y + transform.right * m_localVelocity.z;

        transform.Translate(globalVelocity * Time.fixedDeltaTime, Space.World);

        //Check for ground collisions when falling
        if (globalVelocity.y < 0.1f)
        {
            m_characterCustomPhysics.GroundCollisionCheck();
        }
    }

    public bool IsAlive()
    {
        return m_currentHealth > 0.0f;
    }

    public void OnDeath()
    {

    }

    public bool IsGrounded()
    {
        return m_characterCustomPhysics.m_downCollision;
    }
}
