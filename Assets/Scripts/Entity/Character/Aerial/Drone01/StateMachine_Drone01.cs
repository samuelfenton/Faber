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

        StateDrone01_Attack attack = NewNextState<StateDrone01_Attack>();
        
        StateDrone01_AttackManoeuvring attackManoeuvring = NewNextState<StateDrone01_AttackManoeuvring>();

        StateDrone01_Patrol patrol = NewNextState<StateDrone01_Patrol>();
        StateDrone01_MoveTowardsTarget moveTowardsTarget = NewNextState<StateDrone01_MoveTowardsTarget>();

        StateDrone01_Idle idle = NewNextState<StateDrone01_Idle>();

        //Init all 
        death.StateInit(true, p_character);
        knockback.StateInit(false, p_character);

        attack.StateInit(false, p_character);

        attackManoeuvring.StateInit(false, p_character);
        
        moveTowardsTarget.StateInit(false, p_character);
        patrol.StateInit(false, p_character);

        idle.StateInit(true, p_character);
        
        //Add in next states
        //Interrrupts
        knockback.AddNextState(attack);
        knockback.AddNextState(attackManoeuvring);
        knockback.AddNextState(moveTowardsTarget);
        knockback.AddNextState(idle);

        //Normal
        moveTowardsTarget.AddNextState(attack);
        moveTowardsTarget.AddNextState(attackManoeuvring);
        moveTowardsTarget.AddNextState(patrol);
        moveTowardsTarget.AddNextState(idle);

        patrol.AddNextState(attack);
        patrol.AddNextState(attackManoeuvring);
        patrol.AddNextState(moveTowardsTarget);
        patrol.AddNextState(idle);

        idle.AddNextState(attack);
        idle.AddNextState(attackManoeuvring);
        idle.AddNextState(moveTowardsTarget);
        idle.AddNextState(patrol);

        attack.AddNextState(attackManoeuvring);
        attack.AddNextState(moveTowardsTarget);
        attack.AddNextState(patrol);
        attack.AddNextState(idle);

        attackManoeuvring.AddNextState(attack);
        attackManoeuvring.AddNextState(moveTowardsTarget);
        attackManoeuvring.AddNextState(patrol);
        attackManoeuvring.AddNextState(idle);

        m_currentState = idle;

        base.InitStateMachine(p_character);
    }
}
