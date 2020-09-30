using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_TargetDummy : Character
{
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
}
