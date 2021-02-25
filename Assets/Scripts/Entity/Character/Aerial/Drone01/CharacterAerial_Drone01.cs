using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAerial_Drone01 : Character_Aerial
{
    protected StateMachine_Drone01 m_droneStateMachine = null;

    [Header("Assigned GameObjects")]
    public GameObject m_weaponObject = null;

    [Header("Projectile")]
    public ObjectPool m_objectPoolLightProjectile = null;
    public ObjectPool m_objectPoolHeavyProjectile = null;
    public GameObject m_projectileSpawnAnchor = null;

    public float m_maxFiringAngle = 40.0f; 

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();

        m_customAnimation = GetComponent<CustomAnimation_Drone01>();

        //Init
        m_droneStateMachine = gameObject.AddComponent<StateMachine_Drone01>();

        m_droneStateMachine.InitStateMachine(this);//Run first as animation depends on states being created

        if(m_objectPoolLightProjectile != null)
        {
            m_objectPoolLightProjectile.Init();
            m_objectPoolLightProjectile.SetupAsEntities();
        }

        if (m_objectPoolHeavyProjectile != null)
        {
            m_objectPoolHeavyProjectile.Init();
            m_objectPoolHeavyProjectile.SetupAsEntities();
        }
    }

    /// <summary>
    /// Update an entity, this should be called from scene controller
    /// Used to handle different scene state, pause vs in game etc
    /// </summary>
    public override void UpdateEntity()
    {
        //Get logic
        m_droneStateMachine.UpdateStateMachine();

        base.UpdateEntity();
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
