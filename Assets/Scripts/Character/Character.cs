using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Entity
{
    [HideInInspector]
    public Character_SplinePhysics m_characterSplinePhysics = null;
    [HideInInspector]
    public CharacterStateMachine m_characterStateMachine = null;
    [HideInInspector]
    public CharacterAnimationController m_characterAnimationController = null;

    [Header("Basic Character Varibles")]
    [Tooltip("Set in inspector")]
    public GameController m_gameController = null;

    public GameObject m_characterModel = null;

    public enum TEAM { PLAYER, NPC, GAIA }
    public TEAM m_characterTeam = TEAM.GAIA;

    [Header("Grounded Movement")]
    public float m_groundedHorizontalSpeedMax = 1.0f;
    public float m_groundedHorizontalAcceleration = 1.0f;
    public float m_groundedHorizontalDeacceleration = 1.0f;

    [Header("Jumping Stats")]
    public float m_jumpSpeed = 10.0f;

    [Header("In Air Stats")]
    public float m_inAirHorizontalAcceleration = 0.5f;
    public float m_doubleJumpSpeed = 6.0f;

    [Header("Wall Jump Stats")]
    public float m_wallJumpVerticalSpeed = 5.0f;
    public float m_wallJumpHorizontalSpeed = 2.0f;
    public float m_wallJumpInputDelay = 0.1f;

    public enum ATTACK_TYPE { NONE, LIGHT, HEAVY }

    [Header("Attack Stats")]
    public ATTACK_TYPE m_currentAttackType = ATTACK_TYPE.NONE;
    protected Weapon m_weapon = null;

    [Header("Character Stats")]
    public float m_maxHealth = 10.0f;
    [SerializeField]
    private float m_currentHealth = 10.0f;

    protected CharacterInput m_characterInput = null;
    public CharacterInput.InputState m_currentCharacterInput;

    protected CharacterInventory m_characterInventory = null;

    //-------------------
    //Character setup
    //  Ensure all need componets are attached, and get initilised if needed
    //-------------------
    protected override void Start()
    {
        base.Start();

        //Get references
        m_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        m_characterSplinePhysics = (Character_SplinePhysics)m_splinePhysics;
        m_characterStateMachine = GetComponent<CharacterStateMachine>();
        m_characterAnimationController = GetComponentInChildren<CharacterAnimationController>();
        m_characterInventory = GetComponent<CharacterInventory>();
        m_characterInput = GetComponent<CharacterInput>();

        m_currentCharacterInput = new CharacterInput.InputState();

        //Init
        if (m_characterStateMachine != null)
            m_characterStateMachine.InitStateMachine();//Run first as animation depends on states being created
        if(m_characterAnimationController!=null)
            m_characterAnimationController.InitAnimationController();
        if (m_characterInventory != null)
            m_characterInventory.InitInventory();//Least importance as has no dependances

        if (m_characterStateMachine != null)
            m_characterStateMachine.StartStateMachine();//Run intial state
        //Secondary references
        m_weapon = GetComponentInChildren<Weapon>();

        if(m_weapon != null)
        {
            m_characterAnimationController.SetVarible(CharacterAnimationController.VARIBLES.WEAPON_SLOT, (float)m_weapon.m_animationIndex);
            m_weapon.m_parentCharacter = this;
        }

        m_currentHealth = m_maxHealth;
    }

    //-------------------
    //Character update
    //  Get input, apply physics, update character state machine
    //-------------------
    protected virtual void Update()
    {
        m_currentCharacterInput = m_characterInput.GetInputState();

        //Apply Velocity
        m_localVelocity.y += PhysicsController.m_gravity * Time.deltaTime;

        //Stop colliding with objects
        m_characterSplinePhysics.UpdatePhysics();

        //Setup rotation on game model, completly aesthetic based
        if (m_localVelocity.x > 0.1f)
        {
            m_characterModel.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }
        else if (m_localVelocity.x < -0.1f)
        {
            m_characterModel.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }

        m_characterStateMachine.UpdateStateMachine();
    }

    //-------------------
    //Is current character alive
    //
    //Return bool: Is health greater than 0
    //-------------------
    public bool IsAlive()
    {
        return m_currentHealth > 0.0f;
    }

    //-------------------
    //Character death function call
    //-------------------
    public void OnDeath()
    {
        DestroyEntity();
    }

    protected override void EntityDestroyed()
    {
        m_weapon.DropWeapon();
    }

    //-------------------
    //Change characters health and check for death after health change
    //
    //Param p_value: how much to change health by
    //-------------------
    public void ModifyHealth(float p_value)
    {
        m_currentHealth += p_value;

        if (!IsAlive())
            OnDeath();
    }

    //-------------------
    //Enable to disable weapon damage
    //
    //Param p_val: true = Enabled, false = disabled
    //-------------------
    public void ToggleWeapon(bool p_val)
    {
        if(p_val)
            m_weapon.EnableWeaponCollisions();
        else
            m_weapon.DisableWeaponCollisions();
    }
}
