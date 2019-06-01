using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_StateMachine : MonoBehaviour
{
    public NPC_State m_currentState = null;
    public Character_NPC m_parentNPC = null;

    //-------------------
    //Initilise the state machine
    //
    //Param
    //      p_parentNPC: parent NPC script used
    //-------------------
    public virtual void InitStateMachine(Character_NPC p_parentNPC)
    {
        m_parentNPC = p_parentNPC;
    }

    //-------------------
    //Initilise the states in the state machine
    //-------------------
    public virtual void InitStates()
    {
        //Initilise all states
        NPC_State[] m_NPCStates = GetComponents<NPC_State>();

        foreach (NPC_State state in m_NPCStates)//Init states
        {
            state.StateInit(m_parentNPC);
            foreach (NPC_StateCondition stateCondition in state.m_NPCStateConditions)//Init state conditions
            {
                stateCondition.Init(m_parentNPC);
            }
        }

        m_currentState = m_NPCStates[0];
    }

    //-------------------
    //Run first time, similar to InitStateMachine, but runs last
    //-------------------
    public void StartStateMachine()
    {
        //Run first state
        m_currentState.StateStart();
    }

    //-------------------
    //Update state machine and check for current state completion
    //-------------------
    public void UpdateStateMachine()
    {
        if (m_currentState.UpdateState())
        {
            //Find next valid state
            foreach (NPC_State NPCState in m_currentState.m_nextStates)
            {
                if (NPCState.IsValid())
                {
                    SwapState(NPCState);
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
    private void SwapState(NPC_State p_nextState)
    {
        m_currentState.StateEnd();

        m_currentState = p_nextState;

        m_currentState.StateStart();
    }
}
