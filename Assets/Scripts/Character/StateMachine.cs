using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State m_currentState = null;
    protected Character m_parentCharacter = null;
    protected List<State> m_states = new List<State>();
    protected List<State> m_interuptStates = new List<State>();

    /// <summary>
    /// Store varibles for future usage
    /// </summary>
    protected virtual void Start()
    {
        m_parentCharacter = GetComponent<Character>();
    }

    /// <summary>
    /// Initilise the state machine
    /// Run derived class first, add in states needed. 
    /// </summary>
    public virtual void InitStateMachine()
    {
        //Setup inturrupts
        for (int i = 0; i < m_interuptStates.Count; i++)
        {
            m_interuptStates[i].StateInit();
        }

        //Setup all states
        for (int i = 0; i < m_states.Count; i++)
        {
            m_states[i].StateInit();
        }

        if(m_currentState==null)//Default to first
            m_currentState = m_states[0];
    }

    /// <summary>
    /// Run first time, similar to InitStateMachine, but intended to run after
    /// </summary>
    public void StartStateMachine()
    {
        //Run first state
        m_currentState.StateStart();
    }

    /// <summary>
    /// Update state machine and check for current state completion
    /// </summary>
    public void UpdateStateMachine()
    {
        foreach (State interuptSate in m_interuptStates)
        {
            if(interuptSate.IsValid())
            {
                SwapState(interuptSate);
                return;
            }
        }

        if (m_currentState.UpdateState())
        {
            //Find next valid state
            foreach (State characterState in m_states)
            {
                if(characterState != m_currentState && characterState.IsValid())//Dont reuse state if another is valid
                {
                    SwapState(characterState);
                    return; //Early break out
                }
            }
        }
    }

    /// <summary>
    /// Swap between states, run end state, and start state functions
    /// </summary>
    /// <param name="p_nextState">next state to use</param>
    private void SwapState(State p_nextState)
    {
        m_currentState.StateEnd();

        m_currentState = p_nextState;

        m_currentState.StateStart();
    }
}
