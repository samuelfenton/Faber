﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_InAir : Player_State
{
    private float m_horizontalSpeedMax = 1.0f;
    private float m_horizontalAcceleration = 0.5f;

    private float m_doubleJumpSpeed = 6.0f;

    private enum IN_AIR_STATE {INITIAL, SECOND_JUMP, FINAL }
    private IN_AIR_STATE m_inAirState = IN_AIR_STATE.INITIAL;
    private string m_animInAir = "";
    private string m_animDoubleJump = "";

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);

        m_horizontalSpeedMax = m_character.m_groundRunVel; //Always use grounded as max speed
        m_horizontalAcceleration = m_character.m_inAirAccel;
        m_doubleJumpSpeed = m_character.m_doubleJumpSpeed;

        m_animInAir = CustomAnimation.Instance.GetLocomotion(CustomAnimation.LOCOMOTION_ANIM.IN_AIR);
        m_animDoubleJump = CustomAnimation.Instance.GetLocomotion(CustomAnimation.LOCOMOTION_ANIM.DOUBLE_JUMP);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        m_inAirState = IN_AIR_STATE.INITIAL;

        CustomAnimation.Instance.PlayAnimation(m_animator, m_animInAir);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        float horizontal = m_playerCharacter.m_input.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL);
        m_character.SetDesiredVelocity(horizontal * m_character.m_groundRunVel);

        switch (m_inAirState)
        {
            case IN_AIR_STATE.INITIAL:
                //Double Jump
                if (m_playerCharacter.m_input.GetKey(CustomInput.INPUT_KEY.JUMP) == CustomInput.INPUT_STATE.DOWNED)
                {
                    CustomAnimation.Instance.PlayAnimation(m_animator, m_animDoubleJump);
                    
                    Vector3 newVelocity = m_character.m_localVelocity;
                    newVelocity.y = m_doubleJumpSpeed;
                    m_character.m_localVelocity = newVelocity;
                    
                    m_inAirState = IN_AIR_STATE.SECOND_JUMP;
                }
                break;
            case IN_AIR_STATE.SECOND_JUMP:
                if(CustomAnimation.Instance.IsAnimationDone(m_animator))
                {
                    m_inAirState = IN_AIR_STATE.FINAL;
                    CustomAnimation.Instance.PlayAnimation(m_animator, m_animInAir);
                }
                break;
            case IN_AIR_STATE.FINAL:
                break;
            default:
                break;
        }

        return m_character.m_splinePhysics.m_downCollision;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return !m_character.m_splinePhysics.m_downCollision;
    }
}
