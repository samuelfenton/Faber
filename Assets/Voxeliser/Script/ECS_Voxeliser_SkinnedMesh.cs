using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ECS_Voxeliser_SkinnedMesh : ECS_Voxeliser
{
    private SkinnedMeshRenderer m_skinnedMeshRenderer = null;

    //--------------------
    //  Setup of the Voxeliser
    //      Get skinned mesh renderer
    //      Base implementaion
    //--------------------
    protected override void Start()
    {
        base.Start();
        m_skinnedMeshRenderer = m_objectWithMesh.GetComponent<SkinnedMeshRenderer>();

        if (m_skinnedMeshRenderer == null)
            Destroy(this);
    }

    //--------------------
    //  Update the mesh used in determining vertex position
    //  Bake mesh to ensure up to date animation frames
    //--------------------
    protected override void UpdateMesh()
    {
        m_modelMesh = new Mesh();
        m_skinnedMeshRenderer.BakeMesh(m_modelMesh);
    }
}
