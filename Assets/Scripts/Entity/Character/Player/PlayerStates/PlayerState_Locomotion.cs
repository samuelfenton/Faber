using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Locomotion : Player_State
{
    private string m_animIdle = "";
    private string m_paramVelocity = "";
    private string m_paramRandomIdle = "";

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);

        m_animIdle = CustomAnimation.GetLocomotion(CustomAnimation.LOCOMOTION_ANIM.IDLE);
        m_paramVelocity = CustomAnimation.GetVarible(CustomAnimation.VARIBLE_ANIM.CURRENT_VELOCITY);
        m_paramRandomIdle = CustomAnimation.GetVarible(CustomAnimation.VARIBLE_ANIM.RANDOM_IDLE);
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        //Movement
        float horizontal = m_playerCharacter.m_input.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL);

        if(m_playerCharacter.m_input.GetKeyBool(CustomInput.INPUT_KEY.SPRINT))
        {
            m_character.SetDesiredVelocity(horizontal * m_character.m_groundRunVel * Character.SPRINT_MODIFIER);
        }
        else
            m_character.SetDesiredVelocity(horizontal * m_character.m_groundRunVel);

        return false;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        base.StateEnd();
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_character.m_splinePhysics.m_downCollision && !m_playerCharacter.m_input.GetKeyBool(CustomInput.INPUT_KEY.BLOCK);
    }
}
