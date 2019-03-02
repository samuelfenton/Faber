using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathing_GetPath : BehaviourNode
{
    public override RESULT Execute(Character_NPC p_character)
    {
        p_character.m_path = PathfindingController.GetPath(p_character, p_character.m_targetCharacter.m_characterCustomPhysics.m_currentSpline);

        if(p_character.m_path.Count > 0)
            return RESULT.SUCCESS;
        return RESULT.FAILED;
    }
}
