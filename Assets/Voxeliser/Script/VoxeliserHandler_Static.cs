using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VoxeliserHandler_Static : VoxeliserHandler
{
    [Header("Saving")]
    public bool m_saveMeshOnPlay = false;

    //--------------------
    //  Initialise all voxels used
    //  Static mesh requires material to function, so dont disable.
    //  params:
    //      p_voxelCount - How many voxels to create
    //--------------------
    public override void InitVoxels(int p_voxelCount)
    {
        m_disableMaterialsOnRun = false;
        base.InitVoxels(p_voxelCount);
    }

    //--------------------
    //  Static meshes dont need to be updated every frame, instead single mesh can be created and used
    //      Setup voxels as needed, Combine meshes
    //      In case of saving, do so
    //      Remove all voxels in destroy
    //  params:
    //      p_voxelPositions - Positions of all voxels for this frame
    //      p_colors - Colors of all voxels for this frame
    //--------------------
    public override IEnumerator HandleVoxels(HashSet<VectorDouble> p_voxelPositions, List<Vector3> p_colors)
    {
        //Base setup of voxels
        int colorIndex = 0;
        foreach (VectorDouble position in p_voxelPositions)
        {
            AddVoxel(position, p_colors[colorIndex]);
            colorIndex++;
        }

        GameObject[] meshObjects = new GameObject[m_voxelsInUse.Count];
        int iteratorIndex = 0;
        foreach (Voxel usedVoxel in m_voxelsInUse.Values)
        {
            meshObjects[iteratorIndex] = usedVoxel.gameObject;
            iteratorIndex++;
        }

        CustomMeshHandeling.SetMesh(CustomMeshHandeling.CombineMeshes(meshObjects, transform.position), gameObject);

#if UNITY_EDITOR
        //Saving
        if (m_saveMeshOnPlay)
        {
            string path = EditorUtility.SaveFilePanel("Save Mesh", "Assets/", name, "Mesh");
            if (!string.IsNullOrEmpty(path))
            {
                path = FileUtil.GetProjectRelativePath(path);

                MeshUtility.Optimize(m_voxelCompanion.m_modelMesh);

                AssetDatabase.CreateAsset(CustomMeshHandeling.GetMesh(gameObject), path);
                AssetDatabase.SaveAssets();
            }
        }
#endif

        //Voxels placed relative to orginal rotation so remove roation once new mesh created
        transform.rotation = Quaternion.identity;

        //Cleanup
        Destroy(m_voxelCompanion);
        Destroy(this);

        yield break;
    }
}
