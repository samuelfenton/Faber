using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMeshHandeling : MonoBehaviour
{
    //--------------------
    //  Get mesh from eith skinned mesh renderer or mesh renderer
    //  
    //  params:
    //      p_object - object to get mesh for
    //  return:
    //      Mesh - correct mesh based off avalibilty of renderers in children
    //--------------------
    public static Mesh GetMesh(GameObject p_object)
    {
        if (p_object == null) //Early breakout
            return null;

        MeshFilter meshFilter = p_object.GetComponentInChildren<MeshFilter>();
        if (meshFilter != null)
        {
            return meshFilter.mesh;
        }
        else
        {
            SkinnedMeshRenderer skinnedMeshRenderer = p_object.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                Mesh bakedMesh = new Mesh();
                skinnedMeshRenderer.BakeMesh(bakedMesh);
                return bakedMesh;
            }
        }

        return null;
    }

    //--------------------
    //  Set the mesh of a object regardless of if its using skinned renderer or mesh renderer
    //  
    //  params:
    //      p_mesh - mesh to assign
    //      p_object - object to set mesh for
    //--------------------
    public static void SetMesh(Mesh p_mesh, GameObject p_object)
    {
        if (p_mesh == null || p_object == null) //Early breakout
            return;

        MeshFilter meshFilter = p_object.GetComponentInChildren<MeshFilter>();
        if(meshFilter != null)
        {
            meshFilter.mesh = p_mesh;
        }
        else
        {
            SkinnedMeshRenderer skinnedMeshRenderer = p_object.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
                skinnedMeshRenderer.sharedMesh = p_mesh;
        }
    }

    //--------------------
    //  Combine a series of meshes into a single one
    //  
    //  params:
    //      m_meshObjects - objects to combine
    //      p_offsetPosition - Offset for all objects, example if in reference to parent object
    //  return:
    //      Mesh - single combined mesh
    //--------------------
    public static Mesh CombineMeshes(GameObject[] m_meshObjects, Vector3 p_offsetPosition)
    {
        if (m_meshObjects.Length == 0) //Early breakout
            return null;

        // combine meshes
        CombineInstance[] combine = new CombineInstance[m_meshObjects.Length];
        int i = 0;
        while (i < m_meshObjects.Length)
        {
            Transform newTransform = m_meshObjects[i].transform;

            newTransform.position -= p_offsetPosition;//Apply offset in position

            combine[i].mesh = GetMesh(m_meshObjects[i]);
            combine[i].transform = newTransform.localToWorldMatrix;

            i++;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        //Save new mesh
        return combinedMesh;
    }

    //--------------------
    //  Combine a series of meshes into a single one
    //  
    //  params:
    //      m_meshObjects - objects to combine
    //  return:
    //      Mesh - single combined mesh
    //--------------------
    public static Mesh CombineMeshes(GameObject[] m_meshObjects)
    {
        return CombineMeshes(m_meshObjects, Vector3.zero);
    }
}
