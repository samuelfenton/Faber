using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateNPC_Waiting : State_NPC
{
    private Pathing_Spline m_patrolSpline;
    private float m_patrolSplinePercent = 0.0f;

    private enum WAITING_STATUS {IDLE, PATROLLING}
    private WAITING_STATUS m_currentWaitingState = WAITING_STATUS.IDLE;

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

        SetNextStatus();

        if (m_currentWaitingState == WAITING_STATUS.IDLE)
        {
            m_character.SetDesiredVelocity(Vector3.zero);
        }
        else //Patrolling
        {
            //pick random spline to move to and random percent
            List<Character_NPC.PatrolPoint> possiblePoints = new List<Character_NPC.PatrolPoint>();

            possiblePoints.AddRange(m_NPCCharacter.m_patrolPoints);

            Character_NPC.PatrolPoint randomPatrolPoint = possiblePoints[Random.Range(0, possiblePoints.Count)];

            m_patrolSpline = randomPatrolPoint.m_nodeA.GetSpline(randomPatrolPoint.m_nodeB);
            m_patrolSplinePercent = randomPatrolPoint.m_splinePercent;
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
            return m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.BASE) || (m_NPCCharacter.m_targetCharacter != null && SmartTargetWithinRange(m_NPCCharacter.m_targetCharacter, m_NPCCharacter.m_detectionDistance));
        }
        else //Patrol
        {
            if (m_patrolSpline == m_entity.m_splinePhysics.m_currentSpline) //Moveing towards percent
            {
                MoveTowardsPercent(m_patrolSplinePercent, m_character.m_groundRunVel / 2.0f); //Walk speed

                return Mathf.Abs(m_entity.m_splinePhysics.m_currentSplinePercent - m_patrolSplinePercent) < 0.1f; //Get within 0.1 percent
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
        if(m_NPCCharacter.m_patrolPoints.Count <= 1)
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
