using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_GroundMovement : PlayerState
{
    public float m_minHorizontalSpeed = 0.1f;
    private float m_horizontalSpeed = 1.0f;

    //-------------------
    //Initilse the state, runs only once at start
    //-------------------
    public override void StateInit()
    {
        base.StateInit();
        m_horizontalSpeed = m_parentCharacter.m_groundedHorizontalSpeed;
    }

    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public override void StateStart()
    {
        m_parentCharacter.m_characterAnimationController.PlayAnimation(CharacterAnimationController.ANIMATION.RUN);
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
        newVelocity.x = m_horizontalSpeed * Input.GetAxisRaw("Horizontal");

        m_parentCharacter.m_localVelocity = newVelocity;
        return true;
    }

    //-------------------
    //When swapping to a new state, this is called.
    //-------------------
    public override void StateEnd()
    {
        m_parentCharacter.m_localVelocity.x = 0.0f;
    }

    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public override bool IsValid()
    {
        return m_parentCharacter.IsGrounded() && Input.GetAxisRaw("Horizontal") != 0.0f;
    }
}
