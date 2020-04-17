using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Roll : Player_State
{
    private string m_animRoll = "";

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);

        m_animRoll = CustomAnimation.GetLocomotion(CustomAnimation.LOCOMOTION_ANIM.ROLL);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        CustomAnimation.PlayAnimtion(m_animator, m_animRoll);

        float desiredVelocity = 0.0f;
        float modelToObjectForwardDot = Vector3.Dot(m_character.m_characterModel.transform.forward, m_character.transform.forward);

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

        if(CustomAnimation.GetAnimationPercent(m_animator) >0.7f)
            m_character.SetDesiredVelocity(0.0f);

        return CustomAnimation.IsAnimationDone(m_animator);
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_character.SetDesiredVelocity(0.0f);
        m_character.HardSetVelocity(0.0f);

        base.StateEnd();
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_character.m_splinePhysics.m_downCollision && m_playerCharacter.m_input.GetKeyBool(CustomInput.INPUT_KEY.ROLL);
    }
}
