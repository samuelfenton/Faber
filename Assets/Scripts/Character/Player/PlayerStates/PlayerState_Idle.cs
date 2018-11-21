﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Idle : PlayerState
{
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
        m_parentCharacter.m_characterAnimationController.SetAnimation(CharacterAnimationController.ANIMATIONS.LOCOMOTION);
        m_parentCharacter.m_characterAnimationController.SetVarible(CharacterAnimationController.VARIBLES.MOVEMENT_SPEED, 0.0f);
    }

    //-------------------
    //State update, perform any actions for the given state
    //
    //Return bool: Has this state been completed, e.g. Attack has completed, idle would always return true 
    //-------------------
    public override bool UpdateState()
    {
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
        return m_parentCharacter.m_localVelocity.x == 0.0f && m_parentCharacter.m_characterCustomPhysics.m_downCollision;
    }
}
