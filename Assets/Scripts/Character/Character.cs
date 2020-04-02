using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Entity
{
    [Header("Assigned Character Varibles")]
    public GameObject m_characterModel = null;
    public GameObject m_rightHand = null;
    public GameObject m_leftHand = null;

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

    [Header("Character Stats")]
    public float m_maxHealth = 10.0f;
    [SerializeField]
    private float m_currentHealth = 10.0f;

    protected CharacterInventory m_characterInventory = null;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();

        //Get references
        m_characterInventory = GetComponent<CharacterInventory>();

        if (m_characterInventory != null)
            m_characterInventory.InitInventory(this);//Least importance as has no dependances

        m_currentHealth = m_maxHealth;
    }

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

    public WeaponManager GetCurrentWeapon()
    {
        return m_characterInventory.GetCurrentWeapon();
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
    }
}
