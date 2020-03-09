using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_InAir : Player_State
{
    private bool m_doubleJump = true;

    private float m_horizontalSpeedMax = 1.0f;
    private float m_horizontalAcceleration = 0.5f;

    private float m_doubleJumpSpeed = 6.0f;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    public override void StateInit()
    {
        base.StateInit();

        m_horizontalSpeedMax = m_parentCharacter.m_groundedHorizontalSpeedMax; //Always use grounded as max speed
        m_horizontalAcceleration = m_parentCharacter.m_inAirHorizontalAcceleration;
        m_doubleJumpSpeed = m_parentCharacter.m_doubleJumpSpeed;
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        m_doubleJump = true;
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        //Movement
        Vector3 newVelocity = m_parentCharacter.m_localVelocity;

        newVelocity.x += m_horizontalAcceleration * m_parentPlayer.m_input.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL) * Time.deltaTime;
        newVelocity.x = Mathf.Clamp(newVelocity.x, -m_horizontalSpeedMax, m_horizontalSpeedMax);

        //Double Jump
        if (m_doubleJump && m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.JUMP) == CustomInput.INPUT_STATE.DOWNED)
        {
            m_doubleJump = false;
            newVelocity.y = m_doubleJumpSpeed;
        }

        m_parentCharacter.m_localVelocity = newVelocity;

        m_parentCharacter.m_characterAnimationController.SetVarible(CharacterAnimationController.VARIBLES.CURRENT_VELOCITY, Mathf.Abs(newVelocity.x / m_horizontalSpeedMax));

        return true;
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
        return !m_parentCharacter.m_splinePhysics.m_downCollision;
    }
}
