using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_StateMachine : StateMachine
{
    public NPC_Character m_NPCCharacter = null;

    /// <summary>
    /// Initilise the state machine
    /// Run derived class first, add in states needed. 
    /// </summary>
    /// <param name="p_character">Parent character this state machine is attached to</param>
    public override void InitStateMachine(Character p_character)
    {
        base.InitStateMachine(p_character);
        m_NPCCharacter = (NPC_Character)m_character;
    }
}
