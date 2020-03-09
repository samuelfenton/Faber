using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_StateMachine : StateMachine
{
    public Player_Character m_parentPlayer = null;

    /// <summary>
    /// Store varibles for future usage
    /// </summary>
    protected override void Start()
    {
        base.Start();
        m_parentPlayer = (Player_Character)m_parentCharacter;
    }

    /// <summary>
    /// Initilise the state machine
    /// Run derived class first, add in states needed. 
    /// </summary>
    public override void InitStateMachine()
    {
        //Add all components to state machine
        PlayerState_Death death = gameObject.AddComponent<PlayerState_Death>();
        
        PlayerState_Locomotion groundMovement = gameObject.AddComponent<PlayerState_Locomotion>();
        PlayerState_Jump jump = gameObject.AddComponent<PlayerState_Jump>();
        PlayerState_InAir inAir = gameObject.AddComponent<PlayerState_InAir>();
        PlayerState_Land land = gameObject.AddComponent<PlayerState_Land>();
        PlayerState_WallJump wallJump = gameObject.AddComponent<PlayerState_WallJump>();
        
        PlayerState_SingleAttack singleAttack = gameObject.AddComponent<PlayerState_SingleAttack>();

        m_interuptStates.Add(death);

        m_states.Add(groundMovement);
        m_states.Add(jump);
        m_states.Add(inAir);
        m_states.Add(land);
        m_states.Add(wallJump);
        m_states.Add(singleAttack);

        m_currentState = groundMovement;

        base.InitStateMachine();
    }
}
