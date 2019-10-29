using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_State : State
{
    protected NPC_StateMachine m_NPCStateMachine = null;
    protected NPC_Character m_parentNPC = null;

    /// <summary>
    /// Initilse the state, runs only once at start
    /// </summary>
    public override void StateInit()
    {
        base.StateInit();

        m_NPCStateMachine = (NPC_StateMachine)m_parentStateMachine;
        m_parentNPC = (NPC_Character)m_parentCharacter;
    }

    /// <summary>
    /// Is the character close enough based off current speed and slowdown rate
    ///     Kinematic equation
    ///     Vf^2 = Vi^2 + 2ad
    ///     d = -Vi^2 / 2a
    ///     As a is negitive -> d = Vi^2 / 2a
    /// </summary>
    /// <returns></returns>
    protected bool CloseEnough()
    {
        float stoppingDistance = m_parentCharacter.m_localVelocity.x == 0.0f ? 0 : Mathf.Pow(m_parentCharacter.m_localVelocity.x, 2) / (2 * m_parentCharacter.m_groundedHorizontalDeacceleration); //Close enough based off time to slow down

        return (m_parentNPC.m_targetCharacter != null && MOARMaths.SqrDistance(m_parentCharacter.transform.position, m_parentNPC.m_targetCharacter.transform.position) <= Mathf.Pow(m_parentNPC.m_attackingDistance + stoppingDistance, 2));
    }
}
