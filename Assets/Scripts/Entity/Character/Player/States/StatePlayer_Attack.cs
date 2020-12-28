using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatePlayer_Attack : State_Player
{
    private AttackController m_weaponManager = null;

    private enum ATTACK_STATE {PERFORMING_ATTACK, RETURN_TO_SHEATH}
    private ATTACK_STATE m_attackState = ATTACK_STATE.PERFORMING_ATTACK;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Entity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);
        m_weaponManager = m_player.GetComponent<AttackController>();
    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        m_player.SetDesiredVelocity(0.0f);
        m_attackState = ATTACK_STATE.PERFORMING_ATTACK;

        //TODO logic to determine type of attack, in air vs ground vs sprint
        ManoeuvreController.MANOEUVRE_TYPE currentType = m_character.m_splinePhysics.m_downCollision ? (Mathf.Abs(m_character.m_splinePhysics.m_splineLocalVelocity.x) > m_character.m_groundRunVel ? ManoeuvreController.MANOEUVRE_TYPE.SPRINT : ManoeuvreController.MANOEUVRE_TYPE.GROUND) : ManoeuvreController.MANOEUVRE_TYPE.INAIR;
        ManoeuvreController.MANOEUVRE_STANCE currentStance = m_player.m_customInput.GetKeyBool(CustomInput.INPUT_KEY.LIGHT_ATTACK) ? ManoeuvreController.MANOEUVRE_STANCE.LIGHT : ManoeuvreController.MANOEUVRE_STANCE.HEAVY;

        m_weaponManager.StartAttack(currentType, currentStance);
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true</returns>
    public override bool StateUpdate()
    {
        if(m_attackState == ATTACK_STATE.PERFORMING_ATTACK)
        {
            if (m_weaponManager.UpdateAttack())
            {
                if(m_weaponManager.m_currentController != null && m_weaponManager.m_currentController.m_requiresSheathingBlend)
                {
                    m_customAnimator.PlayAnimation(CustomAnimation.END_ATTACK_BLEND, CustomAnimation.BLEND_TIME.SHORT);
                    m_attackState = ATTACK_STATE.RETURN_TO_SHEATH;
                    return true;
                }
                else
                {
                    return true;
                }
            }
        }
        else
        {
            return m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.ATTACK);
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
        return m_player.m_customInput.GetKeyBool(CustomInput.INPUT_KEY.LIGHT_ATTACK) || m_player.m_customInput.GetKeyBool(CustomInput.INPUT_KEY.HEAVY_ATTACK);
    }
}
