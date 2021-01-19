using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatePlayer_Attack : State_Player
{
    private ManoeuvreController m_manoeuvreController = null;

    private enum ATTACK_STATE {PERFORMING_ATTACK, RETURN_TO_SHEATH, END_MANOEUVRE }
    private ATTACK_STATE m_attackState = ATTACK_STATE.END_MANOEUVRE;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Entity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);
        m_manoeuvreController = m_player.m_manoeuvreController;
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
        Manoeuvre.MANOEUVRE_TYPE currentType = m_character.m_splinePhysics.m_downCollision ? (Mathf.Abs(m_character.m_splinePhysics.m_splineLocalVelocity.x) > m_character.m_groundRunVel ? Manoeuvre.MANOEUVRE_TYPE.SPRINT : Manoeuvre.MANOEUVRE_TYPE.GROUND) : Manoeuvre.MANOEUVRE_TYPE.INAIR;
        Manoeuvre.MANOEUVRE_STANCE currentStance = m_player.m_customInput.GetKeyBool(CustomInput.INPUT_KEY.LIGHT_ATTACK) ? Manoeuvre.MANOEUVRE_STANCE.LIGHT : Manoeuvre.MANOEUVRE_STANCE.HEAVY;

        Manoeuvre manoeuvre = m_manoeuvreController.GetInitialManoeuvre(currentStance, currentType);

        if (manoeuvre != null)
        {
            m_manoeuvreController.StartManoeuvre(manoeuvre);
        }
        else
        {
            m_attackState = ATTACK_STATE.END_MANOEUVRE;
        }
    }

    /// <summary>
    /// State update, perform any actions for the given state
    /// </summary>
    /// <returns>Has this state been completed, e.g. Attack has completed, idle would always return true</returns>
    public override bool StateUpdate()
    {
        if (m_attackState == ATTACK_STATE.PERFORMING_ATTACK)
        {
            m_manoeuvreController.UpdateManoeuvre();

            if (m_manoeuvreController.HasManoeuvreCompleted()) //Get next manoeuvre if possible
            {
                m_manoeuvreController.EndManoeuvre();

                Manoeuvre manoeuvre = m_manoeuvreController.GetNextManoeuvre();

                if (manoeuvre == null) //There are no more combo manoeuvres, check for blend
                {
                    if (m_manoeuvreController.m_currentManoeuvre.m_requiresSheathingBlend) // Does this attack need to blend using sheathing animation?
                    {
                        m_customAnimator.PlayAnimation(CustomAnimation.END_ATTACK_BLEND, CustomAnimation.BLEND_TIME.LONG);
                        m_attackState = ATTACK_STATE.RETURN_TO_SHEATH;
                        return false;
                    }
                    else
                    {
                        m_attackState = ATTACK_STATE.END_MANOEUVRE;
                        return true;
                    }
                }
                else //Start next manoeuvre
                {
                    m_manoeuvreController.StartManoeuvre(manoeuvre);
                    return false;
                }
            }
        }
        else if (m_attackState == ATTACK_STATE.RETURN_TO_SHEATH)
        {
            return m_customAnimator.IsAnimationDone(CustomAnimation.LAYER.ATTACK);
        }
        else //End of attack
        {
            return true;
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
