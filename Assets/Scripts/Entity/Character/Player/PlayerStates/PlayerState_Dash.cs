using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Dash : State_Player
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

        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.DASH, true);

        float desiredVelocity = 0.0f;
        float modelToObjectForwardDot = Vector3.Dot(m_character.m_characterModel.transform.forward, m_entity.transform.forward);

        //Update Translation
        if (modelToObjectForwardDot >= 0.0f)//Facing correct way, roll backwards
            desiredVelocity = -m_character.m_rollbackVelocity;
        else
            desiredVelocity = m_character.m_rollbackVelocity;

        m_character.SetDesiredVelocity(desiredVelocity);
        m_character.HardSetVelocity(desiredVelocity);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        if(m_customAnimation.GetAnimationPercent(CustomAnimation.LAYER.BASE) >0.7f)
            m_character.SetDesiredVelocity(0.0f);

        return m_customAnimation.IsAnimationDone(CustomAnimation.LAYER.BASE);
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();

        m_character.SetDesiredVelocity(0.0f);
        m_character.HardSetVelocity(0.0f);

        m_customAnimation.SetVaribleBool(CustomAnimation.VARIBLE_BOOL.DASH, false);
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_entity.m_splinePhysics.m_downCollision && m_player.m_input.GetKeyBool(CustomInput.INPUT_KEY.ROLL);
    }
}
