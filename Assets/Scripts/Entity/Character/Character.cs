using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Entity
{
    public enum FACING_DIR {RIGHT, LEFT}
    public enum TEAM { PLAYER, NPC, GAIA }

    public const float SPRINT_MODIFIER = 1.5f;//Linked to animator

    [Header("Assigned Character Varibles")]
    public GameObject m_characterModel = null;
    public GameObject m_rightHand = null;
    public GameObject m_leftHand = null;

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
    public float m_jumpSpeed = 10.0f;
    public float m_landingDistance = 1.0f;

    [Header("In Air Stats")]
    public float m_inAirAccel = 0.5f;
    public float m_doubleJumpSpeed = 6.0f;

    [Header("Wall Jump Stats")]
    public float m_wallJumpVerticalSpeed = 5.0f;
    public float m_wallJumpHorizontalSpeed = 2.0f;
    public float m_wallJumpInputDelay = 0.1f;

    [Header("Roll Backwards Stats")]
    public float m_rollbackVelocity = 12.0f;

    private string m_animRandomIdle = "";

    //Velocity stuff
    private float m_desiredVelocity = 0.0f;

    private string m_animCurrentVel = "";
    private string m_animDesiredVel = "";
    private string m_animAbsVel = "";

    //Flags
    public bool m_blockingFlag = false;
    public bool m_knockbackFlag = false;
    public bool m_recoilFlag = false;
    public bool m_deathFlag = false;

    //Stored references
    protected WeaponManager m_weaponManager = null;
    protected Animator m_animator = null;
    protected ObjectPoolManager_InGame m_objectPoolingManger = null;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();

        //Get references
        m_weaponManager = GetComponent<WeaponManager>();
        m_animator = m_characterModel.GetComponent<Animator>();

        if (m_weaponManager != null)
            m_weaponManager.Init(this);//Least importance as has no dependances

        m_objectPoolingManger = GameObject.FindGameObjectWithTag(CustomTags.GAME_CONTROLLER).GetComponent<ObjectPoolManager_InGame>();

        m_currentHealth = m_maxHealth;

        m_animRandomIdle = CustomAnimation.Instance.GetVarible(CustomAnimation.VARIBLE_ANIM.RANDOM_IDLE);

        m_animCurrentVel = CustomAnimation.Instance.GetVarible(CustomAnimation.VARIBLE_ANIM.CURRENT_VELOCITY);
        m_animDesiredVel = CustomAnimation.Instance.GetVarible(CustomAnimation.VARIBLE_ANIM.DESIRED_VELOCITY);
        m_animAbsVel = CustomAnimation.Instance.GetVarible(CustomAnimation.VARIBLE_ANIM.ABSOLUTE_VELOCITY);

    }

    protected override void Update()
    {
        base.Update();

        Vector3 newVelocity = m_localVelocity;

        float accel = m_splinePhysics.m_downCollision ? m_groundAccel : m_inAirAccel;
        float deaccel = m_splinePhysics.m_downCollision ? m_groundedDeaccel : m_inAirAccel;

        //Update velocity
        //Check for walls
        if (m_localVelocity.x < 0.0f && m_desiredVelocity < 0.0f && m_splinePhysics.m_backCollision)
        {
            m_desiredVelocity = 0.0f;
        }
        if (m_localVelocity.x > 0.0f && m_desiredVelocity > 0.0f && m_splinePhysics.m_forwardCollision)
        {
            m_desiredVelocity = 0.0f;
        }

        //Run update to velocity based off desired
        if (m_desiredVelocity == 0.0f) //Stop
        {
            float deltaSpeed = deaccel * Time.deltaTime;
            if (deltaSpeed > Mathf.Abs(newVelocity.x))//Close enough to stopping this frame
                newVelocity.x = 0.0f;
            else
                newVelocity.x += newVelocity.x < 0 ? deltaSpeed : -deltaSpeed;//Still have high velocity, just slow down
        }
        else if(m_desiredVelocity > 0.0f) //Run forwards
        {
            float deltaSpeed = m_localVelocity.x > m_desiredVelocity || m_localVelocity.x < 0.0f ? deaccel * Time.deltaTime : accel * Time.deltaTime; // how much speed will change?

            if (deltaSpeed > Mathf.Abs(newVelocity.x - m_desiredVelocity))
                newVelocity.x = m_desiredVelocity;
            else
                newVelocity.x += m_localVelocity.x < m_desiredVelocity ? deltaSpeed : -deltaSpeed;

        }
        else //Run backwards
        {
            float deltaSpeed = m_localVelocity.x < m_desiredVelocity || m_localVelocity.x > 0.0f ? deaccel * Time.deltaTime : accel * Time.deltaTime; // how much speed will change?

            if (deltaSpeed > Mathf.Abs(newVelocity.x - m_desiredVelocity))
                newVelocity.x = m_desiredVelocity;
            else
                newVelocity.x += m_localVelocity.x > m_desiredVelocity ? -deltaSpeed : deltaSpeed;
        }

        m_localVelocity = newVelocity;

        UpdateAnimationLocomotion();

        //Setup rotation on game model, completly aesthetic based
        if (m_localVelocity.x > 0.1f)
        {
            FaceDirection(FACING_DIR.RIGHT);
        }
        else if (m_localVelocity.x < -0.1f)
        {
            FaceDirection(FACING_DIR.LEFT);
        }
    }

    /// <summary>
    /// Make model face left or right
    /// </summary>
    /// <param name="p_facingDir"></param>
    public void FaceDirection(FACING_DIR p_facingDir)
    {
        if(p_facingDir == FACING_DIR.RIGHT)
        {
            m_characterModel.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        }
        else
        {
            m_characterModel.transform.localRotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }
    }

    /// <summary>
    /// Get the current facnig direction
    /// </summary>
    /// <returns>Right when forward is same for model and base object</returns>
    public FACING_DIR GetFacingDir()
    {
        float alignedModelDot = Vector3.Dot(transform.forward, m_characterModel.transform.forward);

        if (alignedModelDot >= 0.0f)
            return FACING_DIR.RIGHT;
        return FACING_DIR.LEFT;
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
    /// <param name="p_value"></param>
    /// <param name="p_targetCharacter"></param>
    public void DealDamage(float p_value, Character p_targetCharacter)
    {
        if (p_targetCharacter == null)
            return;

        if(p_targetCharacter.m_blockingFlag)
        {
            m_recoilFlag = true;
        }
        else
        {
            p_targetCharacter.ModifyHealth(-p_value);
            p_targetCharacter.m_knockbackFlag = true;

            m_objectPoolingManger.SpawnHitMarker(p_targetCharacter.transform.position + Vector3.up * 2.0f, Quaternion.identity, Mathf.RoundToInt(p_value));
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
    /// <param name="p_val">Desired velocity, will automatically get clamped</param>
    public void SetDesiredVelocity(float p_val)
    {
        m_desiredVelocity = Mathf.Clamp(p_val, -m_groundRunVel * SPRINT_MODIFIER, m_groundRunVel * SPRINT_MODIFIER);
    }

    /// <summary>
    /// Hard set the value of velocity
    /// </summary>
    /// <param name="p_val">velocity</param>
    public void HardSetVelocity(float p_val)
    {
        m_localVelocity.x = p_val;
    }

    /// <summary>
    /// Hard set the value of velocity
    /// </summary>
    /// <param name="p_val">velocity</param>
    public void HardSetUpwardsVelocity(float p_val)
    {
        m_localVelocity.y = p_val;
    }

    /// <summary>
    /// Update the locomotion base varibles
    /// </summary>
    public void UpdateAnimationLocomotion()
    {
        CustomAnimation.Instance.SetVarible(m_animator, m_animCurrentVel, m_localVelocity.x / m_groundRunVel);
        CustomAnimation.Instance.SetVarible(m_animator, m_animDesiredVel, m_desiredVelocity / m_groundRunVel);
        CustomAnimation.Instance.SetVarible(m_animator, m_animAbsVel, Mathf.Abs(m_localVelocity.x / m_groundRunVel));
    }

    /// <summary>
    /// Set a random idle value
    /// Should be set at start of aniamtion before play is called
    /// </summary>
    public void SetRandomIdle()
    {
        int idleIndex = Random.Range(0, CustomAnimation.IDLE_COUNT);
        CustomAnimation.Instance.SetVarible(m_animator, m_animRandomIdle, idleIndex);
    }

    #region WEAPON FUNCTIONS - OVERRIDE

    /// <summary>
    /// Function desired to be overridden, should this managed have a possitive light attack input? 
    /// Example click by player, or logic for NPC
    /// </summary>
    /// <returns>True when theres desired light attack input</returns>
    public virtual bool DetermineLightInput()
    {
        return false;
    }

    /// <summary>
    /// Function desired to be overridden, should this managed have a possitive heavy attack input? 
    /// Example click by player, or logic for NPC
    /// </summary>
    /// <returns>True when theres desired heavy attack input</returns>
    public virtual bool DetermineHeavyInput()
    {
        return false;
    }
    #endregion
}
