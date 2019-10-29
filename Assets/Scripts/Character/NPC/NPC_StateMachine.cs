using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_StateMachine : StateMachine
{
    public NPC_Character m_parentNPC = null;

    /// <summary>
    /// Store varibles for future usage
    /// </summary>
    protected override void Start()
    {
        base.Start();
        m_parentNPC = (NPC_Character)m_parentCharacter;
    }

    /// <summary>
    /// Initilise the state machine
    /// Run derived class first, add in states needed. 
    /// </summary>
    public override void InitStateMachine()
    {
        base.InitStateMachine();
    }
}
