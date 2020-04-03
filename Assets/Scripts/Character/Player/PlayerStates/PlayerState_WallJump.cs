using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerState_WallJump : Player_State
{
    private float m_wallJumpVerticalSpeed = 5.0f;
    private float m_wallJumpHorizontalSpeed = 2.0f;

    private float m_inputDelay = 0.1f;
    private float m_inputDelayTimer = 0.0f;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);

        m_wallJumpVerticalSpeed = m_character.m_wallJumpVerticalSpeed;
        m_wallJumpHorizontalSpeed = m_character.m_wallJumpHorizontalSpeed;

        m_inputDelay = m_character.m_wallJumpInputDelay;
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        //Movement
        Vector3 newVelocity = m_character.m_localVelocity;

        if (m_character.m_splinePhysics.m_forwardCollision)
        {
            newVelocity.y = m_wallJumpVerticalSpeed;
            newVelocity.x = -m_wallJumpHorizontalSpeed;
        }
        else if (m_character.m_splinePhysics.m_backCollision)
        {
            newVelocity.y = m_wallJumpVerticalSpeed;
            newVelocity.x = m_wallJumpHorizontalSpeed;
        }

        m_character.m_localVelocity = newVelocity;

        //Setup timer
        m_inputDelayTimer = m_inputDelay;
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        m_inputDelayTimer -= Time.deltaTime;
        return m_inputDelayTimer < 0.0f;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {

    }
    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return
        m_playerCharacter.m_input.GetKeyBool(CustomInput.INPUT_KEY.JUMP) &&
        (m_character.m_splinePhysics.m_forwardCollision || m_character.m_splinePhysics.m_backCollision) &&
        !m_character.m_splinePhysics.m_downCollision;
    }
}
