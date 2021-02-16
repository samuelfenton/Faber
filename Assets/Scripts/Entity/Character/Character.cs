using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SplinePhysics))]
public class Character : Entity
{
    public enum KNOCKBACK_SPLINE_DIR { POSITIVE, NEGATIVE, NONE }
    public enum KNOCKBACK_DIR {FRONT, BACK, NONE }
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
    [Tooltip("Keep x positive, to push away from wall")]
    public Vector2 m_wallJumpVelocity = new Vector2(2.0f, 8.0f);

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

    [Header("Knockback")]
    public float m_knockbackRecoverTime = 1.0f;
    public float m_knockbackVelocity = 3.0f;
    public float m_knockforwardVelocity = 2.0f;

    [Header("Blocking")]
    [Range(0.0f,1.0f)]
    public float m_blockMovementModifer = 0.5f;
    //Velocity stuff
    protected Vector2 m_desiredVelocity = Vector2.zero;

    [Header("Effects")]
    public Color m_effectColor1 = Color.white;
    public Color m_effectColor2 = Color.white;

    //Flags
    [HideInInspector]
    public bool m_performedInAirAttackFlag = false;
    [HideInInspector]
    public bool m_doubleJumpFlag = false;
    [HideInInspector]
    public bool m_inAirDashFlag = false;
    [HideInInspector]
    public bool m_blockingFlag = false;
    [HideInInspector]
    public KNOCKBACK_SPLINE_DIR m_knockbackSpineDirection = KNOCKBACK_SPLINE_DIR.NONE; //What way the hit came from? This will influence moving direction
    [HideInInspector]
    public KNOCKBACK_DIR m_knockbackBodyHitDirection = KNOCKBACK_DIR.NONE; //What side of the body did the hit come from? This will influence the animation
    [HideInInspector]
    public bool m_knockbackRecoverFlag = false;
    [HideInInspector]
    public bool m_recoilFlag = false;
    [HideInInspector]
    public bool m_deathFlag = false;

    //Stored references
    [Header("Manoeuvre Prefab")]
    public GameObject m_manoeuvreControllerObjPrefab = null;
    protected GameObject m_manoeuvreControllerObjStored = null;

    [HideInInspector]
    public ManoeuvreController m_manoeuvreController = null;
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

        m_customAnimation = GetComponent<CustomAnimation>();
        m_customAnimation.Init();

        if (m_manoeuvreControllerObjPrefab != null)
        {
            m_manoeuvreControllerObjStored = Instantiate(m_manoeuvreControllerObjPrefab, transform, false);
            m_manoeuvreController = m_manoeuvreControllerObjStored.GetComponent<ManoeuvreController>();
    
            if (m_manoeuvreController != null)
                m_manoeuvreController.Init(this, m_customAnimation);//Least importance as has no dependances
        }

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
        Vector3 frameVelocity = m_splinePhysics.m_splineLocalVelocity;

        float accel = m_splinePhysics.m_downCollision ? m_groundAccel : m_groundAccel * m_inAirAccelModifier;
        float deaccel = m_splinePhysics.m_downCollision ? m_groundedDeaccel : m_groundedDeaccel * m_inAirAccelModifier;

