using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Player : State
{
    protected Humanoid_Player m_player = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);

        m_player = (Humanoid_Player)p_character;
    }
}
