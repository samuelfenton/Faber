﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput_NPC : CharacterInput
{
    public InputState m_currentState;

    //-------------------
    //Get the characters current input state, generated by NPC statemachine
    //
    //Return InputState: structer containing all required data
    //-------------------
    public override InputState GetInputState()
    {
        return m_currentState;
    }
}
