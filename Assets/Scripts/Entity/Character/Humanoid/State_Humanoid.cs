using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Humanoid : State
{
    protected CustomAnimation m_customAnimation = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Enity p_character)
    {
        base.StateInit(p_loopedState, p_character);

        m_customAnimation = p_character.GetComponent<CustomAnimation>();
    }
}
