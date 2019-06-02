using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState_BaseAttack : CharacterState
{
    protected float m_horizontalSpeedMax = 1.0f;
    protected float m_horizontalDeacceleration = 1.0f;

    //-------------------
    //Initilse the state, runs only once at start
    //-------------------
    public override void StateInit()
    {
        base.StateInit();
        m_horizontalSpeedMax = m_parentCharacter.m_groundedHorizontalSpeedMax;
        m_horizontalDeacceleration = m_parentCharacter.m_groundedHorizontalDeacceleration;

        m_stateType = CharacterStateMachine.STATE.ATTACK;
    }

    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public override void StateStart()
    {
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK, true);
        m_parentCharacter.m_currentAttackType = Character.ATTACK_TYPE.LIGHT;
    }

    //-------------------
    //State update, perform any actions for the given state
    //
    //Return bool: Has this state been completed, e.g. Attack has completed, idle would always return true 
    //-------------------
    public override bool UpdateState()
    {
        //Movement slowdown
        Vector3 newVelocity = m_parentCharacter.m_localVelocity;

        float deltaSpeed = m_horizontalDeacceleration * Time.deltaTime;
        if (deltaSpeed > Mathf.Abs(newVelocity.x))//Close enough to stopping this frame
            newVelocity.x = 0.0f;
        else
            newVelocity.x += newVelocity.x < 0 ? deltaSpeed : -deltaSpeed;//Still have high velocity, just slow down

        m_parentCharacter.m_localVelocity = newVelocity;

        m_characterAnimationController.SetVarible(CharacterAnimationController.VARIBLES.MOVEMENT_SPEED, Mathf.Abs(newVelocity.x / m_horizontalSpeedMax));

        return true;
    }

    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public override bool IsValid()
    {
        return false;
    }
}
