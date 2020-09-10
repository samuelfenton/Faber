using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController_InGame : SceneController
{
    public GameObject m_splinePrefab = null;

    private Entity[] m_entities;

    [HideInInspector]
    public CustomInput m_customInput = null;

    public enum INGAME_STATE {IN_GAME, PAUSED}
    private INGAME_STATE m_inGameState = INGAME_STATE.IN_GAME;

    /// <summary>
    /// On setting game state, resets input
    /// </summary>
    public INGAME_STATE InGameState
    {
        get { return m_inGameState; }
        set { 
            m_inGameState = value;
            m_customInput.ResetInput();
        }
    }

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    public override void Init()
    {
        base.Init();

        m_customInput = gameObject.AddComponent<CustomInput>();

        Pathing_Node[] pathingNodes = FindObjectsOfType<Pathing_Node>();

        for (int nodeIndex = 0; nodeIndex < pathingNodes.Length; nodeIndex++)
        {
            pathingNodes[nodeIndex].InitNode(m_splinePrefab);
        }

        m_entities = FindObjectsOfType<Entity>();

        for (int entityIndex = 0; entityIndex < m_entities.Length; entityIndex++)
        {
            m_entities[entityIndex].InitEntity();
        }
    }

    private void Update()
    {
        m_customInput.UpdateInput();
        if(m_inGameState == INGAME_STATE.IN_GAME)
        {
            foreach (Entity entity in m_entities)
            {
                entity.UpdateEntity();
            }
        }
    }
}
