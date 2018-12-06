using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterCustomPhysics))]
[RequireComponent(typeof(CharacterStateMachine))]
public class Character : MonoBehaviour
{
    [HideInInspector]
    public CharacterCustomPhysics m_characterCustomPhysics = null;
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
    private float m_currentHealth = 10.0f;

    public Vector3 m_localVelocity = Vector3.zero;

    protected virtual void Start()
    {
#if UNITY_EDITOR
        if (m_gameController == null)
            Debug.Log("Character " + this + " hasnt had game controller selected in inspector");
#endif
        m_characterCustomPhysics = GetComponent<CharacterCustomPhysics>();
        m_characterStateMachine = GetComponent<CharacterStateMachine>();
        m_characterAnimationController = GetComponentInChildren<CharacterAnimationController>();

        m_weapon = GetComponentInChildren<Weapon>();

        m_characterStateMachine.InitStateMachine();

        m_currentHealth = m_maxHealth;
    }

    protected virtual void Update()
    {
        //Apply Velocity
        m_localVelocity.y += PhysicsController.m_gravity * Time.deltaTime;

        //Stop colliding with objects
        m_characterCustomPhysics.UpdateCollisions();

        //Check for ground collisions when falling
        if (m_localVelocity.y <= 0)
        {
            m_characterCustomPhysics.GroundCollisionCheck();
        }

        Vector3 globalVelocity = new Vector3();
        globalVelocity = transform.forward * m_localVelocity.x + transform.up * m_localVelocity.y + transform.right * m_localVelocity.z;

        transform.Translate(globalVelocity * Time.deltaTime, Space.World);

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

    public bool IsAlive()
    {
        return m_currentHealth > 0.0f;
    }

    public void OnDeath()
    {

    }

    public void ModifyHealth(float p_value)
    {
        m_currentHealth += p_value;
    }

    public void DealDamage()
    {
        Debug.Log("STAB");

        foreach (Character character in m_weapon.m_hitCharacters)
        {
            if (character.m_characterTeam != m_characterTeam)
            {
                //determine damage
                float totalDamage = m_weapon.m_weaponLightDamage;

                if (m_currentAttackType == ATTACK_TYPE.HEAVY)
                    totalDamage = m_weapon.m_weaponHeavyDamage;

                character.ModifyHealth(totalDamage);
            }
        }
    }

    public virtual void PerformCombo()
    {
    }

    public virtual NavigationController.FACING_DIR GetDesiredDirection()
    {
        return NavigationController.FACING_DIR.NORTH;
    }
}
