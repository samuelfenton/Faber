using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel_Trailing : Voxel
{
    private float m_trailingShrinkStep = 0.1f;
    private float m_trailingSpeed = 1.0f;
    private Vector3 m_driftingDir = Vector3.zero;

    //--------------------
    //  Initialise trailing voxel varibles defined by parent voxel handler
    //  params:
    //      p_scale - Scale of voxel
    //      p_trailingTime - How long for voxels to shrink after trailing start
    //      p_trailingSpeed - Speed of a trailing voxel
    //--------------------
    public void InitTrailing(float p_scale, float p_trailingTime)
    {
        m_trailingShrinkStep = p_scale / p_trailingTime;
    }

    //--------------------
    //  Trailing Effect
    //  Applys shrinking effect and a non gravity based velocity
    //  TODO randomise movement to seem more realistic
    //--------------------
    private IEnumerator TrailingEffect()
    {
        yield return null;
        transform.Translate(m_driftingDir * m_trailingSpeed * Time.deltaTime, Space.World);
        float newScale = transform.localScale.x - m_trailingShrinkStep * Time.deltaTime;
        transform.localScale = new Vector3(newScale, newScale, newScale);
        if (newScale < 0.01f)//Finished floating away
        {
            if (m_parentVoxeliser == null)//While trailing parent was deleted
                Destroy(this.gameObject);
            else
                ((VoxeliserHandler_Trail)m_parentVoxeliser).FreedVoxel(this);
        }
        else
        {
            StartCoroutine(TrailingEffect());
        }
    }

    //--------------------
    //  Begin trailing effect
    //  params:
    //      p_driftingDir - Direction to trail
    //      p_speed - speed at which voxel trails away
    //--------------------
    public void ApplyTrail(Vector3 p_driftingDir, float p_speed)
    {
        m_driftingDir = p_driftingDir;
        m_trailingSpeed = p_speed;
        StartCoroutine(TrailingEffect());
    }
}
