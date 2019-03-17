using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacking_SingleAttack : BehaviourNode
{
    private bool m_trigger = false;
    public override RESULT Execute(Character_NPC p_character)
    {
        //Setup input
        m_characterInput_NPC.m_currentState = new CharacterInput.InputState();

        if(!m_trigger)
        {
            m_characterInput_NPC.m_currentState.m_lightAttack = true;
            m_trigger = true;
        }
        else
        {
            if (p_character.m_characterAnimationController.EndOfAnimation())
            {
                m_trigger = false;
                return RESULT.SUCCESS;
            }
        }
        return RESULT.PENDING;
    }
}
