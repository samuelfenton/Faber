using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_StateMachine : MonoBehaviour
{
    public NPC_State m_currentState = null;

    public virtual void InitStateMachine()
    {
        //Initilise all states
        NPC_State[] m_NPCStates = GetComponentsInChildren<NPC_State>();

        for (int i = 0; i < m_NPCStates.Length; i++)
        {
            m_NPCStates[i].StateInit();
        }
    }

    public void StartStateMachine()
    {
        //Run first state
        m_currentState.StateStart();
    }

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

    private void SwapState(NPC_State p_nextState)
    {
        m_currentState.StateEnd();

        m_currentState = p_nextState;

        m_currentState.StateStart();
    }
}
