using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : MonoBehaviour
{
    protected StateMachine m_parentStateMachine = null;
    protected Character m_parentCharacter = null;

    protected CharacterAnimationController m_characterAnimationController = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    public virtual void StateInit()
    {
        m_parentStateMachine = GetComponent<StateMachine>();
        m_parentCharacter = GetComponent<Character>();

        m_characterAnimationController = GetComponentInChildren<CharacterAnimationController>();
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
    public virtual bool UpdateState()
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
}
