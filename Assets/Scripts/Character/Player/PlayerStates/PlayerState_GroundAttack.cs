using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_GroundAttack : Player_State
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
        if(m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK) == CustomInput.INPUT_STATE.DOWNED)//Base attack
        {
            m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK, true);
        }
        else if (m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK_SECONDARY) == CustomInput.INPUT_STATE.DOWNED)//Heavy attack
        {
            m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.HEAVY_ATTACK, true);
        }
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        return m_characterAnimationController.EndOfAnimation();
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK, false);
        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.HEAVY_ATTACK, false);
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_parentCharacter.m_splinePhysics.m_downCollision && 
            (m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK) == CustomInput.INPUT_STATE.DOWNED ||
            m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK_SECONDARY) == CustomInput.INPUT_STATE.DOWNED);
    }
}
