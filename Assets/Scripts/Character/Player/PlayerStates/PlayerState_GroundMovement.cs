using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_GroundMovement : PlayerState
{
    private float m_horizontalSpeedMax = 1.0f;
    private float m_horizontalAcceleration = 1.0f;
    private float m_horizontalDeacceleration = 1.0f;

    //-------------------
    //Initilse the state, runs only once at start
    //-------------------
    public override void StateInit()
    {
        base.StateInit();
        m_horizontalSpeedMax = m_parentCharacter.m_groundedHorizontalSpeedMax;
        m_horizontalAcceleration = m_parentCharacter.m_groundedHorizontalAcceleration;
        m_horizontalDeacceleration = m_parentCharacter.m_groundedHorizontalDeacceleration;

        m_stateType = CharacterStateMachine.STATE.GROUND;

    }

    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public override void StateStart()
    {
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LOCOMOTION, true);
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

        if(!m_inputController.GetAxisBool(InputController.INPUT_AXIS.HORIZONTAL)) //No input, slowdown
        {
            float deltaSpeed = m_horizontalDeacceleration * Time.deltaTime;
            if (deltaSpeed > Mathf.Abs(newVelocity.x))//Close enough to stopping this frame
                newVelocity.x = 0.0f;
            else
                newVelocity.x += newVelocity.x < 0 ? deltaSpeed : -deltaSpeed;//Still have high velocity, just slow down
        }
        else//Input so normal movemnt
        {
            newVelocity.x += m_horizontalAcceleration * m_inputController.GetAxisFloat(InputController.INPUT_AXIS.HORIZONTAL) * Time.deltaTime;
            newVelocity.x = Mathf.Clamp(newVelocity.x, -m_horizontalSpeedMax, m_horizontalSpeedMax);
        }

        m_parentCharacter.m_localVelocity = newVelocity;

        m_parentCharacter.m_characterAnimationController.SetVarible(CharacterAnimationController.VARIBLES.MOVEMENT_SPEED, Mathf.Abs(newVelocity.x/ m_horizontalSpeedMax));

        return true;
    }

    //-------------------
    //When swapping to a new state, this is called.
    //-------------------
    public override void StateEnd()
    {
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LOCOMOTION, false);
    }

    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public override bool IsValid()
    {
        return m_parentCharacter.m_characterCustomPhysics.m_downCollision && (m_parentCharacter.m_localVelocity.x != 0.0f || m_inputController.GetAxisBool(InputController.INPUT_AXIS.HORIZONTAL)) ;
    }
}
