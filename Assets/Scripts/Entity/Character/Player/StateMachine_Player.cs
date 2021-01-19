using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine_Player : StateMachine
{
    /// <summary>
    /// Initilise the state machine
    /// Run derived class first, add in states needed. 
    /// </summary>
    /// <param name="p_character">Parent character this state machine is attached to</param>
    public override void InitStateMachine(Character p_character)
    {
        //Add components
        StatePlayer_Death death = NewInterruptState<StatePlayer_Death>();
        StatePlayer_Knockback knockback = NewInterruptState<StatePlayer_Knockback>();
        StatePlayer_Recoil recoil = NewInterruptState<StatePlayer_Recoil>();

        StatePlayer_Locomotion loco = NewNextState<StatePlayer_Locomotion>();
        StatePlayer_Jump jump = NewNextState<StatePlayer_Jump>();
        StatePlayer_InAir inAir = NewNextState<StatePlayer_InAir>();
        StatePlayer_Land land = NewNextState<StatePlayer_Land>();
        StatePlayer_WallJump wallJump = NewNextState<StatePlayer_WallJump>();
        StatePlayer_Dash dash = NewNextState<StatePlayer_Dash>();
        StatePlayer_InAirDash inAirDash = NewNextState<StatePlayer_InAirDash>();
        StatePlayer_Block block = NewNextState<StatePlayer_Block>();

        StatePlayer_Attack attack = NewNextState<StatePlayer_Attack>();

        StatePlayer_Idle idle = NewNextState<StatePlayer_Idle>();
        
        //Init all 
        death.StateInit(true, p_character);
        knockback.StateInit(false, p_character);
        recoil.StateInit(false, p_character);

        loco.StateInit(true, p_character);
        jump.StateInit(false, p_character);
        inAir.StateInit(true, p_character);
        wallJump.StateInit(false, p_character);
        land.StateInit(false, p_character);
        dash.StateInit(false, p_character);
        inAirDash.StateInit(false, p_character);
        block.StateInit(false, p_character);
        idle.StateInit(false, p_character);

        attack.StateInit(false, p_character);

        //Add in next states
        //Interrrupts
        knockback.AddNextState(attack);
        knockback.AddNextState(inAir);
        knockback.AddNextState(block);
        knockback.AddNextState(jump);
        knockback.AddNextState(loco);

        recoil.AddNextState(attack);
        recoil.AddNextState(inAir);
        recoil.AddNextState(dash);
        recoil.AddNextState(jump);
        recoil.AddNextState(loco);

        //Normal
        loco.AddNextState(inAir);
        loco.AddNextState(attack);
        loco.AddNextState(dash);
        loco.AddNextState(block);
        loco.AddNextState(jump);
        loco.AddNextState(idle);

        block.AddNextState(attack);
        block.AddNextState(jump);
        block.AddNextState(inAir);
        block.AddNextState(loco);

        dash.AddNextState(inAir);
        dash.AddNextState(loco);

        inAirDash.AddNextState(wallJump);
        inAirDash.AddNextState(inAir);
        inAirDash.AddNextState(land);

        jump.AddNextState(wallJump);
        jump.AddNextState(inAir);
        jump.AddNextState(land);

        inAir.AddNextState(wallJump);
        inAir.AddNextState(land);
        inAir.AddNextState(attack);
        inAir.AddNextState(inAirDash);

        land.AddNextState(inAir);
        land.AddNextState(attack);
        land.AddNextState(dash);
        land.AddNextState(block);
        land.AddNextState(jump);
        land.AddNextState(loco);

        wallJump.AddNextState(inAir);
        wallJump.AddNextState(land);

        idle.AddNextState(attack);
        idle.AddNextState(inAir);
        idle.AddNextState(jump);
        idle.AddNextState(loco);

        attack.AddNextState(inAir);
        attack.AddNextState(block);
        attack.AddNextState(loco);

        m_currentState = loco;

        base.InitStateMachine(p_character);
    }
}
