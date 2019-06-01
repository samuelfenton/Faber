using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_StateMachine : MonoBehaviour
{
    public NPC_State m_currentState = null;
    public Character_NPC m_parentNPC = null;

    public virtual void InitStateMachine(Character_NPC p_parentNPC)
    {
        m_parentNPC = p_parentNPC;
    }

    public virtual void InitStates()
    {
        //Initilise all states
        NPC_State[] m_NPCStates = GetComponents<NPC_State>();

        for (int i = 0; i < m_NPCStates.Length; i++)
        {
            m_NPCStates[i].StateInit(m_parentNPC);
        }

        m_currentState = m_NPCStates[0];
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
