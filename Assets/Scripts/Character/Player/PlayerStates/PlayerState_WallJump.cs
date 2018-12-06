using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_WallJump : PlayerState
{
    private float m_wallJumpVerticalSpeed = 5.0f;
    private float m_wallJumpHorizontalSpeed = 2.0f;

    private float m_inputDelay = 0.1f;
    private float m_inputDelayTimer = 0.0f;

    //-------------------
    //Initilse the state, runs only once at start
    //-------------------
    public override void StateInit()
    {
        base.StateInit();

        m_wallJumpVerticalSpeed = m_parentCharacter.m_wallJumpVerticalSpeed;
        m_wallJumpHorizontalSpeed = m_parentCharacter.m_wallJumpHorizontalSpeed;

        m_inputDelay = m_parentCharacter.m_wallJumpInputDelay;

        m_stateType = CharacterStateMachine.STATE.WALL_JUMP;
    }

    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public override void StateStart()
    {
        //Movement
        Vector3 newVelocity = m_parentCharacter.m_localVelocity;

        if (m_parentCharacter.m_characterCustomPhysics.m_forwardCollision)
        {
            newVelocity.y = m_wallJumpVerticalSpeed;
            newVelocity.x = -m_wallJumpHorizontalSpeed;
        }
        else if (m_parentCharacter.m_characterCustomPhysics.m_backCollision)
        {
            newVelocity.y = m_wallJumpVerticalSpeed;
            newVelocity.x = m_wallJumpHorizontalSpeed;
        }

        m_parentCharacter.m_localVelocity = newVelocity;

        //Setup timer
        m_inputDelayTimer = m_inputDelay;
    }

    //-------------------
    //State update, perform any actions for the given state
    //
    //Return bool: Has this state been completed, e.g. Attack has completed, idle would always return true 
    //-------------------
    public override bool UpdateState()
    {
        m_inputDelayTimer -= Time.deltaTime;
        return m_inputDelayTimer < 0.0f;
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
        return 
        m_inputController.GetKeyInput(InputController.INPUT_KEY.JUMP, InputController.INPUT_STATE.DOWNED) &&
        (m_parentCharacter.m_characterCustomPhysics.m_forwardCollision || m_parentCharacter.m_characterCustomPhysics.m_backCollision) &&
        !m_parentCharacter.m_characterCustomPhysics.m_downCollision;
    }
}
