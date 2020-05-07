using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : MonoBehaviour
{
    public List<State> m_nextStates = new List<State>();

    [HideInInspector]
    public bool m_loopedState = false;

    protected Character m_character = null;
    protected CustomAnimation_Humanoid m_customAnimation = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public virtual void StateInit(bool p_loopedState, Character p_character)
    {
        m_loopedState = p_loopedState;
        m_character = p_character;
        m_customAnimation = m_character.GetComponentInChildren<CustomAnimation_Humanoid>();
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public virtual void StateStart()
    {

    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public virtual bool StateUpdate()
    {
        return true;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public virtual void StateEnd()
    {
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public virtual bool IsValid()
    {
        return false;
    }

    public void AddNextState(State p_nextState)
    {
        m_nextStates.Add(p_nextState);
    }
}
