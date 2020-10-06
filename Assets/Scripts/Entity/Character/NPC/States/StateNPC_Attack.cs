using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateNPC_Attack : State_NPC
{
    public bool m_lightAttackFlag = false;
    public bool m_heavyAttackFlag = false;

    private AttackController m_weaponManager = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_entity">Parent entity reference</param>
    public override void StateInit(bool p_loopedState, Entity p_entity)
    {
        base.StateInit(p_loopedState, p_entity);
        m_weaponManager = m_NPCCharacter.GetComponent<AttackController>();
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

        //Reset Data
        m_lightAttackFlag = false;
        m_heavyAttackFlag = false;

        if (SmartTargetWithinRange(m_NPCCharacter.m_targetCharacter, m_NPCCharacter.m_attackingDistance)) //Nto facing right way, or enemy has moved away end
        {
            //Setup flags
            int randomIndex = Random.Range(0, 3);

            if(randomIndex <= 1) // 66%
            {
                m_lightAttackFlag = true;
            }
            else
            {
                m_heavyAttackFlag = true;
            }
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
        return m_NPCCharacter.m_targetCharacter != null && SmartTargetWithinRange(m_NPCCharacter.m_targetCharacter, m_NPCCharacter.m_attackingDistance);
    }
}
