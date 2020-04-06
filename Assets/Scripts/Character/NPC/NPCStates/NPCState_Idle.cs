using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState_Idle : NPC_State
{
    private string m_animLoco = "";
    private string m_paramIdle = "";

    private Pathing_Spline m_patrolSpline = null;
    private float m_patrolSplinePercent = 0.0f;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);

        m_animLoco = AnimController.GetLocomotion(AnimController.LOCOMOTION_ANIM.LOCOMOTION);
        m_paramIdle = AnimController.GetVarible(AnimController.VARIBLE_ANIM.RANDOM_IDLE);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        m_animator.Play(m_animLoco);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        return false;
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
        return m_NPCCharacter.m_patrolSplines.Count == 0 && (m_NPCCharacter.m_targetCharacter == null || !TargetWithinRange(m_NPCCharacter.m_targetCharacter.transform.position, m_NPCCharacter.m_detectionDistance));
    }
}
