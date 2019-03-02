using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput_Player : CharacterInput
{
    private PlayerInputController m_inputController = null;

    private void Start()
    {
        m_inputController = GetComponent<PlayerInputController>();
    }

    public override InputState GetInputState()
    {
        InputState currentInput = new InputState();

        currentInput.m_horizontal = m_inputController.GetAxisFloat(PlayerInputController.INPUT_AXIS.HORIZONTAL);
        currentInput.m_vertical = m_inputController.GetAxisFloat(PlayerInputController.INPUT_AXIS.VERTICAL);
        currentInput.m_jump = m_inputController.GetKeyInput(PlayerInputController.INPUT_KEY.JUMP, PlayerInputController.INPUT_STATE.DOWNED);
        currentInput.m_dodge = false;
        currentInput.m_lightAttack = m_inputController.GetKeyInput(PlayerInputController.INPUT_KEY.ATTACK, PlayerInputController.INPUT_STATE.DOWNED); 
        currentInput.m_heavyAttack = m_inputController.GetKeyInput(PlayerInputController.INPUT_KEY.ATTACK_ALT, PlayerInputController.INPUT_STATE.DOWNED);
        currentInput.m_lightCombo = m_inputController.GetKeyInput(PlayerInputController.INPUT_KEY.ATTACK, PlayerInputController.INPUT_STATE.CURRENT);
        currentInput.m_heavyCombo = m_inputController.GetKeyInput(PlayerInputController.INPUT_KEY.ATTACK_ALT, PlayerInputController.INPUT_STATE.CURRENT);

        return currentInput;
    }
}
