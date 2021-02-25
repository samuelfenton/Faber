using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Entity
{
    public const float MAX_FLY_TIME = 5.0f;

    [Header("Projectile Stats")]
    public float m_damage = 0.0f;
    public float m_velocity = 0.0f;
    public int m_maxCollisions = 1;

    [Header("Effects")]
    public GameObject m_startEffect = null;
    public GameObject m_flyingEffect = null;
    public GameObject m_hitEffect = null;
    public GameObject m_endEffect = null;
    public float m_endEffectTime = 1.0f;

    protected Rigidbody m_rigidbody = null;
    protected Collider m_collider = null;

    protected PoolObject m_poolParent = null;
    protected Character m_parentCharacter = null;
    protected float m_damageModifier = 1.0f;

    protected int m_collisionCount = 0;

    protected Coroutine m_endProjectileRoutine = null;
    protected Coroutine m_fixedFlyTimeRoutine = null;

    protected List<Character> m_collidedCharacters = new List<Character>();

    protected bool m_inUseFlag = false;

    /// <summary>
    /// Initiliase this object pool object
    /// </summary>
    /// <param name="p_objectPool">Parent object pool</param>
    public override void InitEntity()
    {
        base.InitEntity();

        m_rigidbody = GetComponent<Rigidbody>();
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;

        m_poolParent = GetComponent<PoolObject>();

        if (m_startEffect != null)
            m_startEffect.SetActive(false);
        if (m_flyingEffect != null)
            m_flyingEffect.SetActive(false);
        if (m_hitEffect != null)
            m_hitEffect.SetActive(false);
        if (m_endEffect != null)
            m_endEffect.SetActive(false);

        m_collider.enabled = false;
    }

    /// <summary>
    /// Update an entity, this should be called from scene controller
    /// Used to handle fixed updates, tranfroms etc
    /// </summary>
    public override void FixedUpdateEntity()
    {
        base.FixedUpdateEntity();

        if(m_inUseFlag && m_splinePhysics.AnyCollisions()) //Check if coliding with any enviroments
        {
            //Effects
            if (m_hitEffect != null)
            {
                m_hitEffect.SetActive(false);
                m_hitEffect.SetActive(true);
            }

            //No more projectile
            EndProjectile();
        }
    }

    /// <summary>
    /// Start of projectile script, example, its just been fired from gun.
    /// Assumming that roation/position is already set
    /// </summary>
    /// <param name="p_parentCharacter">Character which fired the projectile</param>
    /// <param name="p_characterToAnchorOffset">Offset form parent to determine spline percent</param>
    /// <param name="p_damageModifier">Through skills etc, damage modifer can be used, at 2.0f itll be twice the damage etc</param>
    public void ProjectileStart(Character p_parentCharacter, Vector2 p_characterToAnchorOffset, float p_damageModifier)
    {
        //Assign variables
        m_parentCharacter = p_parentCharacter;
        m_damageModifier = p_damageModifier;
        m_collisionCount = 0;

        //Setup varibles to sue
        m_inUseFlag = true;
        m_collidedCharacters.Clear();
        m_collider.enabled = true;

        //Setup spline
        m_splinePhysics.m_currentSpline = m_parentCharacter.m_splinePhysics.m_currentSpline;
        m_splinePhysics.m_nodeA = m_parentCharacter.m_splinePhysics.m_nodeA;
        m_splinePhysics.m_nodeB = m_parentCharacter.m_splinePhysics.m_nodeB;

        float splinePercent = m_splinePhysics.m_currentSpline.GetSplinePercentFromOffset(m_parentCharacter.m_splinePhysics.m_currentSplinePercent, p_characterToAnchorOffset.x);

        //TODO, if nmore then 1.0f or less then 0.0f, spawn on previous or next splines. In case of no spline, detonate immediatly
        m_splinePhysics.m_currentSplinePercent = splinePercent;
        
        //Setup position and y-pos
        transform.position = m_splinePhysics.m_currentSpline.GetPosition(splinePercent) + Vector3.up * p_characterToAnchorOffset.y;

        //Physics
        //Velocity
        float localYAxis = transform.forward.y;
        float localXAxis = Mathf.Sqrt(1 - localYAxis * localYAxis);

        Vector2 localSplineVelocity = new Vector2(localXAxis, localYAxis);
        localSplineVelocity = localSplineVelocity.normalized * m_velocity;
        m_splinePhysics.HardSetVelocity(localSplineVelocity);

        //Effects
        if (m_startEffect != null)
            m_startEffect.SetActive(true);
        if (m_flyingEffect != null)
            m_flyingEffect.SetActive(true);
        if (m_hitEffect != null)
            m_hitEffect.SetActive(false);
        if (m_endEffect != null)
            m_endEffect.SetActive(false);

        m_fixedFlyTimeRoutine = StartCoroutine(FixedFlyTimeEnd());
    }

    /// <summary>
    /// End of project, hit enemy or enviroment etc
    /// get efect and remove
    /// </summary>
    protected void EndProjectile()
    {
        m_inUseFlag = false;

        //Effects
        if (m_flyingEffect != null)
            m_flyingEffect.SetActive(false);

        if (m_endEffect != null)
            m_endEffect.SetActive(true);

        //Physcis
        m_splinePhysics.HardSetVelocity(Vector2.zero);

        m_collider.enabled = false;

        //End routines
        if (m_fixedFlyTimeRoutine != null)
        {
            StopCoroutine(m_fixedFlyTimeRoutine);
            m_fixedFlyTimeRoutine = null;
        }
        m_endProjectileRoutine = StartCoroutine(EndProjectileDelay());
    }

    /// <summary>
    /// Return/add to queue this object to the pool
    /// Called once, when returning back to pool
    /// </summary>
    public void CleanupProjectile()
    {
        //Effects
        if (m_startEffect!= null)
            m_startEffect.SetActive(false);
        if (m_flyingEffect != null)
            m_flyingEffect.SetActive(false);
        if (m_hitEffect != null)
            m_hitEffect.SetActive(false);
        if (m_endEffect != null)
            m_endEffect.SetActive(false);

        //End routines
        if (m_endProjectileRoutine != null)
        {
            StopCoroutine(m_endProjectileRoutine);
            m_endProjectileRoutine = null;
        }
        if (m_fixedFlyTimeRoutine != null)
        {
            StopCoroutine(m_fixedFlyTimeRoutine);
            m_fixedFlyTimeRoutine = null;
        }

        m_poolParent.Return();
    }


    /// <summary>
    /// Collision handling
    /// </summary>
    /// <param name="other"></param>
    protected void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == CustomLayers.m_hurtBoxLayer)//Hit other character, check if same team, otherwise deal damage
        {
            Character otherCharacter = other.GetComponent<Character>();

            if(otherCharacter != null && otherCharacter.m_team != m_parentCharacter.m_team && !m_collidedCharacters.Contains(otherCharacter)) //Hit other team character
            {
                m_collisionCount++;

                if (m_hitEffect != null)
                {
                    m_hitEffect.SetActive(false);
                    m_hitEffect.SetActive(true);
                }

                m_parentCharacter.DealDamage(otherCharacter, m_damage * m_damageModifier, transform.position);

                if (m_collisionCount >= m_maxCollisions) //Hit max characters, destory here
                    EndProjectile();

                m_collidedCharacters.Add(otherCharacter);
            }
        }
        else if(other.gameObject.layer == CustomLayers.m_enviromentLayer)//Hit enviroment, play effect
        {
            EndProjectile();
        }
    }

    /// <summary>
    /// Used to determine when the end efect has completed
    /// </summary>
    /// <returns></returns>
    protected IEnumerator EndProjectileDelay()
    {
        yield return new WaitForSeconds(m_endEffectTime);

        CleanupProjectile();
    }

    /// <summary>
    /// Used to ensure all projectiles will be returned to the object pool
    /// </summary>
    /// <returns></returns>
    protected IEnumerator FixedFlyTimeEnd()
    {
        yield return new WaitForSeconds(MAX_FLY_TIME);

        CleanupProjectile();
    }
}
