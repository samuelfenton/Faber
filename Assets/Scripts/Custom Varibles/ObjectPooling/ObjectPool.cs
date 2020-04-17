﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    //Using the active(disabling when not in use) method of object pooling
    
    private Queue<PoolObject> m_objectQueue;

    /// <summary>
    /// Initlise the object pool
    /// </summary>
    /// <param name="p_prefab">Prefab to object pool, requres the PoolObject component attached</param>
    /// <param name="p_count">How many prefabs to include</param>
    /// <returns>true when successfully intilalised</returns>
    public bool Init(GameObject p_prefab, int p_count)
    {
        m_objectQueue = new Queue<PoolObject>();

        if (p_prefab == null)
        {
            return false;
        }

        List<GameObject> objects = new List<GameObject>();

        for (int objectIndex = 0; objectIndex < p_count; objectIndex++)
        {
            objects.Add(Instantiate(p_prefab));

            PoolObject newScript = objects[objectIndex].GetComponentInChildren<PoolObject>();

            if (newScript == null)
            {
#if UNITY_EDITOR
                Debug.Log("Assigned prefab for " + name + " does not contain the requried scripts");
#endif
                return false;
            }

            newScript.Init(this);
            m_objectQueue.Enqueue(newScript);

            objects[objectIndex].SetActive(false);
        }
        return true;
    }

    /// <summary>
    /// Rent or get a object from the pool
    /// </summary>
    /// <returns>Object reference or null when not possible</returns>
    /// <param name="p_position">Position to spawn</param>
    /// <param name="p_rotation">Rotation to spwan at</param> 
    public PoolObject RentObject(Vector3 p_position, Quaternion p_rotation)
    {
        if(m_objectQueue.Count > 0)
        {
            PoolObject rentedObject = m_objectQueue.Dequeue();
            rentedObject.gameObject.SetActive(true);

            rentedObject.Rent(p_position, p_rotation);

            return rentedObject;
        }
        return null;  
    }

    /// <summary>
    /// Return an object back to the queue
    /// </summary>
    /// <param name="p_object">Obejct to return</param>
    public void ReturnObject(PoolObject p_object)
    {
        if (p_object != null && !m_objectQueue.Contains(p_object))
        {
            m_objectQueue.Enqueue(p_object);
            p_object.gameObject.SetActive(false);
        }
    }
}
