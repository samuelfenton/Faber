using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_NPC : Character
{
    [Header("NPC Logic")]
    public List<Navigation_Spline> m_path = new List<Navigation_Spline>();
    public Character m_targetCharacter = null;

    private BehaviourTree m_behaviourTree = null;

    protected override void Start()
    {
        base.Start();

        m_behaviourTree = GetComponent<BehaviourTree>();
    }

    protected override void Update()
    {
        base.Update();

        m_behaviourTree.RunBehaviourTree(this);
    }
}
