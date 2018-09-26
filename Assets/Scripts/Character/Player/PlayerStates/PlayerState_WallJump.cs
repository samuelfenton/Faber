using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_WallJump : PlayerState
{
    public float m_inputDelayTime = 0.1f;
    private float m_inputDelayTimer = 0.0f;
    public float m_jumpingOffWallVerticalSpeed = 5.0f;
    public float m_jumpingOffWallHorizontalSpeed = 2.0f;

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
        //Movement
        Vector3 newVelocity = m_parentCharacter.m_localVelocity;

        if (m_parentCharacter.m_characterCustomPhysics.m_forwardCollision)
        {
            newVelocity.y = m_jumpingOffWallVerticalSpeed;
            newVelocity.x = -m_jumpingOffWallHorizontalSpeed;
        }
        else if (m_parentCharacter.m_characterCustomPhysics.m_backCollision)
        {
            newVelocity.y = m_jumpingOffWallVerticalSpeed;
            newVelocity.x = m_jumpingOffWallHorizontalSpeed;
        }

        m_parentCharacter.m_localVelocity = newVelocity;

        //Setup timer
        m_inputDelayTimer = m_inputDelayTime;
    }

    //-------------------
    //State update, perform any actions for the given state
    //
    //Return bool: Has this state been completed, e.g. Attack has completed, idle would always return true 
    //-------------------
    public override bool UpdateState()
    {
        m_inputDelayTimer -= Time.fixedDeltaTime;
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
        m_inputController.GetInput(InputController.INPUT.JUMP, InputController.INPUT_STATE.DOWNED) &&
        (m_parentCharacter.m_characterCustomPhysics.m_forwardCollision || m_parentCharacter.m_characterCustomPhysics.m_backCollision) &&
        !m_parentCharacter.IsGrounded();
    }
}
