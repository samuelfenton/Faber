using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_NPC : Character
{
    //Path finding
    protected List<Pathing_Node> m_pathingList = new List<Pathing_Node>();

    [Header("NPC Variables")]
    public Character m_target = null;
    public float m_attackDelay = 1.0f;

    [HideInInspector]
    public bool m_canAttackFlag = true;


    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();
        m_target = FindObjectOfType<Character_Player>();
    }

    /// <summary>
    /// Update an entity, this should be called from scene controller
    /// Used to handle different scene state, pause vs in game etc
    /// </summary>
    public override void UpdateEntity()
    {
        //TODO update statemachine here for each custom NPC 
        base.UpdateEntity();
    }

    /// <summary>
    /// Rotate a give weapon(GameObject towards a target)
    /// Rotates along the x local axis
    /// </summary>
    /// <param name="p_weapon"></param>
    /// <param name="p_targetPosition"></param>
    /// <param name="p_maxAngle"></param>
    public void RotateWeaponTowardsTarget(GameObject p_weapon, Vector3 p_targetPosition, float p_maxAngle = 80.0f)
    {
        Vector3 weaponToTarget = p_targetPosition - p_weapon.transform.position;

        float alignment = Vector3.Dot(transform.forward, weaponToTarget.normalized);

        //TODO make sure this lines up right
        float horizontalDis = alignment == 0.0 ? 0.0f : Mathf.Sqrt(weaponToTarget.x * weaponToTarget.x + weaponToTarget.z * weaponToTarget.z) * alignment;

        float verticalDis = weaponToTarget.y;

        float angle = Mathf.Atan(Mathf.Abs(verticalDis) / Mathf.Abs(horizontalDis)) * Mathf.Rad2Deg;

        angle = Mathf.Clamp(angle, -p_maxAngle, p_maxAngle);

        //Flip when below, looking up is negitive angle
        if (verticalDis >= 0.0f)
        {
            angle = -angle;
        }

        //Flip when trying to aim backwards
        if (alignment < 0.0f)
            angle = -180 - angle;

        p_weapon.transform.localRotation = Quaternion.Euler(angle, 0.0f, 0.0f);
    }

    /// <summary>
    /// Fire a projectile
    /// </summary>
    /// <param name="p_objectPool">Object pool to spawn projectile from</param>
    /// <param name="p_spawnPosition">Spawn Position</param>
    /// <param name="p_spawnRotation">Spawn Rotation</param>
    /// <param name="p_damageModifier">Damage modifier, defualt to 1</param>
    public void FireProjectile(ObjectPool p_objectPool, Vector3 p_spawnPosition, Quaternion p_spawnRotation, float p_damageModifier = 1.0f)
    {
        PoolObject projectilePoolObject = p_objectPool.RentObject(p_spawnPosition, p_spawnRotation);

        Projectile projectileScript = projectilePoolObject.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.ProjectileStart(this, MOARMaths.ConvertFromVector3ToVector2(p_spawnPosition - transform.position, transform.forward), p_damageModifier);
        }
    }

    /// <summary>
    /// Used to start the attacking delay
    /// </summary>
    public void StartAttackDelay()
    {
        StartCoroutine(AttackingDelayTimer());
    }

    private IEnumerator AttackingDelayTimer()
    {
        m_canAttackFlag = false;

        yield return new WaitForSeconds(m_attackDelay);

        m_canAttackFlag = true;
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
    public override ATTACK_INPUT_STANCE DetermineAttackStance()
    {
        return ATTACK_INPUT_STANCE.NONE;
    }
    #endregion
}
