using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_GroundAttack : Player_State
{
    private const float PERFORM_PERCENT = 0.1f;
    private const float END_ANIMATION_PERCENT = 0.8f;

    private enum ATTACK_STATE {PERFORMING, CAN_COMBO, END }
    private ATTACK_STATE m_currentState = ATTACK_STATE.PERFORMING;

    private bool m_comboFlag = false;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    public override void StateInit()
    {
        base.StateInit();
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        if(m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK) == CustomInput.INPUT_STATE.DOWNED)//Base attack
        {
            m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK, true);
        }
        else if (m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK_SECONDARY) == CustomInput.INPUT_STATE.DOWNED)//Heavy attack
        {
            m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.HEAVY_ATTACK, true);
        }
        m_parentPlayer.m_rightHandFire.SetActive(true);

        SwapAttackingState(ATTACK_STATE.PERFORMING);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// Key info, at 0.8 of animtion stop checking to ensure wont go to next animation by accident, as trasistion occurs at .9
    /// hence 0.1-0.8 asking for combo
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true </returns>
    public override bool UpdateState()
    {
        float animationPercent = m_characterAnimationController.GetNormalizedTime();

        switch (m_currentState)//Update state
        {
            case ATTACK_STATE.PERFORMING:
                if (animationPercent > PERFORM_PERCENT && animationPercent < END_ANIMATION_PERCENT)
                    SwapAttackingState(ATTACK_STATE.CAN_COMBO);
                break;
            case ATTACK_STATE.CAN_COMBO:
                if(!m_comboFlag)
                {
                    if (m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK) == CustomInput.INPUT_STATE.DOWNED)//Base attack
                    {
                        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK, true);
                        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.HEAVY_ATTACK, false);
                        m_comboFlag = true;

                    }
                    else if (m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK_SECONDARY) == CustomInput.INPUT_STATE.DOWNED)//Heavy attack
                    {
                        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK, false);
                        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.HEAVY_ATTACK, true);
                        m_comboFlag = true;
                    }
                }

                if (animationPercent > END_ANIMATION_PERCENT)
                {
                    if(m_comboFlag)
                        SwapAttackingState(ATTACK_STATE.PERFORMING);
                    else
                        SwapAttackingState(ATTACK_STATE.END);
                }
                break;
            case ATTACK_STATE.END:
                return true;
            default:
                break;
        }

        return m_characterAnimationController.EndOfAnimation();
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_parentPlayer.m_rightHandFire.SetActive(false);

        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK, false);
        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.HEAVY_ATTACK, false);
    }

    /// <summary>
    /// Is this currently a valid state
    /// </summary>
    /// <returns>True when valid, e.g. Death requires players to have no health</returns>
    public override bool IsValid()
    {
        return m_parentCharacter.m_splinePhysics.m_downCollision && 
            (m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK) == CustomInput.INPUT_STATE.DOWNED ||
            m_parentPlayer.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK_SECONDARY) == CustomInput.INPUT_STATE.DOWNED);
    }

    private void SwapAttackingState(ATTACK_STATE p_newState)
    {
        switch (m_currentState) //end of old state
        {
            case ATTACK_STATE.PERFORMING:
                break;
            case ATTACK_STATE.CAN_COMBO:
                break;
            case ATTACK_STATE.END:
                break;
            default:
                break;
        }


        switch (p_newState)//Start of new state
        {
            case ATTACK_STATE.PERFORMING:
                m_comboFlag = false;
                break;
            case ATTACK_STATE.CAN_COMBO:
                break;
            case ATTACK_STATE.END:
                m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.LIGHT_ATTACK, false);
                m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.HEAVY_ATTACK, false);
                break;
            default:
                break;
        }

        m_currentState = p_newState;
    }
}
