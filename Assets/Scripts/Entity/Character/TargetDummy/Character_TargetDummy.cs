using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_TargetDummy : Character
{
    [Header("Assigned Variables")]
    public GameObject m_baseObject = null;
    public GameObject m_bodyObject = null;

    public GameObject m_deathEffect = null;

    protected StateMachine_TargetDummy m_stateMachine = null;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();

        //Init
        m_stateMachine = gameObject.AddComponent<StateMachine_TargetDummy>();

        m_stateMachine.InitStateMachine(this);//Run first as animation depends on states being created

        if (m_deathEffect != null)
            m_deathEffect.SetActive(false);
    }

    /// <summary>
    /// Update an entity, this should be called from scene controller
    /// Used to handle different scene state, pause vs in game etc
    /// </summary>
    public override void UpdateEntity()
    {
        m_stateMachine.UpdateStateMachine();

        base.UpdateEntity();
    }

    /// <summary>
    /// Called as an entity is destroyed intially
    /// </summary>
    protected override void EntityInitialDestory()
    {
        if (m_baseObject != null)
            m_baseObject.SetActive(false);
        if (m_bodyObject != null)
            m_bodyObject.SetActive(false);

        m_deathEffect.transform.parent = null;

        if (m_deathEffect != null)
            m_deathEffect.SetActive(true);

        Destroy(m_deathEffect, 5.0f);
        
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Apply delay for entity destruction to allow any effects
    /// </summary>
    protected override void EntityDelayedDestroy()
    {
    }
}
