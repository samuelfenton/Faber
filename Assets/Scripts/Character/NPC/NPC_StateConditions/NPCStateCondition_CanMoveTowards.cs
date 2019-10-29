using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateCondition_CanMoveTowards : NPC_StateCondition
{
    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public override bool Execute(Character_NPC p_character)
    {
        int pathCount = p_character.m_path.Count;

        if (p_character.m_characterSplinePhysics.m_currentSpline == p_character.m_targetCharacter.m_characterSplinePhysics.m_currentSpline ||
           (pathCount > 0 && p_character.m_path[pathCount - 1] == p_character.m_targetCharacter.m_characterSplinePhysics.m_currentSpline)) //True when on same path or final destination is the current target spline
            return true;

        return false;
    }
}
