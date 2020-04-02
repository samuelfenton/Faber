using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_State : State
{
    protected Player_Character m_parentPlayer = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_parentCharacter">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_parentCharacter)
    {
        base.StateInit(p_loopedState, p_parentCharacter);

        m_parentPlayer = (Player_Character)p_parentCharacter;
    }
}
