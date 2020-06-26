using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Locomotion : State_Player
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
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        //Movement
        float horizontal = m_player.m_input.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL);

        if(m_player.m_input.GetKeyBool(CustomInput.INPUT_KEY.SPRINT))
        {
            m_entity.SetDesiredVelocity(horizontal * m_entity.m_groundRunVel * Enity.SPRINT_MODIFIER);
        }
        else
            m_entity.SetDesiredVelocity(horizontal * m_entity.m_groundRunVel);

        return false;
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
        return m_entity.m_splinePhysics.m_downCollision && !m_player.m_input.GetKeyBool(CustomInput.INPUT_KEY.BLOCK);
    }
}
