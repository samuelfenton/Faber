using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController_InGame : SceneController
{
    private Entity[] m_entities;
    private Pathing_Spline[] m_splines;
    private Pathing_Node[] m_nodes;

    public override void InitController()
    {
        m_entities = FindObjectsOfType<Entity>();
        m_splines = FindObjectsOfType<Pathing_Spline>();
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
