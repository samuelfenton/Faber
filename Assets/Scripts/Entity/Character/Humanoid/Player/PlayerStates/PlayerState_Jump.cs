using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Jump : State_Player
{
    private float m_jumpSpeed = 10.0f;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Enity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);
        m_jumpSpeed = m_entity.m_jumpSpeed;
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        Vector3 newVelocity = m_entity.m_localVelocity;
        newVelocity.y = m_jumpSpeed;
        m_entity.m_localVelocity = newVelocity;

        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.JUMP, true);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        return m_customAnimation.IsAnimationDone() || m_entity.m_localVelocity.y <= 0.0f;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();

        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.JUMP, false);
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        //Able to jump while jump key is pressed, grounded, and no collision above
        return m_player.m_input.GetKey(CustomInput.INPUT_KEY.JUMP) == CustomInput.INPUT_STATE.DOWNED
            && m_entity.m_splinePhysics.m_downCollision && 
            !m_entity.m_splinePhysics.m_upCollision;
    }
}