        //Run update to velocity based off desired, only horizontal
        if (m_desiredVelocity.x == 0.0f) //Stop
        {
            float deltaHorizontalSpeed = deaccel * Time.deltaTime;
            if (deltaHorizontalSpeed > Mathf.Abs(frameVelocity.x))//Close enough to stopping this frame
                frameVelocity.x = 0.0f;
            else
                frameVelocity.x += frameVelocity.x < 0 ? deltaHorizontalSpeed : -deltaHorizontalSpeed;//Still have high velocity, just slow down
        }
        else if (m_desiredVelocity.x > 0.0f) //Run forwards
        {
            float deltaHorizontalSpeed = frameVelocity.x > m_desiredVelocity.x || frameVelocity.x < 0.0f ? deaccel * Time.deltaTime : accel * Time.deltaTime; // Desired slower then current, or current is wrong direction, use deaccel

            if (deltaHorizontalSpeed > Mathf.Abs(frameVelocity.x - m_desiredVelocity.x))
                frameVelocity.x = m_desiredVelocity.x;
            else
                frameVelocity.x += frameVelocity.x < m_desiredVelocity.x ? deltaHorizontalSpeed : -deltaHorizontalSpeed;

        }
        else //Wants to run in opposite direction
        {
            float deltaHorizontalSpeed = frameVelocity.x < m_desiredVelocity.x || frameVelocity.x > 0.0f ? deaccel * Time.deltaTime : accel * Time.deltaTime; // Desired slower then current, or current is wrong direction, use deaccel

            if (deltaHorizontalSpeed > Mathf.Abs(frameVelocity.x - m_desiredVelocity.x))
                frameVelocity.x = m_desiredVelocity.x;
            else
                frameVelocity.x += frameVelocity.x < m_desiredVelocity.x ? deltaHorizontalSpeed : -deltaHorizontalSpeed;
        }

        //Update y velocity
        float deltaVerticalSpeed = 20.0f * Time.deltaTime; //Ho much vertical velocity can occur on one frame
        float desiredChange = m_desiredVelocity.y - frameVelocity.y;

        if (deltaVerticalSpeed > Mathf.Abs(desiredChange))
            frameVelocity.y = m_desiredVelocity.y;
        else
            frameVelocity.y += frameVelocity.y < m_desiredVelocity.y ? deltaVerticalSpeed : -deltaVerticalSpeed;

