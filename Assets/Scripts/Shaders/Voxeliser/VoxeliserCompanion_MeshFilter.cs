using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxeliserCompanion_MeshFilter : VoxeliserCompanion
{
    MeshFilter m_meshFilter = null;

    protected override void MeshInit()
    {
        m_meshFilter = GetComponentInChildren<MeshFilter>();
        if (m_meshFilter != null)
        {
            m_modelMesh = Instantiate(m_meshFilter.mesh);//Duplicated shared mesh to not override orginal
            m_meshFilter.mesh = m_modelMesh;
        }
        else
        {
            Debug.Log("Object has no attached mesh filter, maybe its a skinned renderer?");
            Destroy(this);
        }
    }
}
