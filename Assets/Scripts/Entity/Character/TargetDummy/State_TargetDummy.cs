using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_TargetDummy : State_Character
{
    protected Character_TargetDummy m_targetDummy = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Entity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);

        m_targetDummy = (Character_TargetDummy)p_entity;
    }
}
