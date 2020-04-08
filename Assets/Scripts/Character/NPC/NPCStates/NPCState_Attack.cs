using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState_Attack : NPC_State
{
    public bool m_lightAttackFlag = false;
    public bool m_heavyAttackFlag = false;

    private WeaponManager m_weaponManager = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    /// <param name="p_loopedState">Will this state be looping?</param>
    /// <param name="p_character">Parent character reference</param>
    public override void StateInit(bool p_loopedState, Character p_character)
    {
        base.StateInit(p_loopedState, p_character);

    }

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        m_character.FaceDirection(GetDesiredTargetFacing());

        m_weaponManager = m_character.GetCurrentWeapon();
        m_weaponManager.StartAttackSequence();

        m_character.SetDesiredVelocity(0.0f);
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

        if (GetDesiredTargetFacing() == m_character.GetFacingDir() && SmartTargetWithinRange(m_NPCCharacter.m_targetCharacter, m_NPCCharacter.m_attackingDistance)) //Nto facing right way, or enemy has moved away end
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

        return m_weaponManager.UpdateAttackSequence();
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

    /// <summary>
    /// Get the desired facing direction
    /// </summary>
    /// <returns>Right when allinged towards enemy</returns>
    public Character.FACING_DIR GetDesiredTargetFacing()
    {
        if (m_NPCCharacter.m_targetCharacter == null)
            return Character.FACING_DIR.RIGHT;

        float enemyAlignedDot = Vector3.Dot(transform.forward, (m_NPCCharacter.m_targetCharacter.transform.position - transform.position).normalized);

        if (enemyAlignedDot >= 0.0f)
            return Character.FACING_DIR.RIGHT;
        return Character.FACING_DIR.LEFT;
    }
}
