using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine : MonoBehaviour
{
    public enum STATE {IDLE, GROUND, IN_AIR, JUMP, LAND, ATTACK, WALL_JUMP, DEATH }
    public STATE m_currentStateType = STATE.IDLE;

    public CharacterState m_currentState = null;

    //-------------------
    //Initilise the state machine
    //-------------------
    public virtual void InitStateMachine()
    {
        //Initilise all states
        CharacterState[] m_characterStates = GetComponentsInChildren<CharacterState>();

        for (int i = 0; i < m_characterStates.Length; i++)
        {
            m_characterStates[i].StateInit();
        }
    }

    //-------------------
    //Run first time, similar to InitStateMachine, but runs last
    //-------------------
    public void StartStateMachine()
    {
        //Run first state
        m_currentState.StateStart();
        m_currentStateType = m_currentState.m_stateType;
    }

    //-------------------
    //Update state machine and check for current state completion
    //-------------------
    public void UpdateStateMachine()
    {
        if (m_currentState.UpdateState())
        {
            //Find next valid state
            foreach (CharacterState characterState in m_currentState.m_nextStates)
            {
                if(characterState.IsValid())
                {
                    SwapState(characterState);
                    return; //Early break out
                }
            }
        }
    }

    //-------------------
    //Swap between states, run end state, and start state functions
    //
    //Param
    //      p_nextState: next state to use
    //-------------------
    private void SwapState(CharacterState p_nextState)
    {
        m_currentState.StateEnd();

        m_currentState = p_nextState;

        m_currentState.StateStart();
        m_currentStateType = m_currentState.m_stateType;
    }
}
