using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatePlayer_InAir : State_Player
{
    private enum IN_AIR_STATE {IN_AIR, SECOND_JUMP}
    private IN_AIR_STATE m_inAirState = IN_AIR_STATE.IN_AIR;

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

        m_inAirState = IN_AIR_STATE.IN_AIR;

        m_customAnimator.PlayAnimation((int)CustomAnimation_Player.BASE_DEFINES.INAIR, CustomAnimation.LAYER.BASE, CustomAnimation.BLEND_TIME.SHORT);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool StateUpdate()
    {
        base.StateUpdate();

        //Allow player to move in air
        m_player.ApplyHorizontalMovement(true);

        switch (m_inAirState)
        {
            case IN_AIR_STATE.IN_AIR:
                //Double Jump
                if (m_character.m_doubleJumpFlag && m_player.m_customInput.GetKey(CustomInput.INPUT_KEY.JUMP) == CustomInput.INPUT_STATE.DOWNED)
                {
                    m_customAnimator.PlayAnimation((int)CustomAnimation_Player.BASE_DEFINES.DOUBLE_JUMP, CustomAnimation.LAYER.BASE);

                    m_entity.m_splinePhysics.HardSetUpwardsVelocity(m_character.m_doubleJumpSpeed);

                    m_character.m_doubleJumpFlag = false;

                    m_inAirState = IN_AIR_STATE.SECOND_JUMP;
                }
                break;
            case IN_AIR_STATE.SECOND_JUMP:
                if(m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.BASE))
                {
                    m_customAnimator.PlayAnimation((int)CustomAnimation_Player.BASE_DEFINES.INAIR, CustomAnimation.LAYER.BASE);

                    m_inAirState = IN_AIR_STATE.IN_AIR;
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
        return !m_entity.m_splinePhysics.m_downCollision;
    }
}
