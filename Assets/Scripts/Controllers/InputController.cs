using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public enum INPUT_KEY {ATTACK, DEFEND, ATTACK_ALT, JUMP, SUBMIT, CANCEL, INPUT_COUNT};
    public enum INPUT_AXIS {HORIZONTAL, VERTICAL, INPUT_COUNT };
    public enum INPUT_STATE {DOWNED, UPPED, CURRENT};

    public KeyInformation[] m_keyInformation = new KeyInformation[(int)INPUT_KEY.INPUT_COUNT];
    public Dictionary<INPUT_AXIS, string> m_axisInformation = new Dictionary<INPUT_AXIS, string>();

    public class KeyInformation
    {
        public string m_keyString = "";

        //m_state[0] = downed state
        //m_state[1] = upped state
        //m_state[2] = current state
        public bool[] m_state = new bool[3];

        public KeyInformation(string p_keyString)
        {
            m_keyString = p_keyString;
        }
    }

    private void Start()
    {
        m_keyInformation[(int)INPUT_KEY.ATTACK] = new KeyInformation("Attack");
        m_keyInformation[(int)INPUT_KEY.DEFEND] = new KeyInformation("Defend");
        m_keyInformation[(int)INPUT_KEY.ATTACK_ALT] = new KeyInformation("AttackAlt");
        m_keyInformation[(int)INPUT_KEY.JUMP] = new KeyInformation("Jump");
        m_keyInformation[(int)INPUT_KEY.SUBMIT] = new KeyInformation("Submit");
        m_keyInformation[(int)INPUT_KEY.CANCEL] = new KeyInformation("Cancel");

        m_axisInformation.Add(INPUT_AXIS.HORIZONTAL, "Horizontal");
        m_axisInformation.Add(INPUT_AXIS.VERTICAL, "Vertical");
    }

    private void Update()
    {
        for (int i = 0; i < (int)INPUT_KEY.INPUT_COUNT; i++)
        {
            UpdateKey(i);
        }
    }

    private void UpdateKey(int index)
    {
        bool currentState = Input.GetAxisRaw(m_keyInformation[index].m_keyString) != 0.0f;

        m_keyInformation[index].m_state[0] = !m_keyInformation[index].m_state[2] && currentState;//Key just pressed

        m_keyInformation[index].m_state[1] = m_keyInformation[index].m_state[2] && !currentState;//Key just upped

        m_keyInformation[index].m_state[2] = currentState; //Store current state for next frame
    }

    public bool GetKeyInput(INPUT_KEY p_input, INPUT_STATE p_inputState = INPUT_STATE.CURRENT)
    {
        if (p_input < INPUT_KEY.INPUT_COUNT)
            return m_keyInformation[(int)p_input].m_state[(int)p_inputState];
        return false;
    }

    public bool GetAxisBool(INPUT_AXIS p_input)
    {
        if(p_input < INPUT_AXIS.INPUT_COUNT)
            return Input.GetAxisRaw(m_axisInformation[p_input]) != 0.0f;
        return false;
    }

    public float GetAxisFloat(INPUT_AXIS p_input)
    {
        if (p_input < INPUT_AXIS.INPUT_COUNT)
            return Input.GetAxisRaw(m_axisInformation[p_input]);
        return 0.0f;
    }
}
