using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Entity
{
    [HideInInspector]
    public CharacterAnimationController m_characterAnimationController = null;

    [Header("Basic Character Varibles")]
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
    public float m_landingDistance = 1.0f;

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

    protected CharacterInventory m_characterInventory = null;

    //-------------------
    //Character setup
    //  Ensure all need componets are attached, and get initilised if needed
    //-------------------
    public override void InitEntity()
    {
        base.InitEntity();

        //Get references
        m_gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        m_characterAnimationController = GetComponentInChildren<CharacterAnimationController>();
        m_characterInventory = GetComponent<CharacterInventory>();

        if(m_characterAnimationController!=null)
            m_characterAnimationController.InitAnimationController();
        if (m_characterInventory != null)
            m_characterInventory.InitInventory();//Least importance as has no dependances

        //Secondary references
        m_weapon = GetComponentInChildren<Weapon>();

        if(m_weapon != null)
        {
            m_weapon.m_parentCharacter = this;
        }

        m_currentHealth = m_maxHealth;
    }

    //-------------------
    //Character update
    //  Get input, apply physics, update character state machine
    //-------------------
    protected override void Update()
    {
        base.Update();

        //Setup rotation on game model, completly aesthetic based
        if (m_localVelocity.x > 0.1f)
        {
            m_characterModel.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }
        else if (m_localVelocity.x < -0.1f)
        {
            m_characterModel.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }
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

    /// <summary>
    /// Apply friction to a charcter till it stops
    /// </summary>
    public void ApplyFriction()
    {
        Vector3 newVelocity = m_localVelocity;

        float deltaSpeed = m_groundedHorizontalDeacceleration * Time.deltaTime;
        if (deltaSpeed > Mathf.Abs(newVelocity.x))//Close enough to stopping this frame
            newVelocity.x = 0.0f;
        else
            newVelocity.x += newVelocity.x < 0 ? deltaSpeed : -deltaSpeed;//Still have high velocity, just slow down

        m_localVelocity = newVelocity;

        m_characterAnimationController.SetVarible(CharacterAnimationController.VARIBLES.CURRENT_VELOCITY, Mathf.Abs(newVelocity.x / m_groundedHorizontalSpeedMax));
    }
}
