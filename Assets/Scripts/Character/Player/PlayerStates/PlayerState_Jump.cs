using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Jump : Player_State
{
    private float m_jumpSpeed = 10.0f;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    public override void StateInit()
    {
        base.StateInit();
        m_jumpSpeed = m_parentCharacter.m_jumpSpeed;
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        Vector3 newVelocity = m_parentCharacter.m_localVelocity;
        newVelocity.y = m_jumpSpeed;
        m_parentCharacter.m_localVelocity = newVelocity;

        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.JUMP, true);
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.IN_AIR, true);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        return m_characterAnimationController.EndOfAnimation();
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.JUMP, false);
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        //Able to jump while jump key is pressed, grounded, and no collision above
        return m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.JUMP) == CustomInput.INPUT_STATE.DOWNED
            && m_parentCharacter.m_splinePhysics.m_downCollision && 
            !m_parentCharacter.m_splinePhysics.m_upCollision;
    }
}
