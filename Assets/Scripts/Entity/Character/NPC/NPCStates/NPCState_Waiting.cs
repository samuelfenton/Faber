﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState_Waiting : NPC_State
{
    private string m_animIdle = "";
    private string m_paramVelocity = "";
    private string m_paramIdle = "";

    private Pathing_Spline m_patrolSpline = null;
    private float m_patrolSplinePercent = 0.0f;

    private enum WAITING_STATUS {IDLE, PATROLLING}
    private WAITING_STATUS m_currentWaitingState = WAITING_STATUS.IDLE;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);

        m_animIdle = CustomAnimation.Instance.GetLocomotion(CustomAnimation.LOCOMOTION_ANIM.IDLE);
        m_paramVelocity = CustomAnimation.Instance.GetVarible(CustomAnimation.VARIBLE_ANIM.CURRENT_VELOCITY);
        m_paramIdle = CustomAnimation.Instance.GetVarible(CustomAnimation.VARIBLE_ANIM.RANDOM_IDLE);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        SetNextStatus();

        if (m_currentWaitingState == WAITING_STATUS.IDLE)
        {
            m_character.SetRandomIdle();
            CustomAnimation.Instance.PlayAnimation(m_animator, m_animIdle);

            m_character.SetDesiredVelocity(0.0f);
        }
        else //Patrolling
        {
            //pick random spline to move to and random percent
            List<Pathing_Spline> possibleSplines = new List<Pathing_Spline>();

            possibleSplines.AddRange(m_NPCCharacter.m_patrolSplines);
            possibleSplines.Remove(m_character.m_splinePhysics.m_currentSpline); //Dont attempt to get current spline

            m_patrolSpline = possibleSplines[Random.Range(0, possibleSplines.Count)];
            m_patrolSplinePercent = Random.Range(0.2f, 0.8f);
        }
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        if (m_currentWaitingState == WAITING_STATUS.IDLE)
        {
            return CustomAnimation.Instance.IsAnimationDone(m_animator) || (m_NPCCharacter.m_targetCharacter != null && SmartTargetWithinRange(m_NPCCharacter.m_targetCharacter, m_NPCCharacter.m_detectionDistance));
        }
        else //Patrol
        {
            if (m_patrolSpline == m_character.m_splinePhysics.m_currentSpline) //Moveing towards percent
            {
                MoveTowardsPercent(m_patrolSplinePercent, m_character.m_groundRunVel / 2.0f); //Walk speed

                return Mathf.Abs(m_character.m_splinePhysics.m_currentSplinePercent - m_patrolSplinePercent) < 0.1f; //Get within 0.1 percent
            }
            else
            {
                MoveTowardsSpline(m_patrolSpline, m_character.m_groundRunVel / 2.0f);
            }
        }

        return m_NPCCharacter.m_targetCharacter != null && SmartTargetWithinRange(m_NPCCharacter.m_targetCharacter, m_NPCCharacter.m_detectionDistance);
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
        return m_NPCCharacter.m_targetCharacter == null || !SmartTargetWithinRange(m_NPCCharacter.m_targetCharacter, m_NPCCharacter.m_detectionDistance);
    }

    /// <summary>
    /// Setup the next state
    /// default to idle if patrol doesnt have enough nodes
    /// Otherwise alternate
    /// </summary>
    public void SetNextStatus()
    {
        if(m_NPCCharacter.m_patrolSplines.Count <= 1)
        {
            m_currentWaitingState = WAITING_STATUS.IDLE;
        }
        else
        {
            if (m_currentWaitingState == WAITING_STATUS.IDLE)
                m_currentWaitingState = WAITING_STATUS.PATROLLING;
            else
                m_currentWaitingState = WAITING_STATUS.IDLE;
        }
    }
}
