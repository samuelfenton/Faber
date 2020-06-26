using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_InAir : State_Player
{
    private float m_horizontalSpeedMax = 1.0f;
    private float m_horizontalAcceleration = 0.5f;

    private float m_doubleJumpSpeed = 6.0f;

    private enum IN_AIR_STATE {INITIAL, SECOND_JUMP, FINAL }
    private IN_AIR_STATE m_inAirState = IN_AIR_STATE.INITIAL;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Enity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);

        m_horizontalSpeedMax = m_entity.m_groundRunVel; //Always use grounded as max speed
        m_horizontalAcceleration = m_entity.m_inAirAccel;
        m_doubleJumpSpeed = m_entity.m_doubleJumpSpeed;
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        m_inAirState = IN_AIR_STATE.INITIAL;

        m_customAnimation.SetBool(CustomAnimation.VARIBLE_BOOL.IN_AIR, true);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        float horizontal = m_player.m_input.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL);
        m_entity.SetDesiredVelocity(horizontal * m_entity.m_groundRunVel);

        switch (m_inAirState)
        {
            case IN_AIR_STATE.INITIAL:
                //Double Jump
                if (m_player.m_input.GetKey(CustomInput.INPUT_KEY.JUMP) == CustomInput.INPUT_STATE.DOWNED)
                {
                    m_customAnimation.SetBool(CustomAnimation.VARIBLE_BOOL.DOUBLE_JUMP, true);

                    Vector3 newVelocity = m_entity.m_localVelocity;
                    newVelocity.y = m_doubleJumpSpeed;
                    m_entity.m_localVelocity = newVelocity;
                    
                    m_inAirState = IN_AIR_STATE.SECOND_JUMP;
                }
                break;
            case IN_AIR_STATE.SECOND_JUMP:
                if(m_customAnimation.IsAnimationDone())
                {
                    m_customAnimation.SetBool(CustomAnimation.VARIBLE_BOOL.DOUBLE_JUMP, false);
                    
                    m_inAirState = IN_AIR_STATE.FINAL;
                    m_customAnimation.SetBool(CustomAnimation.VARIBLE_BOOL.IN_AIR, true);
                }
                break;
            case IN_AIR_STATE.FINAL:
                break;
            default:
                break;
        }

        return m_entity.m_splinePhysics.m_downCollision;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();

        m_customAnimation.SetBool(CustomAnimation.VARIBLE_BOOL.IN_AIR, false);
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return !m_entity.m_splinePhysics.m_downCollision;
    }
}
