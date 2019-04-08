using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxeliserCompanion_Child : VoxeliserCompanion
{

    protected override void Start()
    {
        base.Start();

        //Set update frames to the same
        Transform parent = transform.parent;
        VoxeliserCompanion parentVoxeliser = null;

        while (parent != null) //Stop searching when there are no more parents
        {
            parentVoxeliser = parent.GetComponent<VoxeliserCompanion>();
            if (parentVoxeliser != null) //Found a parent voxeliser
            {
                UPDATE_N_FRAMES = parentVoxeliser.UPDATE_N_FRAMES;
                break;
            }
            parent = parent.parent;
        }
    }
    //--------------------
    //  Rather than using Update Voxeliser is Enumerator driven.
    //  Run logic every run every second frame
    //--------------------
    protected override IEnumerator VoxeliserUpdate()
    {
        UpdateMesh();
        StartCoroutine(ConvertToVoxels());
        yield return null;
    }
}
