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
        m_NPCCharacter = (NPC_Character)p_character;

        //Add components
        NPCState_Death death = NewInterruptState<NPCState_Death>();
        NPCState_Knockback knockback = NewInterruptState<NPCState_Knockback>();
        NPCState_Recoil recoil = NewInterruptState<NPCState_Recoil>();

        NPCState_Approach approach = NewNextState<NPCState_Approach>();
        NPCState_Waiting waiting = NewNextState<NPCState_Waiting>();

        NPCState_Attack attack = NewNextState<NPCState_Attack>();
        
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
