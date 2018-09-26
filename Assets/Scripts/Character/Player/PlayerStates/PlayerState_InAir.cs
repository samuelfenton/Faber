using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_InAir : PlayerState
{
    private bool m_doubleJump = true;
    public float m_inAirHorizontalSpeed = 0.5f;
    public float m_doubleJumpSpeed = 6.0f;

    //-------------------
    //Initilse the state, runs only once at start
    //-------------------
    public override void StateInit()
    {
        base.StateInit();
    }

    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public override void StateStart()
    {
        m_doubleJump = true;
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
        newVelocity.x = m_inAirHorizontalSpeed * Input.GetAxisRaw("Horizontal");

        //Double Jump
        if (m_doubleJump && m_inputController.GetInput(InputController.INPUT.JUMP, InputController.INPUT_STATE.DOWNED))
        {
            m_doubleJump = false;
            newVelocity.y = m_doubleJumpSpeed;
        }

        m_parentCharacter.m_localVelocity = newVelocity;

        return true;
    }

    //-------------------
    //When swapping to a new state, this is called.
    //-------------------
    public override void StateEnd()
    {

    }

    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public override bool IsValid()
    {
        return !m_parentCharacter.IsGrounded();
    }
}
