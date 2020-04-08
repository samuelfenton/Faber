using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState_Approach : NPC_State
{
    private string m_paramVelocity = "";

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);

        m_paramVelocity = AnimController.GetVarible(AnimController.VARIBLE_ANIM.CURRENT_VELOCITY);
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

        MoveTowardsEntity(m_NPCCharacter.m_targetCharacter, m_character.m_groundRunVel);

        return SmartTargetWithinRange(m_NPCCharacter.m_targetCharacter, m_NPCCharacter.m_attackingDistance) || !SmartTargetWithinRange(m_NPCCharacter.m_targetCharacter, m_NPCCharacter.m_detectionDistance);
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
        return m_NPCCharacter.m_targetCharacter != null && SmartTargetWithinRange(m_NPCCharacter.m_targetCharacter, m_NPCCharacter.m_detectionDistance);
    }
}
