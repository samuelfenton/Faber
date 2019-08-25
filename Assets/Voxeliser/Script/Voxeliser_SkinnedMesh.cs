using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxeliser_SkinnedMesh : Voxeliser
{
    private SkinnedMeshRenderer m_skinnedMeshRenderer = null;

    /// <summary>
    /// Setup of the Voxeliser
    /// Ensure object has all required components atached
    /// Setup required varibles
    /// </summary>
    protected override void Start()
    {
        base.Start();
        m_skinnedMeshRenderer = m_objectWithMesh.GetComponent<SkinnedMeshRenderer>();

        if (m_skinnedMeshRenderer == null)
        {
            Destroy(this);
#if UNITY_EDITOR
            Debug.Log(name + " Object doesnt have an attached skinned renderer, maybe use static or normal was intended?");
#endif
        }
    }

    /// <summary>
    /// Update the mesh used in determining vertex position
    /// Overriden by skinned renderer/ mesh renderer derrived classes
    /// </summary>
    protected override void UpdateMesh()
    {
        m_modelMesh = new Mesh();
        m_skinnedMeshRenderer.BakeMesh(m_modelMesh);
    }
}
