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
        PlayerState_Recoil recoil = NewInterruptState<PlayerState_Recoil>();

        PlayerState_Locomotion loco = NewNextState<PlayerState_Locomotion>();
        PlayerState_Jump jump = NewNextState<PlayerState_Jump>();
        PlayerState_InAir inAir = NewNextState<PlayerState_InAir>();
        PlayerState_Land land = NewNextState<PlayerState_Land>();
        PlayerState_Roll roll = NewNextState<PlayerState_Roll>();
        PlayerState_Block block = NewNextState<PlayerState_Block>();

        PlayerState_Attack attack = NewNextState<PlayerState_Attack>();

        //Init all 
        death.StateInit(true, p_character);
        knockback.StateInit(false, p_character);
        recoil.StateInit(false, p_character);

        loco.StateInit(true, p_character);
        jump.StateInit(false, p_character);
        inAir.StateInit(true, p_character);
        land.StateInit(false, p_character);
        roll.StateInit(false, p_character);
        block.StateInit(true, p_character);

        attack.StateInit(false, p_character);

        //Add in next states
        //Interrrupts
        knockback.AddNextState(attack);
        knockback.AddNextState(inAir);
        knockback.AddNextState(block);
        knockback.AddNextState(roll);
        knockback.AddNextState(loco);

        recoil.AddNextState(attack);
        recoil.AddNextState(inAir);
        recoil.AddNextState(roll);
        recoil.AddNextState(loco);

        //Normal
        loco.AddNextState(inAir);
        loco.AddNextState(attack);
        loco.AddNextState(roll);
        loco.AddNextState(block);
        loco.AddNextState(jump);

        block.AddNextState(attack);
        block.AddNextState(jump);
        block.AddNextState(inAir);
        block.AddNextState(loco);

        roll.AddNextState(attack);
        roll.AddNextState(inAir);
        roll.AddNextState(block);
        roll.AddNextState(loco);

        jump.AddNextState(inAir);
        jump.AddNextState(land);
        jump.AddNextState(loco);

        inAir.AddNextState(land);
        inAir.AddNextState(attack);

        land.AddNextState(loco);

        attack.AddNextState(inAir);
        attack.AddNextState(roll);
        attack.AddNextState(block);
        attack.AddNextState(loco);

        m_currentState = loco;

        base.InitStateMachine(p_character);
    }
}
