using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Locomotion : Player_State
{
    private float m_horizontalSpeedMax = 1.0f;
    private float m_horizontalAcceleration = 1.0f;
    private float m_horizontalDeacceleration = 1.0f;

    private string m_animLoco = "";
    private string m_paramVelocity = "";
    private string m_paramRandomIdle = "";

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_parentCharacter">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_parentCharacter)
    { 
        base.StateInit(p_loopedState, p_parentCharacter);

        m_horizontalSpeedMax = m_parentCharacter.m_groundedHorizontalSpeedMax;
        m_horizontalAcceleration = m_parentCharacter.m_groundedHorizontalAcceleration;
        m_horizontalDeacceleration = m_parentCharacter.m_groundedHorizontalDeacceleration;

        m_animLoco = AnimController.GetLocomotion(AnimController.LOCOMOTION_ANIM.LOCOMOTION);
        m_paramVelocity = AnimController.GetVarible(AnimController.VARIBLE_ANIM.CURRENT_VELOCITY);
        m_paramRandomIdle = AnimController.GetVarible(AnimController.VARIBLE_ANIM.RANDOM_IDLE);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        m_animator.Play(m_animLoco);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        //Movement
        Vector3 newVelocity = m_parentCharacter.m_localVelocity;
        float horizontal = m_parentPlayer.m_input.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL);

        if (horizontal != 0.0f)//Input so normal movemnt
        {
            newVelocity.x += m_horizontalAcceleration * horizontal * Time.deltaTime;
            newVelocity.x = Mathf.Clamp(newVelocity.x, -m_horizontalSpeedMax, m_horizontalSpeedMax);
            m_parentCharacter.m_localVelocity = newVelocity;
        }
        else //No input, slowdown
        {
            m_parentCharacter.ApplyFriction();
        }

        m_animator.SetFloat(m_paramVelocity, Mathf.Abs(newVelocity.x)/m_horizontalSpeedMax);

        return false;
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
        return m_parentCharacter.m_splinePhysics.m_downCollision;
    }
}
