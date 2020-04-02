using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Land : Player_State
{
    private string m_animLand = "";

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_parentCharacter">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_parentCharacter)
    {
        base.StateInit(p_loopedState, p_parentCharacter);
        m_animLand = AnimController.GetLocomotion(AnimController.LOCOMOTION_ANIM.LAND);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        m_animator.Play(m_animLand);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        return AnimController.IsAnimationDone(m_animator);
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_parentCharacter.m_splinePhysics.m_downCollision;
    }
}
