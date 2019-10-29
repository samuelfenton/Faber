using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_State : State
{
    protected Player_StateMachine m_playerStateMachine = null;
    protected Player_Character m_parentPlayer = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    public override void StateInit()
    {
        base.StateInit();

        m_playerStateMachine = (Player_StateMachine)m_parentStateMachine;
        m_parentPlayer = (Player_Character)m_parentCharacter;
    }
}
