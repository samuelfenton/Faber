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

    public override NavigationController.TURNING GetDesiredTurning(Navigation_Trigger_Junction p_trigger)
    {
        if(m_path.Count > 0)
        {
            Navigation_Spline desiredSpline = m_path[0];

            if (p_trigger.m_forwardLeftSplineInfo.m_spline == desiredSpline)
                return NavigationController.TURNING.LEFT;

            if (p_trigger.m_forwardRightSplineInfo.m_spline == desiredSpline)
                return NavigationController.TURNING.RIGHT;
        }

        return NavigationController.TURNING.CENTER;
    }
}
