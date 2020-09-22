using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Block : State_Player
{
    private enum BLOCK_STATE {START, BLOCKING, END }
    private BLOCK_STATE m_currentState = BLOCK_STATE.START;

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

        m_customAnimator.PlayBase(CustomAnimation.BASE_DEFINES.BLOCK_FROM_IDLE);
        m_currentState = BLOCK_STATE.START;
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        switch (m_currentState)
        {
            case BLOCK_STATE.START:
                if(m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.BASE))
                {
                    m_customAnimator.PlayBase(CustomAnimation.BASE_DEFINES.BLOCK);

                    m_currentState = BLOCK_STATE.BLOCKING;
                    m_character.m_blockingFlag = true;
                }
                break;
            case BLOCK_STATE.BLOCKING:
                if(!m_player.m_customInput.GetKeyBool(CustomInput.INPUT_KEY.BLOCK))
                {
                    m_character.m_blockingFlag = false;

                    m_currentState = BLOCK_STATE.END;
                    m_customAnimator.PlayBase(CustomAnimation.BASE_DEFINES.BLOCK_TO_IDLE);
                }
                if(!m_entity.m_splinePhysics.m_downCollision.m_collision)
                {
                    return true;
                }
                break;
            case BLOCK_STATE.END:
                if (m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.BASE))
                {
                    return true;
                }
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
        return m_entity.m_splinePhysics.m_downCollision.m_collision && m_player.m_customInput.GetKeyBool(CustomInput.INPUT_KEY.BLOCK);
    }
}
