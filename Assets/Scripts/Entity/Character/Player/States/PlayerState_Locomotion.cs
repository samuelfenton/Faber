using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Locomotion : State_Player
{
    private enum LOCOMOTION_STATE {PREVIOUS_STATE_ANIMATION, IDLING, LOCOMOTION, LOCOMOTION_TO_SPRINT, SPRINT }
    private LOCOMOTION_STATE m_currentState = LOCOMOTION_STATE.LOCOMOTION;

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

        if(!m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.BASE))
        {
            m_currentState = LOCOMOTION_STATE.PREVIOUS_STATE_ANIMATION;
        }
        else
        {
            m_customAnimator.PlayAnimation(CustomAnimation.BASE_DEFINES.LOCOMOTION);
            m_currentState = LOCOMOTION_STATE.LOCOMOTION;
        }

        m_character.m_idleDelayTimer = 0.0f;
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        //Movement
        m_player.ApplyHorizontalMovement(true);

        //Animation
        switch (m_currentState)
        {
            case LOCOMOTION_STATE.PREVIOUS_STATE_ANIMATION:
                if(m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.BASE))
                {
                    m_customAnimator.PlayAnimation(CustomAnimation.BASE_DEFINES.LOCOMOTION);
                    m_currentState = LOCOMOTION_STATE.LOCOMOTION;
                }
                break;
            case LOCOMOTION_STATE.IDLING:
                m_character.m_idleDelayTimer += Time.deltaTime;

                if(m_player.m_customInput.AnyInput())
                {
                    m_currentState = LOCOMOTION_STATE.LOCOMOTION;
                    m_character.m_idleDelayTimer = 0.0f;
                }
                break;
            case LOCOMOTION_STATE.LOCOMOTION:
                if(Mathf.Abs(m_entity.m_splinePhysics.m_splineLocalVelocity.x) > m_character.m_groundRunVel) //Character is sprinting
                {
                    m_customAnimator.PlayAnimation(CustomAnimation.BASE_DEFINES.RUN_TO_SPRINT);
                    m_currentState = LOCOMOTION_STATE.LOCOMOTION_TO_SPRINT;
                }
                break;
            case LOCOMOTION_STATE.LOCOMOTION_TO_SPRINT:
                if (m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.BASE))
                {
                    if (Mathf.Abs(m_entity.m_splinePhysics.m_splineLocalVelocity.x) > m_character.m_groundRunVel)//Still sprinting
                    {
                        m_currentState = LOCOMOTION_STATE.SPRINT;
                        m_customAnimator.PlayAnimation(CustomAnimation.BASE_DEFINES.SPRINT);
                    }
                    else //Slow downed during animation
                    {
                        m_currentState = LOCOMOTION_STATE.LOCOMOTION;
                        m_customAnimator.PlayAnimation(CustomAnimation.BASE_DEFINES.LOCOMOTION);
                    }
                }
                break;
            case LOCOMOTION_STATE.SPRINT:
                if (Mathf.Abs(m_entity.m_splinePhysics.m_splineLocalVelocity.x) <= m_character.m_groundRunVel) //Character is no longer sprinting
                {
                    m_customAnimator.PlayAnimation(CustomAnimation.BASE_DEFINES.LOCOMOTION);
                    m_currentState = LOCOMOTION_STATE.LOCOMOTION;
                }
                break;
            default:
                break;
        }

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
        return m_entity.m_splinePhysics.m_downCollision && !m_player.m_customInput.GetKeyBool(CustomInput.INPUT_KEY.BLOCK);
    }
}
