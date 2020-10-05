using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Land : State_Player
{
    private const float SPEED_FOR_ROLLING_LANDING = 0.7f;

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

        if(Mathf.Abs(m_entity.m_splinePhysics.m_splineVelocity.x) / m_character.m_groundRunVel > SPEED_FOR_ROLLING_LANDING) //players moving fast enough to need to roll
        {
            m_customAnimator.PlayAnimation(CustomAnimation.BASE_DEFINES.LANDING_TO_RUN);
            m_character.SetDesiredVelocity(m_character.m_splinePhysics.m_splineVelocity.x);
        }
        else
        {
            m_customAnimator.PlayAnimation(CustomAnimation.BASE_DEFINES.LANDING_TO_IDLE);
            m_character.SetDesiredVelocity(0.0f);
        }
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        return true;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_entity.m_splinePhysics.m_splineVelocity.y <= 0.0f && m_entity.m_splinePhysics.m_downCollision;
    }
}
