using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState_ComboAttack : NPC_State
{
    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    public override void StateInit()
    {

    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK, true);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        if (m_parentNPC.m_characterAnimationController.m_canCombo || m_parentNPC.m_characterAnimationController.EndOfAnimation())
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK, false);
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return CloseEnough() && m_characterAnimationController.m_canCombo;
    }
}
