using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState_Patrol : NPC_State
{
    private string m_animLoco = "";
    private string m_paramVelocity = "";

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
        m_paramVelocity = AnimController.GetVarible(AnimController.VARIBLE_ANIM.CURRENT_VELOCITY);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        m_animator.Play(m_animLoco);

        //pick random spline to move to and random percent
        m_patrolSpline = m_NPCCharacter.m_patrolSplines[Random.Range(0, m_NPCCharacter.m_patrolSplines.Count)];
        m_patrolSplinePercent = Random.Range(0.1f, 0.9f);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        if(m_patrolSpline == m_character.m_splinePhysics.m_currentSpline) //Moveing towards percent
        {
            MoveTowardsPercent(m_patrolSplinePercent);

            return m_character.m_splinePhysics.m_currentSplinePercent - m_patrolSplinePercent < 0.1f; //Get within 0.1 percent
        }
        else
        {
            MoveTowardsSpline(m_patrolSpline);
        }

        return TargetWithinRange(m_NPCCharacter.m_targetCharacter.transform.position, m_NPCCharacter.m_detectionDistance);
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
        return m_NPCCharacter.m_patrolSplines.Count > 0 && (m_NPCCharacter.m_targetCharacter == null || !TargetWithinRange(m_NPCCharacter.m_targetCharacter.transform.position, m_NPCCharacter.m_detectionDistance));
    }
}
