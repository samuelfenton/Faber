using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Character : State
{
    protected CustomAnimation m_customAnimator = null;
    protected Character m_character = null;

    protected bool m_inProgressFlag = false;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_interrruptState">Is this state considered an interrupt state?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Entity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);
        m_character = (Character)p_entity;
        m_customAnimator = m_character.GetComponent<CustomAnimation>();
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        m_inProgressFlag = true;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_inProgressFlag = false;

        base.StateEnd();
    }

}
