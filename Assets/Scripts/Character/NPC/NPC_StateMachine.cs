using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_StateMachine : StateMachine
{
    public NPC_Character m_parentNPC = null;


    /// <summary>
    /// Initilise the state machine
    /// Run derived class first, add in states needed. 
    /// </summary>
    /// <param name="p_parentCharacter">Parent character this state machine is attached to</param>
    public override void InitStateMachine(Character p_parentCharacter)
    {
        base.InitStateMachine(p_parentCharacter);
        m_parentNPC = (NPC_Character)m_parentCharacter;
    }
}
