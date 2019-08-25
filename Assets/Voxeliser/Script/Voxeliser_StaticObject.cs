using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxeliser_StaticObject : Voxeliser
{
    /// <summary>
    /// Rather than using Update Voxeliser is Enumerator driven.
    /// Update mesh used
    /// Run voxeliser
    /// Apply this voxel mesh to the orginal mesh and discontinue IEnumerator functionality
    /// </summary>
    /// <returns>null, wait for next frame</returns>
    protected override IEnumerator VoxeliserUpdate()
    {
        Transform orginalParent = transform.parent;
        transform.SetParent(null, true);

        Vector3 orginalPos = m_objectWithMesh.transform.position;
        Quaternion orginalRot = m_objectWithMesh.transform.rotation;
        Vector3 orginalScale = m_objectWithMesh.transform.localScale;


        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        UpdateMesh();
        ConvertToVoxels();

        //delay to allow late update to occur
        yield return null;

        //Check if currently has skinned mesh, if so remocve and add in mesh filter
        SkinnedMeshRenderer skinnedMesh = m_objectWithMesh.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMesh != null)
        {
            DestroyImmediate(skinnedMesh);

            MeshFilter meshFilter = m_objectWithMesh.AddComponent<MeshFilter>();
            m_objectWithMesh.AddComponent<MeshRenderer>();

            m_modelMesh = meshFilter.mesh;
        }

        m_modelMesh.Clear();

        //Bring back material 
        MeshRenderer meshRenderer = m_objectWithMesh.GetComponent<MeshRenderer>();
        meshRenderer.material = CustomMeshHandeling.GetMaterial(m_voxelObject);

        m_modelMesh.SetVertices(new List<Vector3>(m_voxelMesh.vertices));
        m_modelMesh.SetUVs(0, new List<Vector2>(m_voxelMesh.uv));
        m_modelMesh.SetTriangles(m_voxelMesh.triangles, 0);

        m_modelMesh.Optimize();
        m_modelMesh.RecalculateNormals();


        //Get orginal transform
        transform.position = orginalPos;
        transform.rotation = orginalRot;
        transform.localScale = orginalScale;

        transform.SetParent(orginalParent, true);

        Destroy(m_voxelObject);
        Destroy(this);
    }
}
