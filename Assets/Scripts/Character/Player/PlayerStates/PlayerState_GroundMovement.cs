using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_GroundMovement : Player_State
{
    private float m_horizontalSpeedMax = 1.0f;
    private float m_horizontalAcceleration = 1.0f;
    private float m_horizontalDeacceleration = 1.0f;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    public override void StateInit()
    {
        base.StateInit();

        m_horizontalSpeedMax = m_parentCharacter.m_groundedHorizontalSpeedMax;
        m_horizontalAcceleration = m_parentCharacter.m_groundedHorizontalAcceleration;
        m_horizontalDeacceleration = m_parentCharacter.m_groundedHorizontalDeacceleration;
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LOCOMOTION, true);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        //Movement
        Vector3 newVelocity = m_parentCharacter.m_localVelocity;

        if(m_parentPlayer.m_input.GetAxisBool(Input.INPUT_AXIS.HORIZONTAL)) //No input, slowdown
        {
            m_parentCharacter.ApplyFriction();
        }
        else//Input so normal movemnt
        {
            newVelocity.x += m_horizontalAcceleration * m_parentPlayer.m_input.GetAxis(Input.INPUT_AXIS.HORIZONTAL) * Time.deltaTime;
            newVelocity.x = Mathf.Clamp(newVelocity.x, -m_horizontalSpeedMax, m_horizontalSpeedMax);
        }

        m_parentCharacter.m_localVelocity = newVelocity;

        m_parentCharacter.m_characterAnimationController.SetVarible(CharacterAnimationController.VARIBLES.MOVEMENT_SPEED, Mathf.Abs(newVelocity.x/ m_horizontalSpeedMax));

        return true;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LOCOMOTION, false);
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_parentCharacter.m_splinePhysics.m_downCollision && (m_parentCharacter.m_localVelocity.x != 0.0f || m_parentPlayer.m_input.GetAxisBool(Input.INPUT_AXIS.HORIZONTAL));
    }
}
