using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxeliserCompanion_SkinnedMeshRenderer : VoxeliserCompanion
{
    SkinnedMeshRenderer skinnedMeshRenderer = null;

    protected override void MeshInit()
    {
        //Get Mesh
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null)
        {
            m_modelMesh = Instantiate(skinnedMeshRenderer.sharedMesh);//Duplicated shared mesh to not override orginal
            skinnedMeshRenderer.sharedMesh = m_modelMesh;
        }
        else
        {
            Debug.Log("Object has no attached skinned renderer, maybe its a mesh filter?");
            Destroy(this);
        }
    }

    protected override void UpdateMesh()
    {
        m_modelMesh = new Mesh();
        skinnedMeshRenderer.BakeMesh(m_modelMesh);
    }

}
