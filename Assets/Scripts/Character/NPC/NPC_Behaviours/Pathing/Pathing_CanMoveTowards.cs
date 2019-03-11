using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing_CanMoveTowards : BehaviourNode
{
    public override RESULT Execute(Character_NPC p_character)
    {
        int pathCount = p_character.m_path.Count;

        if (p_character.m_characterCustomPhysics.m_currentSpline == p_character.m_targetCharacter.m_characterCustomPhysics.m_currentSpline ||
           (pathCount > 0 && p_character.m_path[pathCount - 1] == p_character.m_targetCharacter.m_characterCustomPhysics.m_currentSpline)) //True when on same path or final destination is the current target spline
            return RESULT.SUCCESS;

        return RESULT.FAILED;
    }
}
