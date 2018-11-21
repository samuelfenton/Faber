using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine : MonoBehaviour
{
    public CharacterState m_currentState = null;

    public void InitStateMachine()
    {
        //Initilise all states
        CharacterState[] m_characterStates = GetComponentsInChildren<CharacterState>();

        for (int i = 0; i < m_characterStates.Length; i++)
        {
            m_characterStates[i].StateInit();
        }

        //Run first state
        m_currentState.StateStart();
    }

    public void UpdateStateMachine()
    {
        if(m_currentState.UpdateState())
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

    private void SwapState(CharacterState p_nextState)
    {
        m_currentState.StateEnd();
        m_currentState = p_nextState;
        m_currentState.StateStart();
    }
}
