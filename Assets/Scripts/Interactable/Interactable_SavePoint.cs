using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Interactable_SavePoint : Interactable
{
    [Header("Assigned Variables")]
    public GameObject m_effectObjectOnLit = null;
    [Header("Respawn Point")]
    public Pathing_Node m_nodeA = null;
    public Pathing_Node m_nodeB = null;
    [Range(0.0f, 1.0f)]
    public float m_splinePercent = 0.0f;

    public enum SAVEPOINT_STATE {LIT, UNLIT}
    public SAVEPOINT_STATE m_currentState = SAVEPOINT_STATE.UNLIT;


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

        m_currentState = SAVEPOINT_STATE.UNLIT;
        m_effectObjectOnLit.SetActive(false);
    }


    /// <summary>
    /// Called when player tries to interact
    /// </summary>
    public override void Interact()
    {
        base.Interact();

        DataController.SaveInGameSaveData(this);
        DataController.SaveCharacterStatistics(m_playerCharacter.m_characterStatistics);

        ToggleLit();
    }

    /// <summary>
    /// Toggle if shrine is lit
    /// </summary>
    public void ToggleLit()
    {
        if (m_currentState == SAVEPOINT_STATE.UNLIT)
        {
            m_currentState = SAVEPOINT_STATE.LIT;
            m_effectObjectOnLit.SetActive(true);
        }
    }
}
