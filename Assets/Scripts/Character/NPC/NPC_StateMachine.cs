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

        NPCState_Approach approach = NewNextState<NPCState_Approach>();
        NPCState_Patrol patrol = NewNextState<NPCState_Patrol>();
        NPCState_Idle idle = NewNextState<NPCState_Idle>();

        //NPCState_Attack attack = NewNextState<PlayerState_Land>();
        
        //Init all 
        death.StateInit(true, p_character);
        knockback.StateInit(false, p_character);

        approach.StateInit(false, p_character);
        patrol.StateInit(false, p_character);
        idle.StateInit(false, p_character);

        //attack.StateInit(false, p_character);

        //Add in next states
        //knockback.AddNextState(attack);
        knockback.AddNextState(approach);
        knockback.AddNextState(patrol);
        knockback.AddNextState(idle);


        //approach.AddNextState(attack);
        approach.AddNextState(patrol);
        approach.AddNextState(idle);

        //approach.AddNextState(attack);
        patrol.AddNextState(approach);
        patrol.AddNextState(patrol);

        //approach.AddNextState(attack);
        idle.AddNextState(approach);
        idle.AddNextState(idle);

        //attack.AddNextState(approach);
        //attack.AddNextState(patrol);
        //attack.AddNextState(idle);

        m_currentState = idle;

        base.InitStateMachine(p_character);
    }
}
