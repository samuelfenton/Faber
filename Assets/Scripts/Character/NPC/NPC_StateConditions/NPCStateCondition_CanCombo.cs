using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCStateCondition_CanCombo : NPC_StateCondition
{
    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public override bool Execute(Character_NPC p_character)
    {
        return p_character.m_characterAnimationController.m_canCombo;
    }
}
