using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Drone01 : State
{
    protected CharacterAerial_Drone01 m_drone01 = null;
    protected CustomAnimation_Drone01 m_customAnimator = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Entity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);

        m_drone01 = (CharacterAerial_Drone01)p_entity;

        m_customAnimator = m_drone01.GetComponent<CustomAnimation_Drone01>();
    }
}
