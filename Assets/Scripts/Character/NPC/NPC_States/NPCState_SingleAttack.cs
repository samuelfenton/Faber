using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState_SingleAttack : NPC_State
{
    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public override void StateStart()
    {
        m_NPCInput.m_currentState = new CharacterInput.InputState();

        m_NPCInput.m_currentState.m_lightAttack = true;
    }

    //-------------------
    //State update, perform any actions for the given state
    //
    //Return bool: Has this state been completed, e.g. Attack has completed, idle would always return true 
    //-------------------
    public override bool UpdateState()
    {
        if (m_parentNPC.m_characterAnimationController.m_canCombo || m_parentNPC.m_characterAnimationController.EndOfAnimation())
        {
            return true;
        }

        return false;
    }

    //-------------------
    //When swapping to a new state, this is called.
    //-------------------
    public override void StateEnd()
    {
        m_NPCInput.m_currentState = new CharacterInput.InputState();
    }
}
