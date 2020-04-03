using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [HideInInspector]
    public State m_currentState = null;
    protected Character m_character = null;

    [HideInInspector]
    public List<State> m_childStates = new List<State>();

    [HideInInspector]
    public List<State> m_childInterruptStates = new List<State>();

    /// <summary>
    /// Initilise the state machine
    /// Run derived class first, add in states needed. 
    /// </summary>
    /// <param name="p_character">Parent character this state machine is attached to</param>
    public virtual void InitStateMachine(Character p_character)
    {
        m_character = p_character;

        if(m_currentState==null && m_childStates.Count > 0)//Default to first
            m_currentState = m_childStates[0];

        if(m_currentState == null)//Still null, state machine isnt working
        {
#if UNITY_EDITOR
            Debug.Log(name + "'s state machine has no assinged states");
#endif
            enabled = false;
        }
        else
        {
            m_currentState.StateStart();
        }
    }

    /// <summary>
    /// Update state machine and check for current state completion
    /// </summary>
    public void UpdateStateMachine()
    {
        foreach (State interruptSate in m_childInterruptStates)
        {
            if(interruptSate.IsValid())
            {
                SwapState(interruptSate);
                return;
            }
        }

        bool finishedState = m_currentState.UpdateState();

        if (finishedState || m_currentState.m_loopedState)
        {
            //Find next valid state
            foreach (State nextState in m_currentState.m_nextStates)
            {
                if(nextState.IsValid())//Dont reuse state if another is valid
                {
                    SwapState(nextState);
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

    /// <summary>
    /// Create a new state component and get it initilised for next list
    /// </summary>
    /// <typeparam name="T">What type of component is it?</typeparam>
    /// <returns>Ref to created object</returns>
    protected T NewNextState<T>() where T : State
    {
        T newState = gameObject.AddComponent<T>();
        
        m_childStates.Add(newState);
        
        return newState;
    }

    /// <summary>
    /// Create a new state component and get it initilised for interrupt list
    /// </summary>
    /// <typeparam name="T">What type of component is it?</typeparam>
    /// <returns>Ref to created object</returns>
    protected T NewInterruptState<T>() where T : State
    {
        T newState = gameObject.AddComponent<T>();

        m_childInterruptStates.Add(newState);

        return newState;
    }

}
