﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SplinePhysics))]
public class Character : Entity
{ 
    public enum TEAM { PLAYER, NPC, GAIA }
    public enum ATTACK_INPUT_STANCE { LIGHT, HEAVY, NONE }

    public const float SPRINT_MODIFIER = 1.5f;//Linked to animator, To slow down sprint, change here and in animation all related blend trees

    [Header("Assigned Character Varibles")]
    public GameObject m_primaryAnchor = null;
    public GameObject m_secondaryAnchor = null;

    [Header("Team")]
    public TEAM m_team = TEAM.NPC;

    //Stats
    [Header("Character Stats")]
    public float m_maxHealth = 10.0f;
    [SerializeField]
    private float m_currentHealth = 10.0f;

    [Header("Grounded Movement")]
    public float m_groundRunVel = 1.0f;
    public float m_groundAccel = 1.0f;
    public float m_groundedDeaccel = 1.0f;

    [Header("Jumping Stats")]
    public float m_jumpVelocity = 10.0f;

    [Header("In Air Stats")]
    public float m_inAirAccelModifier = 0.5f;
    public float m_doubleJumpSpeed = 6.0f;

    [Header("Wall Jump Stats")]
    public float m_wallJumpVerticalSpeed = 5.0f;
    public float m_wallJumpHorizontalSpeed = 2.0f;
    public float m_wallJumpInputDelay = 0.1f;

    [Header("Dash Stats")]
    public float m_dashVelocity = 12.0f;

    [Header("Animation Variables")]
    [Tooltip("How many idle poses have been assigned to the animator")]
    [Range(1, 100)]
    public int m_idlePoseCount = 1;
    [Tooltip("The average time between no input and idle animations")]
    [Range(1.0f, 100.0f)]
    public float m_idleDelayTime = 5.0f;
    [HideInInspector]
    public float m_idleDelayTimer = 0.0f;

    //Velocity stuff
    [HideInInspector]
    public Vector2 m_desiredVelocity = Vector2.zero;

    //Flags
    [Header("No toucy")]
    public bool m_doubleJumpFlag = false;
    public bool m_inAirDashFlag = false;
    public bool m_blockingFlag = false;
    public bool m_knockbackFlag = false;
    public bool m_recoilFlag = false;
    public bool m_deathFlag = false;

    //Stored references
    protected AttackController m_weaponManager = null;
    protected Animator m_animator = null;
    protected CustomAnimation m_customAnimation = null;

    [HideInInspector]
    public FollowCamera m_followCamera = null;

    [HideInInspector]
    public CharacterStatistics m_characterStatistics = null;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();

        //Get references
        m_characterStatistics = GetComponent<CharacterStatistics>();
        m_weaponManager = GetComponent<AttackController>();
        m_animator = GetComponentInChildren<Animator>();

        m_customAnimation = gameObject.AddComponent<CustomAnimation>();
        m_customAnimation.Init(m_animator);

        if (m_weaponManager != null)
            m_weaponManager.Init(this);//Least importance as has no dependances

        m_currentHealth = m_maxHealth;

