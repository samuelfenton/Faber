﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_WeaponManager : WeaponManager
{
    //Stored varibles
    private Player_Character m_playerCharacter = null;

    /// <summary>
    /// Init manager
    /// </summary>
    /// <param name="p_character">Character that uses this manager</param>
    public override void Init(Character p_character)
    {
        base.Init(p_character);
        m_playerCharacter = p_character.GetComponent<Player_Character>();
    }

    #region OVERRIDE FUNCTIONS

    /// <summary>
    /// Function desired to be overridden, should this managed have a possitive light attack input? 
    /// Example click by player, or logic for NPC
    /// </summary>
    /// <returns>True when theres desired light attack input</returns>
    public override bool DetermineLightInput()
    {
        return m_playerCharacter.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK) == CustomInput.INPUT_STATE.DOWNED;
    }

    /// <summary>
    /// Function desired to be overridden, should this managed have a possitive heavy attack input? 
    /// Example click by player, or logic for NPC
    /// </summary>
    /// <returns>True when theres desired heavy attack input</returns>
    public override bool DetermineHeavyInput()
    {
        return m_playerCharacter.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK_SECONDARY) == CustomInput.INPUT_STATE.DOWNED;
    }

    /// <summary>
    /// Determine what attack type should be performed?
    /// grounded and sprinting = sprinting
    /// just grounded = grounded
    /// in the air = in_air
    /// </summary>
    /// <returns>Correct type, defualt to grounded</returns>
    public override AnimController.ATTACK_TYPE DetermineAttackType()
    {
        if (!m_character.m_splinePhysics.m_downCollision) //In the air
        {
            return AnimController.ATTACK_TYPE.IN_AIR;
        }
        if (m_character.m_localVelocity.x > m_character.m_groundHorizontalMax)//Is it sprinting or just grounded
            return AnimController.ATTACK_TYPE.SPRINTING;

        return AnimController.ATTACK_TYPE.GROUND;
    }

    /// <summary>
    /// Determine attacking stance, light or heavy
    /// </summary>
    /// <returns>Based off input</returns>
    public override AnimController.ATTACK_STANCE DetermineStance()
    {
        return m_playerCharacter.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK) == CustomInput.INPUT_STATE.DOWNED ? AnimController.ATTACK_STANCE.LIGHT : AnimController.ATTACK_STANCE.HEAVY;
    }
    #endregion
}
