using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Knockback : PlayerState_Interrupt
{
    private string m_animKnockback = "";

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);

        m_animKnockback = CustomAnimation.GetInterrupt(CustomAnimation.INTERRUPT_ANIM.KNOCKBACK);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();


        CustomAnimation.PlayAnimtion(m_animator, m_animKnockback);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        return CustomAnimation.IsAnimationDone(m_animator);
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_character.m_knockbackFlag = false; //Reset flag

        base.StateEnd();
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_playerCharacter.m_knockbackFlag && !m_inProgressFlag;
    }
}
