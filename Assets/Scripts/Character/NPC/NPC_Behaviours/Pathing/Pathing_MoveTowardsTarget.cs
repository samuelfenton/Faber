using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing_MoveTowardsTarget : BehaviourNode
{
    public override RESULT Execute(Character_NPC p_character)
    {
        Navigation_Spline currentSpline = p_character.m_characterCustomPhysics.m_currentSpline;
        float desiredPecent = 0.0f;

        //Check if at end of path
        if (p_character.m_path.Count > 0 && p_character.m_path[0] == currentSpline)
            p_character.m_path.RemoveAt(0);

        Navigation_Spline goalSpline = p_character.m_path.Count > 0 ? p_character.m_path[0] : currentSpline;

        if (p_character.m_path.Count == 0)//On same spline as target
        {
            desiredPecent = p_character.m_targetCharacter.m_characterCustomPhysics.m_currentSplinePercent;
        }
        else// Move along path
        {
            if (currentSpline.m_splineStart.ContainsSpine(goalSpline)) //Determine which direction to move
                desiredPecent = 0.0f;
            else
                desiredPecent = 1.0f;
        }

        //Move based off direction to percent
        float directionDir = desiredPecent - p_character.m_characterCustomPhysics.m_currentSplinePercent;

        Vector3 splineForward = currentSpline.GetForwardsDir(transform.position).normalized;

        //Setup input
        m_characterInput_NPC.m_currentState = new CharacterInput.InputState();

        if(Vector3.Dot(transform.forward, splineForward) >=0)
        {
            m_characterInput_NPC.m_currentState.m_horizontal = directionDir >= 0 ? 1.0f : -1.0f;
        }
        else
        {
            m_characterInput_NPC.m_currentState.m_horizontal = directionDir >= 0 ? -1.0f : 1.0f;
        }


        //TODO add in jumping
        return RESULT.SUCCESS;
    }
}
