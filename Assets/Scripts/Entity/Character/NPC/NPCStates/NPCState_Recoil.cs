using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState_Recoil : NPCState_Interrupt
{
    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Entity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        m_customAnimator.PlayAnimation(CustomAnimation.INTERRUPT_DEFINES.RECOIL);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        return m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.INTERRUPT);
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();

        m_character.m_recoilFlag = false; // Reset flag
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_character.m_recoilFlag && !m_inProgressFlag;
    }
}
