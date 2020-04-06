using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_StateMachine : StateMachine
{
    /// <summary>
    /// Initilise the state machine
    /// Run derived class first, add in states needed. 
    /// </summary>
    /// <param name="p_character">Parent character this state machine is attached to</param>
    public override void InitStateMachine(Character p_character)
    {
        //Add components
        PlayerState_Death death = NewInterruptState<PlayerState_Death>();
        PlayerState_Knockback knockback = NewInterruptState<PlayerState_Knockback>();

        PlayerState_Locomotion loco = NewNextState<PlayerState_Locomotion>();
        PlayerState_Jump jump = NewNextState<PlayerState_Jump>();
        PlayerState_InAir inAir = NewNextState<PlayerState_InAir>();
        PlayerState_Land land = NewNextState<PlayerState_Land>();

        PlayerState_Attack attack = NewNextState<PlayerState_Attack>();

        //Init all 
        death.StateInit(true, p_character);
        knockback.StateInit(false, p_character);

        loco.StateInit(true, p_character);
        jump.StateInit(false, p_character);
        inAir.StateInit(true, p_character);
        land.StateInit(false, p_character);

        attack.StateInit(false, p_character);

        //Add in next states
        knockback.AddNextState(attack);
        knockback.AddNextState(inAir);
        knockback.AddNextState(loco);

        loco.AddNextState(inAir);
        loco.AddNextState(jump);
        loco.AddNextState(attack);

        jump.AddNextState(inAir);
        jump.AddNextState(land);
        jump.AddNextState(loco);

        inAir.AddNextState(land);
        inAir.AddNextState(attack);

        land.AddNextState(loco);

        attack.AddNextState(inAir);
        attack.AddNextState(loco);

        m_currentState = loco;

        base.InitStateMachine(p_character);
    }
}
