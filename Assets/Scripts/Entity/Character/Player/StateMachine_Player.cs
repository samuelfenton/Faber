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
        PlayerState_Death death = NewInterruptState<PlayerState_Death>();
        PlayerState_Knockback knockback = NewInterruptState<PlayerState_Knockback>();
        PlayerState_Recoil recoil = NewInterruptState<PlayerState_Recoil>();

        
        PlayerState_Locomotion loco = NewNextState<PlayerState_Locomotion>();
        PlayerState_Jump jump = NewNextState<PlayerState_Jump>();
        PlayerState_InAir inAir = NewNextState<PlayerState_InAir>();
        PlayerState_Land land = NewNextState<PlayerState_Land>();
        PlayerState_Dash dash = NewNextState<PlayerState_Dash>();
        PlayerState_InAirDash inAirDash = NewNextState<PlayerState_InAirDash>();
        PlayerState_Block block = NewNextState<PlayerState_Block>();

        PlayerState_Attack attack = NewNextState<PlayerState_Attack>();

        PlayerState_Idle idle = NewNextState<PlayerState_Idle>();
        
        //Init all 
        death.StateInit(true, p_character);
        knockback.StateInit(false, p_character);
        recoil.StateInit(false, p_character);

        loco.StateInit(true, p_character);
        jump.StateInit(false, p_character);
        inAir.StateInit(true, p_character);
        land.StateInit(false, p_character);
        dash.StateInit(false, p_character);
        inAirDash.StateInit(false, p_character);
        block.StateInit(true, p_character);
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

        inAirDash.AddNextState(inAir);
        inAirDash.AddNextState(land);

        jump.AddNextState(inAir);
        jump.AddNextState(land);
        jump.AddNextState(loco);

        inAir.AddNextState(land);
        inAir.AddNextState(attack);
        inAir.AddNextState(inAirDash);

        land.AddNextState(inAir);
        land.AddNextState(attack);
        land.AddNextState(dash);
        land.AddNextState(block);
        land.AddNextState(jump);
        land.AddNextState(loco);

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
