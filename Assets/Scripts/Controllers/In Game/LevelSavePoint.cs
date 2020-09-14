using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LevelSavePoint : Interactable
{
    public Pathing_Node m_nodeA = null;
    public Pathing_Node m_nodeB = null;
    [Range(0.0f, 1.0f)]
    public float m_splinePercent = 0.0f;

    /// <summary>
    /// Setup a unique id for every interactable
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
    }

    private void OnDrawGizmos()
    {
        if (MOARDebugging.GetSplinePosition(m_nodeA, m_nodeB, m_splinePercent, out Vector3 position))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(position, position + Vector3.up);
        }
    }

    /// <summary>
    /// Initilise the interactable
    /// </summary>
    /// <param name="p_player">Player character</param>
    public override void InitInteractable(Character_Player p_player)
    {
        base.InitInteractable(p_player);
    }

    /// <summary>
    /// Used rather than update
    /// </summary>
    public override void UpdateInteractable()
    {
        base.UpdateInteractable();
    }

    /// <summary>
    /// Called when player tries to interact
    /// </summary>
    public override void Interact()
    {
        base.Interact();

        DataController.SaveLevelData(this);
    }
}
