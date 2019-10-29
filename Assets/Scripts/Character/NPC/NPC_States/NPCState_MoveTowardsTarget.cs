using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState_MoveTowardsTarget : NPC_State
{
    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public override void StateStart()
    {
        m_parentNPC.m_path = PathfindingController.GetPath(m_parentNPC, m_parentNPC.m_targetCharacter.m_characterSplinePhysics.m_currentSpline);
    }

    //-------------------
    //State update, perform any actions for the given state
    //
    //Return bool: Has this state been completed, e.g. Attack has completed, idle would always return true 
    //-------------------
    public override bool UpdateState()
    {
        Navigation_Spline currentSpline = m_parentNPC.m_characterSplinePhysics.m_currentSpline;
        float desiredPecent = 0.0f;

        //Check if at end of path
        if (m_parentNPC.m_path.Count > 0 && m_parentNPC.m_path[0] == currentSpline)
            m_parentNPC.m_path.RemoveAt(0);

        Navigation_Spline goalSpline = m_parentNPC.m_path.Count > 0 ? m_parentNPC.m_path[0] : currentSpline;

        if (m_parentNPC.m_path.Count == 0)//On same spline as target
        {
            desiredPecent = m_parentNPC.m_targetCharacter.m_characterSplinePhysics.m_currentSplinePercent;
        }
        else// Move along path
        {
            if (currentSpline.m_splineStart.ContainsSpine(goalSpline)) //Determine which direction to move
                desiredPecent = 0.0f;
            else
                desiredPecent = 1.0f;
        }

        //Move based off direction to percent
        float directionDir = desiredPecent - m_parentNPC.m_characterSplinePhysics.m_currentSplinePercent;

        Vector3 splineForward = currentSpline.GetForwardsDir(transform.position).normalized;

        //Setup input
        m_NPCInput.m_currentState = new CharacterInput.InputState();

        if (Vector3.Dot(transform.forward, splineForward) >= 0)
        {
            m_NPCInput.m_currentState.m_horizontal = directionDir >= 0 ? 1.0f : -1.0f;
        }
        else
        {
            m_NPCInput.m_currentState.m_horizontal = directionDir >= 0 ? -1.0f : 1.0f;
        }

        //TODO add in jumping
        return true;
    }
}