        m_followCamera = FindObjectOfType<FollowCamera>();

    }

    /// <summary>
    /// Update an entity, this should be called from scene controller
    /// Used to handle different scene state, pause vs in game etc
    /// </summary>
    public override void UpdateEntity()
    {
        base.UpdateEntity();

        UpdateVelocity();
    }

    /// <summary>
    /// Update an entity, this should be called from scene controller
    /// Used to handle fixed updates, tranfroms etc
    /// </summary>
    public override void FixedUpdateEntity()
    {
        base.FixedUpdateEntity();

        UpdateAnimationLocomotion();
    }

    /// <summary>
    /// Update the characters velocity
    /// Use desired velocity to modify facing direction. Negitive desired will rotate
    /// </summary>
    protected void UpdateVelocity()
    {
        Vector3 newVelocity = m_splinePhysics.m_splineVelocity;

        float accel = m_splinePhysics.m_downCollision ? m_groundAccel : m_groundAccel * m_inAirAccelModifier;
        float deaccel = m_splinePhysics.m_downCollision ? m_groundedDeaccel : m_groundedDeaccel * m_inAirAccelModifier;

        //Run update to velocity based off desired
        if (m_desiredVelocity.x == 0.0f) //Stop
        {
            float deltaSpeed = deaccel * Time.deltaTime;
            if (deltaSpeed > Mathf.Abs(newVelocity.x))//Close enough to stopping this frame
                newVelocity.x = 0.0f;
            else
                newVelocity.x += newVelocity.x < 0 ? deltaSpeed : -deltaSpeed;//Still have high velocity, just slow down
        }
        else if (m_desiredVelocity.x > 0.0f) //Run forwards
        {
            float deltaSpeed = newVelocity.x > m_desiredVelocity.x || newVelocity.x < 0.0f ? deaccel * Time.deltaTime : accel * Time.deltaTime; // Desired slower then current, or current is wrong direction, use deaccel

            if (deltaSpeed > Mathf.Abs(newVelocity.x - m_desiredVelocity.x))
                newVelocity.x = m_desiredVelocity.x;
            else
                newVelocity.x += newVelocity.x < m_desiredVelocity.x ? deltaSpeed : -deltaSpeed;

        }
        else //Wants to run in opposite direction
        {
            float deltaSpeed = newVelocity.x < m_desiredVelocity.x || newVelocity.x > 0.0f ? deaccel * Time.deltaTime : accel * Time.deltaTime; // Desired slower then current, or current is wrong direction, use deaccel

            if (deltaSpeed > Mathf.Abs(newVelocity.x - m_desiredVelocity.x))
                newVelocity.x = m_desiredVelocity.x;
            else
                newVelocity.x += newVelocity.x > m_desiredVelocity.x ? -deltaSpeed : deltaSpeed;

            //Check for flip
            if(newVelocity.x < 0.0f)
            {
                m_splinePhysics.SwapFacingDirection();
                newVelocity.x = -newVelocity.x;
                m_desiredVelocity.x = -m_desiredVelocity.x;
            }
        }

        m_splinePhysics.m_splineVelocity = newVelocity;
    }

    /// <summary>
    /// Is the given Character alive?
    /// </summary>
    /// <returns>true when health is greater than 0</returns>
    public bool IsAlive()
    {
        return m_currentHealth > 0.0f;
    }

    /// <summary>
    /// Character has been killed
    /// </summary>
    public void OnDeath()
    {
        StartCoroutine(DestroyEntity());
    }

    /// <summary>
    /// Deal dmage to another character, set flags as needed
    /// </summary>
    /// <param name="p_manoeuvreController">Controller used in dealing damage</param>
    /// <param name="p_targetCharacter"></param>
    /// <param name="p_impactPosition">Position of impact</param>
    public void DealDamage(ManoeuvreController p_manoeuvreController, Character p_targetCharacter, Vector3 p_impactPosition)
    {
        if (p_targetCharacter == null)
            return;

        if(p_targetCharacter.m_blockingFlag)
        {
            m_recoilFlag = true;
        }
        else
        { 
            p_targetCharacter.ModifyHealth(-p_manoeuvreController.m_manoeuvreDamage);

            //Setup knockback
            p_targetCharacter.SetKnockbackImpact(p_manoeuvreController.m_damageImpact);
            p_targetCharacter.m_knockbackFlag = true;

            //Setup hit marker
            Vector3 cameraToTarget = p_targetCharacter.transform.position - m_followCamera.transform.position;
            cameraToTarget.y = 0.0f;
            Quaternion hitmarkerRotation = Quaternion.LookRotation(cameraToTarget, Vector3.up);

            m_sceneController.SpawnHitMarker(p_targetCharacter.transform.position + Vector3.up * 2.0f, hitmarkerRotation, Mathf.RoundToInt(p_manoeuvreController.m_manoeuvreDamage));

            //Setup particle effects
            Vector3 characterToTarget = p_targetCharacter.transform.position - transform.position;
            characterToTarget.y = 0;//Dont care about y

            Quaternion particleRotation = Quaternion.LookRotation(-characterToTarget, Vector3.up);

            m_sceneController.SpawnDamageParticles(p_impactPosition, particleRotation, p_manoeuvreController.m_damageDirection);
        }
    }

    /// <summary>
    /// Change characters health and check for death after health change
    /// </summary>
    /// <param name="p_value">how much to change health by</param>
    public void ModifyHealth(float p_value)
    {
        m_currentHealth += p_value;

        if (!IsAlive())
            OnDeath();
    }

    /// <summary>
    /// Set the desired velocity
    /// </summary>
    /// <param name="p_val">Desired velocity</param>
    public void SetDesiredVelocity(Vector2 p_val)
    {
        m_desiredVelocity = p_val;
    }

    /// <summary>
    /// Set the desired velocity
    /// </summary>
    /// <param name="p_val">Desired velocity</param>
    public void SetDesiredVelocity(float p_val)
    {
        m_desiredVelocity = new Vector2(p_val, 0.0f);
    }

    /// <summary>
    /// Update the locomotion base varibles
    /// </summary>
    public void UpdateAnimationLocomotion()
    {
        m_customAnimation.SetVaribleFloat(CustomAnimation.VARIBLE_FLOAT.CURRENT_VELOCITY, m_splinePhysics.m_splineVelocity.x / m_groundRunVel);
        m_customAnimation.SetVaribleFloat(CustomAnimation.VARIBLE_FLOAT.ABSOLUTE_VELOCITY, Mathf.Abs(m_splinePhysics.m_splineVelocity.x / m_groundRunVel));
    }

    public void SetKnockbackImpact(ManoeuvreController.DAMAGE_IMPACT p_impact)
    {
        float knockbackValue = p_impact == ManoeuvreController.DAMAGE_IMPACT.LOW ? 0.0f : p_impact == ManoeuvreController.DAMAGE_IMPACT.MEDIUM ? 1.0f : 2.0f;

        m_customAnimation.SetVaribleFloat(CustomAnimation.VARIBLE_FLOAT.KNOCKBACK_IMPACT, knockbackValue);
    }

    /// <summary>
    /// Set a random idle pose variable
    /// </summary>
    public void GetRandomIdlePose()
    {
        int randomPose = Random.Range(0, m_idlePoseCount);
        m_customAnimation.SetVaribleFloat(CustomAnimation.VARIBLE_FLOAT.RANDOM_IDLE, randomPose);
    }

    #region WEAPON FUNCTIONS - OVERRIDE
    /// <summary>
    /// Function desired to be overridden, should this character be attempting to perform light or heavy attack
    /// Example click by player, or logic for NPC
    /// </summary>
    /// <returns>Light,heavy or none based off logic</returns>
    public virtual ATTACK_INPUT_STANCE DetermineAttackStance()
    {
        return ATTACK_INPUT_STANCE.NONE;
    }
    #endregion
}
