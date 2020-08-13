using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController_InGame : SceneController
{
    private Entity[] m_entities;
    private Pathing_Node[] m_nodes;

    /// <summary>
    /// Setup varibels to be used in UI
    /// </summary>
    /// <param name="p_masterController">Master controller</param>
    public override void Init(MasterController p_masterController)
    {
        base.Init(p_masterController);

        m_entities = FindObjectsOfType<Entity>();
        m_nodes = FindObjectsOfType<Pathing_Node>();

        for (int nodeIndex = 0; nodeIndex < m_nodes.Length; nodeIndex++)
        {
            m_nodes[nodeIndex].InitNode();
        }

        for (int entityIndex = 0; entityIndex < m_entities.Length; entityIndex++)
        {
            m_entities[entityIndex].InitEntity();
        }
    }
}
