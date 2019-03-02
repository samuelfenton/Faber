using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing_CanMoveTowards : BehaviourNode
{
    public override RESULT Execute(Character_NPC p_character)
    {
        if (p_character.m_targetCharacter.m_characterCustomPhysics.m_currentSpline == p_character.m_characterCustomPhysics.m_currentSpline ||
           p_character.m_path.Count > 0)
            return RESULT.SUCCESS;

        return RESULT.FAILED;
    }
}
