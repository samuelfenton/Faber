using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_StateMachine : StateMachine
{
    /// <summary>
    /// Initilise the state machine
    /// Run derived class first, add in states needed. 
    /// </summary>
    /// <param name="p_parentCharacter">Parent character this state machine is attached to</param>
    public override void InitStateMachine(Character p_parentCharacter)
    {
        //Add components
        PlayerState_Locomotion loco = NewNextState<PlayerState_Locomotion>();
        PlayerState_Jump jump = NewNextState<PlayerState_Jump>();
        PlayerState_InAir inAir = NewNextState<PlayerState_InAir>();
        PlayerState_Land land = NewNextState<PlayerState_Land>();

        PlayerState_Attack attack = NewNextState<PlayerState_Attack>();

        //Init all 
        loco.StateInit(true, p_parentCharacter);
        jump.StateInit(false, p_parentCharacter);
        inAir.StateInit(true, p_parentCharacter);
        land.StateInit(false, p_parentCharacter);

        attack.StateInit(false, p_parentCharacter);

        //Add in next states
        loco.AddNextState(jump);
        loco.AddNextState(inAir);
        loco.AddNextState(attack);

        jump.AddNextState(inAir);
        jump.AddNextState(land);
        jump.AddNextState(loco);

        inAir.AddNextState(land);
        inAir.AddNextState(attack);

        land.AddNextState(loco);

        attack.AddNextState(inAir);
        attack.AddNextState(loco);

        base.InitStateMachine(p_parentCharacter);
    }
}
