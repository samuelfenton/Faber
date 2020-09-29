using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager_InGame: MonoBehaviour
{
    public GameObject m_hitMarkerPrefab = null;
    public int m_hitmarkerCount = 0;

    private ObjectPool m_hitMarkerPool = null;

    private void Start()
    {
        m_hitMarkerPool = gameObject.AddComponent<ObjectPool>();
        m_hitMarkerPool.Init(m_hitMarkerPrefab, m_hitmarkerCount);
    }

    /// <summary>
    /// Spawn a hit marker at a given location
    /// </summary>
    /// <param name="p_position">Position to spawn</param>
    /// <param name="p_rotation">Rotation to spawn</param>
    /// <param name="p_hitmarkerVal">Text value of hit marker</param>
    public void SpawnHitMarker(Vector3 p_position, Quaternion p_rotation, int p_hitmarkerVal)
    {
        PoolObject poolObject = m_hitMarkerPool.RentObject(p_position, p_rotation);

        if(poolObject!=null)
        {
            //Get derived hitmarker class
            PoolObject_HitMarker hitMarker = poolObject.GetComponent<PoolObject_HitMarker>();

            if(hitMarker != null)
                hitMarker.SetHitMarkerVal(p_hitmarkerVal);
        }
    }
}
