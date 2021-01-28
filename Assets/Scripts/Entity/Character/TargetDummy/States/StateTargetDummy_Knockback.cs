using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTargetDummy_Knockback : State_TargetDummy
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

        float knockBackVal = m_character.m_knockbackFlag ? 1.0f : -1.0f; //Assume 1 is getting hit from front
        m_customAnimator.SetVaribleFloat((int)CustomAnimation_TargetDummy.VARIBLE_FLOAT.KNOCKBACK_IMPACT, knockBackVal);

        m_customAnimator.PlayAnimation((int)CustomAnimation_TargetDummy.INTERRUPT_DEFINES.KNOCKBACK, CustomAnimation.LAYER.INTERRUPT);
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

        m_character.m_knockbackFlag = false; //Reset flag
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return (m_character.m_knockbackFlag || m_character.m_knockforwardFlag) && !m_inProgressFlag;
    }
}