        m_splinePhysics.m_splineLocalVelocity = frameVelocity;
    }

    /// <summary>
    /// Swap the facing direction
    /// Inverses velocity due to facing wrong direction 
    /// </summary>
    public virtual void SwapFacingDirection()
    {
        m_splinePhysics.m_splineLocalVelocity.x = -m_splinePhysics.m_splineLocalVelocity.x;

        Vector3 desiredForwards = m_splinePhysics.m_currentSpline.GetForwardDir(m_splinePhysics.m_currentSplinePercent);

        if (AllignedToSpline()) //Currently alligned, so face backwards
        {
            transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f) * Quaternion.LookRotation(desiredForwards, Vector3.up);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(desiredForwards, Vector3.up);
        }
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
    /// <param name="p_targetCharacter"></param>
    /// <param name="p_damage">Damage to do, should be positive</param>
    /// <param name="p_impactPosition">Position of impact</param>
    /// <param name="p_impactForce">Impact level of force</param>
    /// <param name="p_impactDirection">Direction of impact</param>
    public void DealDamage(Character p_targetCharacter, float p_damage, Vector3 p_impactPosition, Manoeuvre.DAMAGE_IMPACT p_impactForce = Manoeuvre.DAMAGE_IMPACT.MEDIUM, Manoeuvre.DAMAGE_DIRECTION p_impactDirection = Manoeuvre.DAMAGE_DIRECTION.NONE)
    {
        if (p_targetCharacter == null)
            return;

        if(p_targetCharacter.m_blockingFlag)
        {
            m_recoilFlag = true;
        }
        else
        {
            if (p_targetCharacter.m_knockbackRecoverFlag)//Target recovering from knockback, take no damage
            { 
            
            }
            else
            {
                p_targetCharacter.ModifyHealth(-p_damage);

                //Setup knockback
                //Determine knockback from direction
                Vector3 toTargetDir = p_targetCharacter.transform.position - transform.position;
                Vector3 splineForward = m_splinePhysics.m_currentSpline.GetForwardDir(m_splinePhysics.m_currentSplinePercent);

                //Spline travel direction
                float splineToTargetDirAlignment = Vector3.Dot(toTargetDir, splineForward);

                p_targetCharacter.m_knockbackSpineDirection = splineToTargetDirAlignment >= 0.0f ? KNOCKBACK_SPLINE_DIR.POSITIVE : KNOCKBACK_SPLINE_DIR.NEGATIVE; //When hit dir alligned with spline dir, push towards end, so positive

                //Determine knockback body direction
                float bodyToTargetAlignment = Vector3.Dot(toTargetDir, p_targetCharacter.transform.forward);
                p_targetCharacter.m_knockbackBodyHitDirection = bodyToTargetAlignment > 0.0f ? KNOCKBACK_DIR.BACK : KNOCKBACK_DIR.FRONT;

                p_targetCharacter.SetKnockbackImpact(p_impactForce);
                
                //Setup hit marker
                Vector3 cameraToTarget = p_targetCharacter.transform.position - m_followCamera.transform.position;
                cameraToTarget.y = 0.0f;
                Quaternion hitmarkerRotation = Quaternion.LookRotation(cameraToTarget, Vector3.up);

                m_sceneController.SpawnHitMarker(p_targetCharacter.transform.position + Vector3.up * 2.0f, hitmarkerRotation, Mathf.RoundToInt(p_damage));

                //Setup particle effects
                Vector3 characterToTarget = p_targetCharacter.transform.position - transform.position;
                characterToTarget.y = 0;//Dont care about y

                Quaternion particleRotation = Quaternion.LookRotation(characterToTarget, Vector3.up);

                m_sceneController.SpawnDamageParticles(p_impactPosition, particleRotation, p_impactDirection, p_targetCharacter.m_effectColor1, p_targetCharacter.m_effectColor2);
            }

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
    /// Update the locomotion base varibles
    /// </summary>
    public void UpdateAnimationLocomotion()
    {
        m_customAnimation.SetVaribleFloat((int)CustomAnimation_Player.VARIBLE_FLOAT.CURRENT_VELOCITY, m_splinePhysics.m_splineLocalVelocity.x / m_groundRunVel);
        m_customAnimation.SetVaribleFloat((int)CustomAnimation_Player.VARIBLE_FLOAT.ABSOLUTE_VELOCITY, Mathf.Abs(m_splinePhysics.m_splineLocalVelocity.x / m_groundRunVel));
    }

    public void SetKnockbackImpact(Manoeuvre.DAMAGE_IMPACT p_impact)
    {
        float knockbackValue = p_impact == Manoeuvre.DAMAGE_IMPACT.LOW ? 0.0f : p_impact == Manoeuvre.DAMAGE_IMPACT.MEDIUM ? 1.0f : 2.0f;

        m_customAnimation.SetVaribleFloat((int)CustomAnimation_Player.VARIBLE_FLOAT.KNOCKBACK_IMPACT, knockbackValue);
    }

    /// <summary>
    /// Set a random idle pose variable
    /// </summary>
    public void GetRandomIdlePose()
    {
        int randomPose = Random.Range(0, m_idlePoseCount);
        m_customAnimation.SetVaribleFloat((int)CustomAnimation_Player.VARIBLE_FLOAT.RANDOM_IDLE, randomPose);
    }
    
    public void StartKnockbackRecover()
    {
        m_knockbackRecoverFlag = true;

        StartCoroutine(KnockbackRecoverRoutine());
    }

    protected IEnumerator KnockbackRecoverRoutine()
    {
        yield return new WaitForSeconds(m_knockbackRecoverTime);

        m_knockbackRecoverFlag = false;
    }

    #region CHARACTER FUNCTIONS REQUIRING OVERRIDE
    /// <summary>
    /// Get turning direction for junction navigation, based off current input
    /// </summary>
    /// <param name="p_node">junction entity will pass through</param>
    /// <returns>Path entity will desire to take</returns>
    public override TURNING_DIR GetDesiredTurning(Pathing_Node p_node)
    {
        return TURNING_DIR.CENTER;
    }

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
