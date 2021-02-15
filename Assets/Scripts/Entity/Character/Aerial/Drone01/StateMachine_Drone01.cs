using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine_Drone01 : StateMachine
{
    /// <summary>
    /// Initilise the state machine
    /// Run derived class first, add in states needed. 
    /// </summary>
    /// <param name="p_character">Parent character this state machine is attached to</param>
    public override void InitStateMachine(Character p_character)
    {
        //Add components
        StateDrone01_Death death = NewInterruptState<StateDrone01_Death>();
        StateDrone01_Knockback knockback = NewInterruptState<StateDrone01_Knockback>();

        StateDrone01_Locomotion loco = NewNextState<StateDrone01_Locomotion>();

        StateDrone01_Attack attack = NewNextState<StateDrone01_Attack>();

        StateDrone01_Idle idle = NewNextState<StateDrone01_Idle>();

        //Init all 
        death.StateInit(true, p_character);
        knockback.StateInit(false, p_character);

        loco.StateInit(true, p_character);
        idle.StateInit(false, p_character);

        attack.StateInit(false, p_character);

        //Add in next states
        //Interrrupts
        knockback.AddNextState(attack);
        knockback.AddNextState(loco);

        //Normal
        loco.AddNextState(attack);
        loco.AddNextState(idle);

        idle.AddNextState(attack);
        idle.AddNextState(loco);

        attack.AddNextState(loco);

        m_currentState = loco;

        base.InitStateMachine(p_character);
    }
}
