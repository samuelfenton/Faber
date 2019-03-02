using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing_MoveTowardsTarget : BehaviourNode
{
    public float m_closeEnoughDistance = 0.1f;

    public override RESULT Execute(Character_NPC p_character)
    {
        Navigation_Spline currentSpline = p_character.m_characterCustomPhysics.m_currentSpline;
        float desiredPecent = 0.0f;

        //Check if at end of path
        if (p_character.m_path.Count > 0 && p_character.m_path[0] == currentSpline)
            p_character.m_path.RemoveAt(0);

        Navigation_Spline goalSpline = p_character.m_path.Count > 0 ? p_character.m_path[0] : currentSpline;

        //Early break if player moves to new spline
        if (goalSpline != p_character.m_path[p_character.m_path.Count - 1])
            return RESULT.FAILED;

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

        if(directionDir * currentSpline.m_splineLength < m_closeEnoughDistance)//Close enough
            return RESULT.SUCCESS;

        Vector3 splineForward = currentSpline.GetForwardsDir(transform.position);

        //Setup input
        m_characterInput_NPC.m_currentState = new CharacterInput.InputState();

        if(Vector3.Dot(transform.forward, splineForward)>=0) //Alligned correctly relative to spline
        {
            m_characterInput_NPC.m_currentState.m_horizontal = directionDir >= 0 ? -1.0f : 1.0f;
        }
        else //Facing backwards relative to spline
        {
            m_characterInput_NPC.m_currentState.m_horizontal = directionDir >= 0 ? 1.0f : -1.0f;
        }

        //TODO add in jumping

        return RESULT.PENDING;
    }
}
