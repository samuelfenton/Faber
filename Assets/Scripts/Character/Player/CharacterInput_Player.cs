using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput_Player : CharacterInput
{
    private InputController m_inputController = null;

    private void Start()
    {
        m_inputController = GetComponent<InputController>();
    }

    public override InputState GetInputState()
    {
        InputState currentInput = new InputState();

        currentInput.m_horizontal = m_inputController.GetAxisFloat(InputController.INPUT_AXIS.HORIZONTAL);
        currentInput.m_vertical = m_inputController.GetAxisFloat(InputController.INPUT_AXIS.VERTICAL);
        currentInput.m_jump = m_inputController.GetKeyInput(InputController.INPUT_KEY.JUMP, InputController.INPUT_STATE.DOWNED);
        currentInput.m_lightAttack = m_inputController.GetKeyInput(InputController.INPUT_KEY.ATTACK, InputController.INPUT_STATE.DOWNED); 
        currentInput.m_heavyAttack = m_inputController.GetKeyInput(InputController.INPUT_KEY.ATTACK_ALT, InputController.INPUT_STATE.DOWNED);
        currentInput.m_dodge = false;

        return currentInput;
    }
}
