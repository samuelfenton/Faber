using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine_TargetDummy : StateMachine
{
    /// <summary>
    /// Initilise the state machine
    /// Run derived class first, add in states needed. 
    /// </summary>
    /// <param name="p_character">Parent character this state machine is attached to</param>
    public override void InitStateMachine(Character p_character)
    {
        //Add components
        StateTargetDummy_Knockback knockback = NewInterruptState<StateTargetDummy_Knockback>();
        StateTargetDummy_Knockforward knockforward = NewInterruptState<StateTargetDummy_Knockforward>();

        StateTargetDummy_Idle idle = NewNextState<StateTargetDummy_Idle>();

        //Init all 
        knockback.StateInit(false, p_character);
        knockforward.StateInit(false, p_character);

        idle.StateInit(true, p_character);

        //Add in next states
        //Interrrupts
        knockback.AddNextState(idle);
        knockforward.AddNextState(idle);

        //Normal
        m_currentState = idle;

        base.InitStateMachine(p_character);
    }
}
