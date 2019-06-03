using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput : MonoBehaviour
{
    public struct InputState
    {
        public float m_horizontal;
        public float m_vertical;
        public bool m_jump;
        public bool m_dodge;
        public bool m_lightAttack;
        public bool m_heavyAttack;
        public bool m_lightCombo;
        public bool m_heavyCombo;

        public InputState(float p_horizontal = 0.0f, float p_vertical = 0.0f, bool p_jump = false, bool p_dodge = false, bool p_lightAttack = false, bool p_heavyAttack = false, bool p_lightCombo = false, bool p_heavyCombo = false)
        {
            m_horizontal = p_horizontal;
            m_vertical = p_vertical;
            m_jump = p_jump;
            m_dodge = p_dodge;
            m_lightAttack = p_lightAttack;
            m_heavyAttack = p_heavyAttack;
            m_lightCombo = p_lightCombo;
            m_heavyCombo = p_heavyCombo;
        }
    }

    //-------------------
    //Get the characters current input state, NPC vs Player
    //
    //Return InputState: structer containing all required data
    //-------------------
    public virtual InputState GetInputState()
    {
        return new InputState();
    }
}
