using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Qualifier_CanCombo : BehaviourNode
{
    public override RESULT Execute(Character_NPC p_character)
    {
        if (p_character.m_characterAnimationController.m_canCombo)
        {
            return RESULT.SUCCESS;
        }
        return RESULT.FAILED;
    }
}
