using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxeliserHandler : MonoBehaviour
{
    [Header("Voxel Details")]
    [Tooltip("Voxel prefab used, this object should contain a Voxel script or derived voxel script")]
    public GameObject m_voxel = null;

    [Tooltip("Size of voxel relative to the base voxel, e.g Voxel of length 2, with VoxelSize set to 0.5 will be a total length of 1")]
    public float m_voxelSize = 1;

    [Header("Explosion Details")]
    [Tooltip("Chance of voxel exploding is determined by this value. Chance is value out of 100")]
    public float m_chanceOfExplode = 5;

    [Tooltip("How powerful is the explosive effect")]
    public float m_explosionFactor = 5.0f;

    [Tooltip("Time required for voxel to shrink")]
    public float m_shrinkTime = 2.0f;

    protected Queue<Voxel> m_voxelsAvailable = new Queue<Voxel>();
    protected Dictionary<VectorDouble, Voxel> m_voxelsInUse = new Dictionary<VectorDouble, Voxel>();
    protected HashSet<VectorDouble> m_voxelPositions = new HashSet<VectorDouble>();

    protected bool m_disableMaterialsOnRun = true;
    protected VoxeliserCompanion m_voxelCompanion = null;

    
    public bool m_disassemble = false;

    //--------------------
    //  Late update
    //  Runs at same time as transform updates to give same illusion as parenting voxels
    //--------------------
    private void LateUpdate()
    {
        foreach (Voxel voxel in m_voxelsInUse.Values)
        {
            voxel.UpdatePositionToParent(transform.position);
        }
    }

    //--------------------
    //  Snap to postion grid
    //  params:
    //      p_position - World position
    //      p_gridSize - size between grid lines
    //  return:
    //      Vector3 - Postioned "Snapped" to a world grid with increments of p_gridSize 
    //--------------------
    Vector3 SnapToGrid(Vector3 p_position, float p_gridSize)
    {
        p_position.x -= p_position.x < 0 ? p_position.x % -p_gridSize : p_position.x % p_gridSize;
        p_position.y -= p_position.y < 0 ? p_position.y % -p_gridSize : p_position.y % p_gridSize;
        p_position.z -= p_position.z < 0 ? p_position.z % -p_gridSize : p_position.z % p_gridSize;

        return p_position;
    }

    //--------------------
    //  Initialise all voxels used
    //  params:
    //      p_voxelCount - How many voxels to create
    //--------------------
    public virtual void InitVoxels(int p_voxelCount)
    {
        GameObject voxelPool = new GameObject();
        voxelPool.name = gameObject.name + " Voxel Pool";

        for (int i = 0; i < p_voxelCount; i++)
        {
            GameObject newVoxel = Instantiate(m_voxel, voxelPool.transform);
            Voxel newVoxelScript = newVoxel.GetComponent<Voxel>();

            newVoxelScript.InitVoxel(m_voxelSize, m_shrinkTime, m_explosionFactor, this);

            m_voxelsAvailable.Enqueue(newVoxelScript);
        }
        m_voxelCompanion = GetComponent<VoxeliserCompanion>();

        if (m_disableMaterialsOnRun)
            DisableMaterial();
    }

    //--------------------
    //  Add/Remove voxels based off latest update of voxel positions
    //      Determine what voxels need to be added/removed, No need to add ones that already exist
    //      Wait
    //      Run through and add/remove
    //  params:
    //      p_voxelPositions - Positions of all voxels for this frame
    //      p_colors - Colors of all voxels for this frame
    //--------------------
    public virtual IEnumerator HandleVoxels(HashSet<VectorDouble> p_voxelPositions, List<Vector3> p_colors)
    {
        if (m_disassemble)
        {
            DisassembleVoxels();
            yield break; //Early breakout
        }

        Queue<VectorDouble> voxelsToRemove = new Queue<VectorDouble>();
        Queue<VectorDouble> voxelsToAdd = new Queue<VectorDouble>();

        Queue<Vector3> addedVoxelColors = new Queue<Vector3>();

        //Remove old voxels not in use
        foreach (VectorDouble position in m_voxelPositions)
        {
            if (!p_voxelPositions.Contains(position))//Old voxel no longer in use
            {
                voxelsToRemove.Enqueue(position);
            }
        }

        //Add new voxels
        int colorIndex = 0;

        foreach (VectorDouble position in p_voxelPositions)
        {
            if (!m_voxelPositions.Contains(position))//Voxel already exists, dont do anything
            {
                voxelsToAdd.Enqueue(position);
                addedVoxelColors.Enqueue(p_colors[colorIndex]);
            }
            colorIndex++;
        }

        //Wait till next frame
        yield return null; 

        //Remove voxels
        while (voxelsToRemove.Count > 0)
        {
            RemoveVoxel(voxelsToRemove.Dequeue());
        }

        //Add voxels
        while (voxelsToAdd.Count > 0)
        {
            AddVoxel(voxelsToAdd.Dequeue(), addedVoxelColors.Dequeue());
        }

        m_voxelPositions = p_voxelPositions;
    }

    //--------------------
    //  Add voxel 
    //      Remove from available, add to inUse
    //      Set color
    //  params:
    //      p_position - Positions of voxel, used in getting voxel from dictionary
    //      p_color -  color of voxel, in Vector3 format(x = r, y = g, z = b)
    //--------------------
    protected virtual void AddVoxel(VectorDouble p_position, Vector3 p_color)
    {
        if (m_voxelsAvailable.Count > 0 && !m_voxelsInUse.ContainsKey(p_position)) //voxel availble and current voxels dont already have this postion
        {
            Voxel newVoxel = m_voxelsAvailable.Dequeue();
            newVoxel.SetupVoxel(VectorDouble.GetVector3(p_position), p_color);
            m_voxelsInUse.Add(p_position, newVoxel);
        }
    }

    //--------------------
    //  Remove voxel, remove from inUse, add to available, reset voxel
    //  params:
    //      p_position - Positions of voxel, used in getting voxel from dictionary
    //--------------------
    public virtual void RemoveVoxel(VectorDouble p_position)
    {
        Voxel removedVoxel = null;
        if (m_voxelsInUse.TryGetValue(p_position, out removedVoxel))
        {
            m_voxelsAvailable.Enqueue(removedVoxel);
            removedVoxel.ResetVoxel();
            m_voxelsInUse.Remove(p_position);
        }
    }

    //--------------------
    //  Set disassemble to true
    //--------------------
    public void EnableDisassemble()
    {
        m_disassemble = true;
    }

    //--------------------
    //  Cause voxels to dissemble
    //  All inUse voxels have a chance to be exploded outwords, otherwise just destroyed
    //--------------------
    protected void DisassembleVoxels()
    {
        int dictionaryIndex = 0;

        Queue<VectorDouble> m_explodedVoxels = new Queue<VectorDouble>();
        foreach (KeyValuePair<VectorDouble, Voxel> kPair in m_voxelsInUse)
        {
            if(Random.Range(0f, 100f) < m_chanceOfExplode) //Cause explosion effect on rando voxels
            {
                kPair.Value.ApplyPhysics();
                kPair.Value.ApplyShrink();
                m_explodedVoxels.Enqueue(kPair.Key);//Dont want to be left in voxels in use, as will be deleted
            }

            dictionaryIndex++;
        }

        while (m_explodedVoxels.Count > 0)
            m_voxelsInUse.Remove(m_explodedVoxels.Dequeue());

        Destroy(GetComponent<VoxeliserCompanion>());
        Destroy(this);
    }

    //--------------------
    //  On destroy, ensure all voxels are also destroyed
    //--------------------
    protected virtual void OnDestroy()
    {
        foreach (Voxel inUseVoxel in m_voxelsInUse.Values)
        {
            if(inUseVoxel!=null)
                Destroy(inUseVoxel.gameObject);
        }

        foreach (Voxel freeVoxel in m_voxelsAvailable)
        {
            if (freeVoxel != null)
                Destroy(freeVoxel.gameObject);
        }
    }

    //--------------------
    //  Disable all materials used by mesh renderer
    //  Typically used in the cause of dynamic voxel movement, as base mesh shouldnt be shown.
    //--------------------
    protected void DisableMaterial()
    {
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.materials = new Material[0];
        }
        else
        {
            SkinnedMeshRenderer skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.materials = new Material[0];
            }
        }
    }
}
