using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAerial_Drone : Character_Aerial
{
    [Header("Assigned GameObjects")]
    public GameObject m_weaponObject = null;
    public GameObject m_projectileSpawnAnchor = null;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();
        //TODO setups statemachine here for each custom NPC 
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
