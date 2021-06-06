using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateDrone01_MoveTowardsTarget : State_Drone01
{
    private Pathing_Spline m_currentSpline = null;

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

        m_customAnimator.PlayAnimation((int)CustomAnimation_Drone01.BASE_DEFINES.LOCOMOTION, CustomAnimation.LAYER.BASE, CustomAnimation.BLEND_TIME.INSTANT);

        m_currentSpline = m_drone01.m_splinePhysics.m_currentSpline;
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        //Check if done
        if(Pathfinding.GetDistance(m_drone01, m_drone01.m_target) <= m_drone01.m_attackEnterValue)
        {
            return true;
        }

        if (!m_drone01.MoveTowardsEntity(m_drone01.m_target, m_drone01.m_groundRunVel, true))
        {
            return true; //Unable to move towards target
        }

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
        if (m_drone01.m_target == null || !m_drone01.m_target.IsAlive()) //No valid targets
            return false;

        if (Pathfinding.GetDistance(m_drone01, m_drone01.m_target) > m_drone01.m_approachDistance) //Too far to approach
            return false;

        if (m_drone01.m_splinePhysics.m_currentSpline == m_drone01.m_target.m_splinePhysics.m_currentSpline) //If same spline, move closer
            return true;

        //Different splines, path find closer
        if (Pathfinding.ValidPath(m_drone01.m_pathingList, m_drone01, m_drone01.m_target))//Valid target
            return true;

        m_drone01.m_pathingList = Pathfinding.GetPath(m_drone01, m_drone01.m_target.m_splinePhysics.m_currentSpline); //Attempt to get new path

        return Pathfinding.ValidPath(m_drone01.m_pathingList, m_drone01, m_drone01.m_target);//Valid target
    }
}
