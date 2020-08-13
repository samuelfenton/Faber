﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Jump : State_Player
{
    private float m_jumpVelocity = 10.0f;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Entity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);
        m_jumpVelocity = m_character.m_jumpVelocity;
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        m_character.HardSetUpwardsVelocity(m_jumpVelocity);

        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.JUMP, true);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        //Allow player to jump and move
        float horizontal = m_player.m_input.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL);
        m_character.SetDesiredVelocity(horizontal * m_character.m_groundRunVel * m_character.m_inAirModifier);

        return m_customAnimation.IsAnimationDone(CustomAnimation.LAYER.BASE) || m_entity.m_localVelocity.y <= 0.0f;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();

        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.JUMP, false);
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        //Able to jump while jump key is pressed, grounded, and no collision above
        return m_player.m_input.GetKey(CustomInput.INPUT_KEY.JUMP) == CustomInput.INPUT_STATE.DOWNED
            && m_entity.m_splinePhysics.m_downCollision && 
            !m_entity.m_splinePhysics.m_upCollision;
    }
}
