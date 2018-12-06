using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_InAir : PlayerState
{
    private bool m_doubleJump = true;

    private float m_horizontalSpeedMax = 1.0f;
    private float m_horizontalAcceleration = 0.5f;

    private float m_doubleJumpSpeed = 6.0f;

    //-------------------
    //Initilse the state, runs only once at start
    //-------------------
    public override void StateInit()
    {
        base.StateInit();

        m_horizontalSpeedMax = m_parentCharacter.m_groundedHorizontalSpeedMax; //Always use grounded as max speed
        m_horizontalAcceleration = m_parentCharacter.m_inAirHorizontalAcceleration;
        m_doubleJumpSpeed = m_parentCharacter.m_doubleJumpSpeed;
    }

    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public override void StateStart()
    {
        m_doubleJump = true;
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.IN_AIR, true);
    }

    //-------------------
    //State update, perform any actions for the given state
    //
    //Return bool: Has this state been completed, e.g. Attack has completed, idle would always return true 
    //-------------------
    public override bool UpdateState()
    {
        //Movement
        Vector3 newVelocity = m_parentCharacter.m_localVelocity;

        newVelocity.x += m_horizontalAcceleration * m_inputController.GetAxisFloat(InputController.INPUT_AXIS.HORIZONTAL) * Time.deltaTime;
        newVelocity.x = Mathf.Clamp(newVelocity.x, -m_horizontalSpeedMax, m_horizontalSpeedMax);

        //Double Jump
        if (m_doubleJump && m_inputController.GetKeyInput(InputController.INPUT_KEY.JUMP, InputController.INPUT_STATE.DOWNED))
        {
            m_doubleJump = false;
            newVelocity.y = m_doubleJumpSpeed;
        }

        m_parentCharacter.m_localVelocity = newVelocity;

        m_parentCharacter.m_characterAnimationController.SetVarible(CharacterAnimationController.VARIBLES.MOVEMENT_SPEED, Mathf.Abs(newVelocity.x / m_horizontalSpeedMax));

        return true;
    }

    //-------------------
    //When swapping to a new state, this is called.
    //-------------------
    public override void StateEnd()
    {
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.IN_AIR, false);
    }

    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public override bool IsValid()
    {
        return !m_parentCharacter.m_characterCustomPhysics.m_downCollision;
    }
}
