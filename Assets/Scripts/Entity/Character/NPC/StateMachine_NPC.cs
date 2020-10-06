using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine_NPC : StateMachine
{
    public Character_NPC m_NPCCharacter = null;

    /// <summary>
    /// Initilise the state machine
    /// Run derived class first, add in states needed. 
    /// </summary>
    /// <param name="p_character">Parent character this state machine is attached to</param>
    public override void InitStateMachine(Character p_character)
    {
        m_NPCCharacter = (Character_NPC)p_character;

        //Add components
        StateNPC_Death death = NewInterruptState<StateNPC_Death>();
        StateNPC_Knockback knockback = NewInterruptState<StateNPC_Knockback>();
        StateNPC_Recoil recoil = NewInterruptState<StateNPC_Recoil>();

        StateNPC_Approach approach = NewNextState<StateNPC_Approach>();
        StateNPC_Waiting waiting = NewNextState<StateNPC_Waiting>();

        StateNPC_Attack attack = NewNextState<StateNPC_Attack>();
        
        //Init all 
        death.StateInit(true, p_character);
        knockback.StateInit(false, p_character);
        recoil.StateInit(false, p_character);

        approach.StateInit(false, p_character);
        waiting.StateInit(false, p_character);

        attack.StateInit(false, p_character);

        //Add in next states
        //Interrrupts
        knockback.AddNextState(attack);
        knockback.AddNextState(approach);
        knockback.AddNextState(waiting);

        recoil.AddNextState(attack);
        recoil.AddNextState(approach);
        recoil.AddNextState(waiting);

        //Normal
        approach.AddNextState(attack);
        approach.AddNextState(waiting);

        waiting.AddNextState(attack);
        waiting.AddNextState(approach);

        attack.AddNextState(approach);
        attack.AddNextState(waiting);

        m_currentState = waiting;

        base.InitStateMachine(p_character);
    }
}
