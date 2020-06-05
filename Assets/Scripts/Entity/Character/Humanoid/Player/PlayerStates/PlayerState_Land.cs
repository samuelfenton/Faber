﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Land : State_Player
{
    private string m_animLand = "";

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);
        m_animLand = m_customAnimation.GetLocomotion(CustomAnimation_Humanoid.LOCOMOTION_ANIM.LAND);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        m_customAnimation.PlayAnimation(m_animLand);
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
        base.StateEnd();
        
        m_customAnimation.EndAnimation();
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_character.m_splinePhysics.m_downCollision;
    }
}