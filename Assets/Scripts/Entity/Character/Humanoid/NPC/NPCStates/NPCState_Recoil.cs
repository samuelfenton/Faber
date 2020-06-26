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
    public override void StateInit(bool p_loopedState, Enity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        m_customAnimation.SetBool(CustomAnimation.VARIBLE_BOOL.RECOIL, true);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        return m_customAnimation.IsAnimationDone();
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_entity.m_recoilFlag = false; // Reset flag
        m_customAnimation.SetBool(CustomAnimation.VARIBLE_BOOL.RECOIL, false);

        base.StateEnd();
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_entity.m_recoilFlag && !m_inProgressFlag;
    }
}
