using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_SingleAttack : Player_State
{
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
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK, true);
        m_parentCharacter.m_currentAttackType = Character.ATTACK_TYPE.LIGHT;
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        //Slowdown movement
        base.UpdateState();

        return m_characterAnimationController.EndOfAnimation() || m_characterAnimationController.m_canCombo;
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
        return m_parentPlayer.m_input.GetKeyBool(Input.INPUT_KEY.ATTACK) || m_parentPlayer.m_input.GetKeyBool(Input.INPUT_KEY.ATTACK_ALT);
    }
}
