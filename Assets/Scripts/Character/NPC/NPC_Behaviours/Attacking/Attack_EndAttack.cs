using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack_EndAttack : BehaviourNode
{
    public override RESULT Execute(Character_NPC p_character)
    {
        //Setup input
        m_characterInput_NPC.m_currentState = new CharacterInput.InputState();
        if (p_character.m_characterAnimationController.EndOfAnimation())
        {
            return RESULT.SUCCESS;
        }
        return RESULT.PENDING;
    }
}
