using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState_Attack : State_Player
{
    private enum ATTACK_STATE {INITIAL, FINISHED }
    private ATTACK_STATE m_currentState = ATTACK_STATE.INITIAL;
    private AttackController m_weaponManager = null;

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

        //TODO logic to determine type of attack, in air vs ground vs sprint
        ManoeuvreController.MANOEUVRE_TYPE currentType = m_character.m_splinePhysics.m_downCollision ? (Mathf.Abs(m_character.m_splinePhysics.m_splineVelocity.x) > m_character.m_groundRunVel ? ManoeuvreController.MANOEUVRE_TYPE.SPRINT : ManoeuvreController.MANOEUVRE_TYPE.GROUND) : ManoeuvreController.MANOEUVRE_TYPE.INAIR;
        ManoeuvreController.MANOEUVRE_STANCE currentStance = ManoeuvreController.MANOEUVRE_STANCE.LIGHT;

        if (m_player.m_customInput.GetKeyBool(CustomInput.INPUT_KEY.HEAVY_ATTACK))
        {
            currentStance = ManoeuvreController.MANOEUVRE_STANCE.HEAVY;
        }

        m_weaponManager.StartAttack(currentType, currentStance);

        m_currentState = ATTACK_STATE.INITIAL;
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true</returns>
    public override bool StateUpdate()
    {
        switch (m_currentState)
        {
            case ATTACK_STATE.INITIAL:
                if(m_weaponManager.UpdateAttack())
                {
                    m_currentState = ATTACK_STATE.FINISHED;
                }
                break;
            case ATTACK_STATE.FINISHED:
                return m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.BASE);
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
