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
        public bool m_lightAttack;
        public bool m_heavyAttack;
        public bool m_dodge;

        public InputState(float p_horizontal = 0.0f, float p_vertical = 0.0f, bool p_jump = false, bool p_lightAttack = false, bool p_heavyAttack = false, bool p_dodge = false)
        {
            m_horizontal = p_horizontal;
            m_vertical = p_vertical;
            m_jump = p_jump;
            m_lightAttack = p_lightAttack;
            m_heavyAttack = p_heavyAttack;
            m_dodge = p_dodge;
        }
    }

    public virtual InputState GetInputState()
    {
        return new InputState();
    }
}
