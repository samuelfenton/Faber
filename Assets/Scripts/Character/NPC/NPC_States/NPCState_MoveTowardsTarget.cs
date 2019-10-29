using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState_MoveTowardsTarget : NPC_State
{
    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    public override void StateInit()
    {
        m_parentStateMachine = GetComponent<StateMachine>();
        m_parentCharacter = GetComponent<Character>();

        m_characterAnimationController = GetComponentInChildren<CharacterAnimationController>();
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        m_parentNPC.m_path = PathfindingController.GetPath(m_parentNPC, m_parentNPC.m_targetCharacter.m_splinePhysics.m_currentSpline);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        Navigation_Spline currentSpline = m_parentNPC.m_splinePhysics.m_currentSpline;

        if (m_parentNPC.m_path[m_parentNPC.m_path.Count - 1] != m_parentNPC.m_targetCharacter.m_splinePhysics.m_currentSpline)//Player has moved away from destination
        {
            m_parentNPC.m_path = new List<Navigation_Spline>();

            m_parentNPC.m_path = PathfindingController.GetPath(m_parentNPC, m_parentNPC.m_targetCharacter.m_splinePhysics.m_currentSpline);
            if(m_parentNPC.m_path.Count == 0)//Unable to get new path
                return true;
        }


        //Check if at next spline
        if (m_parentNPC.m_path.Count > 0 && m_parentNPC.m_path[0] == currentSpline)
            m_parentNPC.m_path.RemoveAt(0);

        if (m_parentNPC.m_path.Count == 0)//Made it to end spline
            return true;

        //Move along spline
        Navigation_Spline goalSpline = m_parentNPC.m_path.Count > 0 ? m_parentNPC.m_path[0] : currentSpline;
        float desiredPecent = 0.0f;

        if (currentSpline.m_splineStart.ContainsSpine(goalSpline)) //Determine which direction to move
            desiredPecent = 0.0f;
        else
            desiredPecent = 1.0f;

        //Move based off direction to percent
        float directionDir = desiredPecent - m_parentNPC.m_splinePhysics.m_currentSplinePercent;

        Vector3 splineForward = currentSpline.GetForwardsDir(transform.position).normalized;


        if (Vector3.Dot(transform.forward, splineForward) >= 0)
        {
            m_parentNPC.m_localVelocity.x = directionDir >= 0 ? 1.0f : -1.0f;
        }
        else
        {
            m_parentNPC.m_localVelocity.x = directionDir >= 0 ? -1.0f : 1.0f;
        }

        //TODO add in jumping
        return true;
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
        if (m_parentNPC.m_splinePhysics.m_currentSpline == m_parentNPC.m_targetCharacter.m_splinePhysics.m_currentSpline) //Already there
            return false;

        if (CloseEnough())//To close to need to move along spine
            return false;

        int pathCount = m_parentNPC.m_path.Count;

        if (pathCount != 0) //Already has a path
            return true;

        m_parentNPC.m_path = PathfindingController.GetPath(m_parentNPC, m_parentNPC.m_targetCharacter.m_splinePhysics.m_currentSpline);

        if (m_parentNPC.m_path.Count != 0) //Able to get new path
            return true;

        return false;
    }
}
