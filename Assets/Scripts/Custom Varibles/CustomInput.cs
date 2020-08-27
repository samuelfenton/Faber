using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomInput : MonoBehaviour
{
    public float m_mouseXSensitivity = 30.0f;
    public float m_mouseYSensitivity = 30.0f;

    public bool m_inverted = false;

    public enum INPUT_KEY {JUMP, SPRINT, DASH, BLOCK, LIGHT_ATTACK, HEAVY_ATTACK, ALT_ATTACK, INTERACT, CAMERA_FLIP, MENU, KEY_COUNT };
    
    public enum INPUT_STATE { UP, DOWN, DOWNED };

    public enum INPUT_AXIS { HORIZONTAL, VERTICAL, MOUSE_X, MOUSE_Y, SCROLL, AXIS_COUNT };

    //[HideInInspector]
    private INPUT_STATE[] m_keyVal = new INPUT_STATE[(int)INPUT_KEY.KEY_COUNT];
    //[HideInInspector]
    private float[] m_axisVal = new float[(int)INPUT_AXIS.AXIS_COUNT];

    private InputAction_Gameplay m_intput = null;

    private void Awake()
    {
        for (int i = 0; i < (int)INPUT_KEY.KEY_COUNT; i++)
        {
            m_keyVal[i] = INPUT_STATE.UP;
        }

        for (int i = 0; i < (int)INPUT_AXIS.AXIS_COUNT; i++)
        {
            m_axisVal[i] = 0.0f;
        }

        m_intput = new InputAction_Gameplay();
    }

    private void OnEnable()
    {
        m_intput.Player.Move.Enable();

        m_intput.Player.Jump.Enable();
        m_intput.Player.Sprint.Enable();
        m_intput.Player.Dash.Enable();
        m_intput.Player.Block.Enable();
        m_intput.Player.LightAttack.Enable();
        m_intput.Player.HeavyAttack.Enable();
        m_intput.Player.AltAttack.Enable();
        m_intput.Player.Interact.Enable();
        m_intput.Player.CameraFlip.Enable();
        m_intput.Player.Menu.Enable();

    }

    private void OnDisable()
    {
        m_intput.Player.Move.Disable();

        m_intput.Player.Jump.Disable();
        m_intput.Player.Sprint.Disable();
        m_intput.Player.Dash.Disable();
        m_intput.Player.Block.Disable();
        m_intput.Player.LightAttack.Disable();
        m_intput.Player.HeavyAttack.Disable();
        m_intput.Player.AltAttack.Disable();
        m_intput.Player.Interact.Disable();
        m_intput.Player.CameraFlip.Disable();
        m_intput.Player.Menu.Disable();
    }


    /// <summary>
    /// Update the inputs of a character
    /// </summary>
    public void UpdateInput()
    {
        m_axisVal[(int)INPUT_AXIS.HORIZONTAL] = m_intput.Player.Move.ReadValue<Vector2>().x;
        m_axisVal[(int)INPUT_AXIS.VERTICAL] = m_intput.Player.Move.ReadValue<Vector2>().y;

        m_keyVal[(int)INPUT_KEY.JUMP] = DetermineState(m_keyVal[(int)INPUT_KEY.JUMP], m_intput.Player.Jump.ReadValue<float>() > 0.0f);
        m_keyVal[(int)INPUT_KEY.SPRINT] = DetermineState(m_keyVal[(int)INPUT_KEY.SPRINT], m_intput.Player.Sprint.ReadValue<float>() > 0.0f);
        m_keyVal[(int)INPUT_KEY.DASH] = DetermineState(m_keyVal[(int)INPUT_KEY.DASH], m_intput.Player.Dash.ReadValue<float>() > 0.0f);
        m_keyVal[(int)INPUT_KEY.BLOCK] = DetermineState(m_keyVal[(int)INPUT_KEY.BLOCK], m_intput.Player.Block.ReadValue<float>() > 0.0f);
        m_keyVal[(int)INPUT_KEY.LIGHT_ATTACK] = DetermineState(m_keyVal[(int)INPUT_KEY.LIGHT_ATTACK], m_intput.Player.LightAttack.ReadValue<float>() > 0.0f);
        m_keyVal[(int)INPUT_KEY.HEAVY_ATTACK] = DetermineState(m_keyVal[(int)INPUT_KEY.HEAVY_ATTACK], m_intput.Player.HeavyAttack.ReadValue<float>() > 0.0f);
        m_keyVal[(int)INPUT_KEY.ALT_ATTACK] = DetermineState(m_keyVal[(int)INPUT_KEY.ALT_ATTACK], m_intput.Player.AltAttack.ReadValue<float>() > 0.0f);
        m_keyVal[(int)INPUT_KEY.INTERACT] = DetermineState(m_keyVal[(int)INPUT_KEY.INTERACT], m_intput.Player.Interact.ReadValue<float>() > 0.0f);
        m_keyVal[(int)INPUT_KEY.CAMERA_FLIP] = DetermineState(m_keyVal[(int)INPUT_KEY.CAMERA_FLIP], m_intput.Player.CameraFlip.ReadValue<float>() > 0.0f);
        m_keyVal[(int)INPUT_KEY.MENU] = DetermineState(m_keyVal[(int)INPUT_KEY.MENU], m_intput.Player.Menu.ReadValue<float>() > 0.0f);
    }

    /// <summary>
    /// Determine the current input state for a button
    /// When not pressed at all, assume p_currentValue is false, and so return INPUT_STATE.UP
    /// If currently pressed(p_currentValue = true), if previously downed, its now down, otherwise its jsut downed
    /// </summary>
    /// <param name="p_currentState">Previous state value</param>
    /// <param name="p_currentValue">Current input value</param>
    /// <returns>The new INPUT_STATE based off previous rules</returns>
    private INPUT_STATE DetermineState(INPUT_STATE p_currentState, bool p_currentValue)
    {
        return p_currentValue ? p_currentState == INPUT_STATE.DOWNED ? INPUT_STATE.DOWN : INPUT_STATE.DOWNED : INPUT_STATE.UP;
    }

    /// <summary>
    /// Simplify key state to a boolean
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
        if(p_input == INPUT_AXIS.MOUSE_Y)
        {
            if (m_inverted)
                return m_axisVal[(int)p_input] * -1;
            return m_axisVal[(int)p_input];
        }
        if (p_input < INPUT_AXIS.AXIS_COUNT)
            return m_axisVal[(int)p_input];
        return 0.0f;
    }
}
