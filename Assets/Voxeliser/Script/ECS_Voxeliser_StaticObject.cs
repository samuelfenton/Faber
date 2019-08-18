using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ECS_Voxeliser_StaticObject : ECS_Voxeliser
{
    protected override IEnumerator VoxeliserUpdate()
    {
        yield return null;

        UpdateMesh();
        ConvertToVoxels();

        //delay to allow late update to occur
        yield return null;
        Destroy(gameObject);
    }
}
