using System.Collections;
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

        m_horizontalSpeedMax = m_character.m_groundHorizontalMax; //Always use grounded as max speed
        m_horizontalAcceleration = m_character.m_inAirHorizontalAcceleration;
        m_doubleJumpSpeed = m_character.m_doubleJumpSpeed;

        m_animInAir = AnimController.GetLocomotion(AnimController.LOCOMOTION_ANIM.IN_AIR);
        m_animDoubleJump = AnimController.GetLocomotion(AnimController.LOCOMOTION_ANIM.DOUBLE_JUMP);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        m_inAirState = IN_AIR_STATE.INITIAL;

        m_animator.Play(m_animInAir);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        //Movement
        float horizontal = m_playerCharacter.m_input.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL);

        if (horizontal != 0.0f)//Input so normal movemnt
        {
            Vector3 newVelocity = m_character.m_localVelocity;
            
            newVelocity.x += m_horizontalAcceleration * horizontal * Time.deltaTime;
            newVelocity.x = Mathf.Clamp(newVelocity.x, -m_horizontalSpeedMax, m_horizontalSpeedMax);

            m_character.m_localVelocity = newVelocity;
        }
        else
        {
            m_character.ApplyFriction();
        }

        switch (m_inAirState)
        {
            case IN_AIR_STATE.INITIAL:
                //Double Jump
                if (m_playerCharacter.m_input.GetKey(CustomInput.INPUT_KEY.JUMP) == CustomInput.INPUT_STATE.DOWNED)
                {
                    m_animator.Play(m_animDoubleJump);
                    
                    Vector3 newVelocity = m_character.m_localVelocity;
                    newVelocity.y = m_doubleJumpSpeed;
                    m_character.m_localVelocity = newVelocity;
                    
                    m_inAirState = IN_AIR_STATE.SECOND_JUMP;
                }
                break;
            case IN_AIR_STATE.SECOND_JUMP:
                if(AnimController.IsAnimationDone(m_animator))
                {
                    m_inAirState = IN_AIR_STATE.FINAL;
                    m_animator.Play(m_animInAir);
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
