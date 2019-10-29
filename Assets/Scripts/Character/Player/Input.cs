﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Input : MonoBehaviour
{
    public enum INPUT_KEY { ATTACK, DEFEND, ATTACK_ALT, JUMP, SUBMIT, CANCEL, KEY_COUNT };
    public enum INPUT_STATE { UP, DOWN, UPPED, DOWNED };

    public enum INPUT_AXIS { HORIZONTAL, VERTICAL, AXIS_COUNT };

    private string[] m_keyStrings = new string[(int)INPUT_KEY.KEY_COUNT];
    private string[] m_axisStrings = new string[(int)INPUT_AXIS.AXIS_COUNT];

    [HideInInspector]
    public INPUT_STATE[] m_keyVal = new INPUT_STATE[(int)INPUT_KEY.KEY_COUNT];
    [HideInInspector]
    public float[] m_axisVal = new float[(int)INPUT_AXIS.AXIS_COUNT];

    private void Start()
    {
        m_keyStrings[(int)INPUT_KEY.ATTACK] = "Attack";
        m_keyStrings[(int)INPUT_KEY.ATTACK_ALT] = "AttackAlt";
        m_keyStrings[(int)INPUT_KEY.DEFEND] = "Defend";
        m_keyStrings[(int)INPUT_KEY.JUMP] = "Jump";
        m_keyStrings[(int)INPUT_KEY.SUBMIT] = "Submit";
        m_keyStrings[(int)INPUT_KEY.CANCEL] = "Cancel";

        m_axisStrings[(int)INPUT_AXIS.HORIZONTAL] = "Horizontal";
        m_axisStrings[(int)INPUT_AXIS.VERTICAL] = "Vertical";

        for (int i = 0; i < (int)INPUT_KEY.KEY_COUNT; i++)
        {
            m_keyVal[i] = INPUT_STATE.UP;
        }

        for (int i = 0; i < (int)INPUT_AXIS.AXIS_COUNT; i++)
        {
            m_axisVal[i] = 0.0f;
        }
    }

    /// <summary>
    /// Update the inputs of a character
    /// </summary>
    public void UpdateInput()
    {
        //Keys
        for (int keyIndex = 0; keyIndex < (int)INPUT_KEY.KEY_COUNT; keyIndex++)
        {
            INPUT_STATE currentState = UnityEngine.Input.GetAxisRaw(m_keyStrings[keyIndex]) != 0.0f ? INPUT_STATE.DOWN : INPUT_STATE.UP;
            INPUT_STATE previousState = m_keyVal[keyIndex];

            if(previousState == INPUT_STATE.UP || previousState == INPUT_STATE.UPPED)
            {
                if (currentState == INPUT_STATE.UP)
                    m_keyVal[keyIndex] = INPUT_STATE.UP;
                else
                    m_keyVal[keyIndex] = INPUT_STATE.DOWNED;
            }
            else if (previousState == INPUT_STATE.DOWN || previousState == INPUT_STATE.DOWNED)
            {
                if (currentState == INPUT_STATE.DOWN)
                    m_keyVal[keyIndex] = INPUT_STATE.DOWN;
                else
                    m_keyVal[keyIndex] = INPUT_STATE.UPPED;
            }
        }

        //Axis
        for (int axisIndex = 0; axisIndex < (int)INPUT_AXIS.AXIS_COUNT; axisIndex++)
        {
            m_axisVal[axisIndex] = UnityEngine.Input.GetAxisRaw(m_axisStrings[axisIndex]);
        }
    }

    /// <summary>
    /// Simplyfy key state to a boolean
    /// </summary>
    /// <param name="p_input">Key to test against</param>
    /// <returns>true when down or downed</returns>
    public bool GetKeyBool(INPUT_KEY p_input)
    {
        if (p_input < INPUT_KEY.KEY_COUNT)
            return m_keyVal[(int)p_input] == INPUT_STATE.DOWN || m_keyVal[(int)p_input] == INPUT_STATE.DOWNED;
        return false;
    }

    /// <summary>
    /// Get Key state
    /// </summary>
    /// <param name="p_input">Axis to test against</param>
    /// <returns>Current state of key</returns>
    public INPUT_STATE GetKey(INPUT_KEY p_input)
    {
        if (p_input < INPUT_KEY.KEY_COUNT)
            return m_keyVal[(int)p_input];
        return INPUT_STATE.UP;
    }

    /// <summary>
    /// Simplyfy axis value to a boolean
    /// </summary>
    /// <param name="p_input">Axis to test against</param>
    /// <returns>true when axis value isnt 0.0f</returns>
    public bool GetAxisBool(INPUT_AXIS p_input)
    {
        if (p_input < INPUT_AXIS.AXIS_COUNT)
            return m_axisVal[(int)p_input] != 0.0f;
        return false;
    }

    /// <summary>
    /// Get Axis value
    /// </summary>
    /// <param name="p_input">Axis to test against</param>
    /// <returns>Value of Axis</returns>
    public float GetAxis(INPUT_AXIS p_input)
    {
        if (p_input < INPUT_AXIS.AXIS_COUNT)
            return m_axisVal[(int)p_input];
        return 0.0f;
    }
}
