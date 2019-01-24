using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput_NPC : CharacterInput
{
    public override InputState GetInputState()
    {
        InputState currentInput = new InputState();

        currentInput.m_horizontal = 0.0f;
        currentInput.m_vertical = 0.0f;
        currentInput.m_jump = false;
        currentInput.m_lightAttack = false;
        currentInput.m_heavyAttack = false;
        currentInput.m_dodge = false;

        return currentInput;
    }
}
