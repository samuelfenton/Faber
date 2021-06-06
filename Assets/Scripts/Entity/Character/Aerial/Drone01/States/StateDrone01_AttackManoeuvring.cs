using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateDrone01_AttackManoeuvring : State_Drone01
{
    public const float DISTANCE_SATISFACTION_AMOUNT = 0.25f;

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
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        if (m_drone01.m_target == null || !m_drone01.m_target.IsAlive())
            return true;

        if(m_drone01.m_canAttackFlag) //Able to attack, try pivot to state
            return true;

        float currentDistance = Pathfinding.GetDistance(m_drone01, m_drone01.m_target);

        if (currentDistance >= m_drone01.m_attackExitValue) //Moved too far out of range, move to chase
            return true;

        if(currentDistance > m_drone01.m_attackDesiredDistance + DISTANCE_SATISFACTION_AMOUNT)//Want to move closer
        {
            m_drone01.MoveTowardsEntity(m_drone01.m_target, m_drone01.m_groundRunVel * m_drone01.m_manoeuvreVelocityModifier, false);
        }
        else if (currentDistance < m_drone01.m_attackDesiredDistance - DISTANCE_SATISFACTION_AMOUNT) //Want to move further away
        {
            m_drone01.MoveTowardsEntity(m_drone01.m_target, -m_drone01.m_groundRunVel * m_drone01.m_manoeuvreVelocityModifier, false);
        }
        else //Close enough to being in the right spot
        {
            m_drone01.SetDesiredVelocity(Vector2.zero);
        }

        m_drone01.FaceTarget(m_drone01.m_target.transform.position);

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
        return !m_drone01.m_canAttackFlag && m_drone01.m_target != null && m_drone01.m_target.IsAlive() && Pathfinding.GetDistance(m_drone01, m_drone01.m_target) <= m_drone01.m_attackEnterValue;
    }
}
