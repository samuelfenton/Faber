using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController_InGame : SceneController
{
    public GameObject m_splinePrefab = null;

    private Entity[] m_entities;

    [HideInInspector]
    public CustomInput m_customInput = null;

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
}
