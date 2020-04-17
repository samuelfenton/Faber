using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_State : State
{
    protected Player_Character m_playerCharacter = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);

        m_playerCharacter = (Player_Character)p_character;
    }
}
