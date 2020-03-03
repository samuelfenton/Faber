using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Combo : Player_State
{
    private bool m_delayTrigger = false;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    public override void StateInit()
    {
        base.StateInit();
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK_COMBO, true);
        m_parentCharacter.m_currentAttackType = Character.ATTACK_TYPE.LIGHT;

        m_delayTrigger = false;
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        //Slowdown movement
        base.UpdateState();

        if(!m_delayTrigger && !(m_characterAnimationController.EndOfAnimation() || m_characterAnimationController.m_canCombo))//Wait for everything to be set to false
        {
            m_delayTrigger = true;
        }

        return m_delayTrigger && (m_characterAnimationController.EndOfAnimation() || m_characterAnimationController.m_canCombo);
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK_COMBO, false);
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return (m_parentPlayer.m_input.GetKeyBool(CustomInput.INPUT_KEY.ATTACK) || m_parentPlayer.m_input.GetKeyBool(CustomInput.INPUT_KEY.ATTACK_SECONDARY)) && m_characterAnimationController.m_canCombo;
    }
}
