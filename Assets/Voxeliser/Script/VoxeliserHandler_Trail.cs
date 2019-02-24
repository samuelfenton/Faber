using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxeliserHandler_Trail : VoxeliserHandler
{
    [Header("Trailing Details")]
    [Tooltip("Chance of trailing voxel appearing when removed out of 100")]
    public float m_chanceOfTrailing = 0.5f;

    [Tooltip("How long voxels will trail away for")]
    public float m_trailingShrinkTime = 1;

    [Tooltip("What velocity will voxels have")]
    public float m_trailingSpeed = 1;
    public enum TRAILING_TYPE { BACKWARDS, FIXED, FIXED_LOCAL};

    [Tooltip("Type of trailing effect, Backwards = move behind object, fixed = move in fixed direction globaly, fixed local = moves in fixed direction relative to object")]
    public TRAILING_TYPE m_trailingType = TRAILING_TYPE.BACKWARDS;

    [Tooltip("When using fixed direction set using this varible")]
    public Vector3 m_trailDirection = new Vector3(0, 0, 0);

    //--------------------
    //  Initialise all voxels used
    //      As trailing, also init trailing varibles
    //      Trailing mesh requires no material to avoid "Ghosting" of orginal mesh
    //  params:
    //      p_voxelCount - How many voxels to create
    //--------------------
    public override void InitVoxels(int p_voxelCount)
    {
        m_disableMaterialsOnRun = true;
        base.InitVoxels(p_voxelCount);

        Voxel[] voxels = m_voxelsAvailable.ToArray();

        for (int i = 0; i < p_voxelCount; i++)
        {
            Voxel_Trailing voxel_Trailing = voxels[i].GetComponent<Voxel_Trailing>();
            if(voxel_Trailing != null)
                voxel_Trailing.InitTrailing(m_voxelSize, m_trailingShrinkTime);
        }

        m_trailDirection = m_trailDirection.normalized;
    }

    //--------------------
    //  Remove voxel, remove from inUse
    //  Voxels may trail, in whih case setup to do so
    //  Otherwise reset voxel, add to avalible 
    //  params:
    //      p_position - Positions of voxel, used in getting voxel from dictionary
    //--------------------
    public override void RemoveVoxel(VectorDouble p_position)
    {
        Voxel removedVoxel = null;

        //Decided trailing effect
        Vector3 trailingDir = GetTrailDirection().normalized;
        float chanceOfTrailing = GetChanceOfTrailing();

        if (m_voxelsInUse.TryGetValue(p_position, out removedVoxel))
        {
            if (Random.Range(0f, 100f) < chanceOfTrailing)
            {
                ((Voxel_Trailing)removedVoxel).ApplyTrail(trailingDir, m_trailingSpeed);
            }
            else
                FreedVoxel(removedVoxel);
            m_voxelsInUse.Remove(p_position);
        }
    }

    //--------------------
    //  Voxel has finished trailing, hence free and add to available voxels
    //  params:
    //      p_voxel - Voxel to add to available voxels
    //--------------------
    public void FreedVoxel(Voxel p_voxel)
    {
        m_voxelsAvailable.Enqueue(p_voxel);
        p_voxel.ResetVoxel();

    }

    //--------------------
    //  Get chance of voxel trailing
    //  return:
    //      float - Varible m_chanceOfTrailing
    //--------------------
    public virtual float GetChanceOfTrailing()
    {
        return m_chanceOfTrailing;
    }

    //--------------------
    //  Get trail direction
    //  return:
    //      Vector3 - Direction for voxel to travel
    //--------------------
    public virtual Vector3 GetTrailDirection()
    {
        Vector3 trailingDir = Vector3.zero;

        switch (m_trailingType)
        {
            case TRAILING_TYPE.BACKWARDS:
                trailingDir = -transform.forward;
                break;
            case TRAILING_TYPE.FIXED:
                trailingDir = m_trailDirection;
                break;
            case TRAILING_TYPE.FIXED_LOCAL:
                trailingDir = transform.localToWorldMatrix * m_trailDirection;
                break;
        }
        return trailingDir;
    }
}
