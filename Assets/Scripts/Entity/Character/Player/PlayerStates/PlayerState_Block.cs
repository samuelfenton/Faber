﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Block : State_Player
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

        m_character.m_blockingFlag = true;
        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.BLOCK, true);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        return false;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();

        m_character.m_blockingFlag = false;
        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.BLOCK, false);
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_entity.m_splinePhysics.m_downCollision && m_player.m_input.GetKeyBool(CustomInput.INPUT_KEY.BLOCK);
    }
}