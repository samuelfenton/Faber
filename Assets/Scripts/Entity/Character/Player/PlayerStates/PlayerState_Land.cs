using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Land : State_Player
{
    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Entity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.IN_AIR, false);
        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.LAND, true);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        //Allow player to land and move
        float horizontal = m_player.m_input.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL);
        m_character.SetDesiredVelocity(horizontal * m_character.m_groundRunVel * m_character.m_inAirModifier);

        return m_customAnimation.IsAnimationDone(CustomAnimation.LAYER.BASE);
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();

        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.LAND, false);
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_entity.m_splinePhysics.m_downCollision;
    }
}
