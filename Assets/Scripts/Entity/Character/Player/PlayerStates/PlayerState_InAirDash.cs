﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_InAirDash : State_Player
{
    private float m_dashVelocity = 0.0f;

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

        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.DASH, true);
        m_character.m_gravity = false;
        m_character.HardSetUpwardsVelocity(0.0f);

        //Determine direction of velocity
        if (m_character.GetFacingDir() == Character.FACING_DIR.RIGHT)
        {
            m_dashVelocity = m_player.m_dashVelocity;
        }
        else
        {
            m_dashVelocity = -m_player.m_dashVelocity;
        }

        m_character.SetDesiredVelocity(m_dashVelocity);
        m_character.HardSetVelocity(m_dashVelocity);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        //Update continously to avoid friction
        m_character.SetDesiredVelocity(m_dashVelocity);
        m_character.HardSetVelocity(m_dashVelocity);

        return m_customAnimation.IsAnimationDone(CustomAnimation.LAYER.BASE);
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();

        m_character.SetDesiredVelocity(0.0f);
        m_character.HardSetVelocity(0.0f);

        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.DASH, false);
        m_character.m_gravity = true;
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_character.m_inAirDashFlag && !m_entity.m_splinePhysics.m_downCollision && m_player.m_customInput.GetKeyBool(CustomInput.INPUT_KEY.DASH);
    }
}
