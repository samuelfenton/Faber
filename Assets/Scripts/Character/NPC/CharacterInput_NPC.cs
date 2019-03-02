using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput_NPC : CharacterInput
{
    public InputState m_currentState;

    public override InputState GetInputState()
    {
        return m_currentState;
    }
}
