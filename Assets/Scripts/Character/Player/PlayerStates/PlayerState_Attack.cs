using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Attack : Player_State
{
    private WeaponManager m_weaponManager = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_parentCharacter">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_parentCharacter)
    {
        base.StateInit(p_loopedState, p_parentCharacter);

    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        m_weaponManager = m_parentCharacter.GetCurrentWeapon();
        m_weaponManager.StartAttackSequence();
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        return m_weaponManager.UpdateAttackSequence();
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK) == CustomInput.INPUT_STATE.DOWNED || m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK_SECONDARY) == CustomInput.INPUT_STATE.DOWNED;
    }
}
