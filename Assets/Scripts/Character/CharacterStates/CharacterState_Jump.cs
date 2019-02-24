using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState_Jump : CharacterState
{
    private float m_jumpSpeed = 10.0f;

    //-------------------
    //Initilse the state, runs only once at start
    //-------------------
    public override void StateInit()
    {
        base.StateInit();
        m_jumpSpeed = m_parentCharacter.m_jumpSpeed;

        m_stateType = CharacterStateMachine.STATE.JUMP;
    }

    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public override void StateStart()
    {
        Vector3 newVelocity = m_parentCharacter.m_localVelocity;
        newVelocity.y = m_jumpSpeed;
        m_parentCharacter.m_localVelocity = newVelocity;

        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.JUMP, true);
    }

    //-------------------
    //State update, perform any actions for the given state
    //
    //Return bool: Has this state been completed, e.g. Attack has completed, idle would always return true 
    //-------------------
    public override bool UpdateState()
    {
        return !m_characterAnimationController.EndOfAnimation();
    }

    //-------------------
    //When swapping to a new state, this is called.
    //-------------------
    public override void StateEnd()
    {
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.JUMP, false);
    }

    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public override bool IsValid()
    {
        //Able to jump while jump key is pressed, grounded, and no collision above
        return m_parentCharacter.m_currentCharacterInput.m_jump && m_parentCharacter.m_characterCustomPhysics.m_downCollision && !m_parentCharacter.m_characterCustomPhysics.m_upCollision;
    }
}
