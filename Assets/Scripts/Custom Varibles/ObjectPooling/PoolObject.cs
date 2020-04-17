using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    ObjectPool m_objectPool = null; 

    /// <summary>
    /// Initiliase this object pool object
    /// </summary>
    /// <param name="p_objectPool">Parent object pool</param>
    public virtual void Init(ObjectPool p_objectPool)
    {
        m_objectPool = p_objectPool;
    }

    /// <summary>
    /// Rent out/move object into use this object from the pool
    /// Called once, when first put inot use
    /// </summary>
    /// <param name="p_position">Position to spawn</param>
    /// <param name="p_rotation">Rotation to spwan at</param>
    public virtual void Rent(Vector3 p_position, Quaternion p_rotation)
    {
        transform.position = p_position;
        transform.rotation = p_rotation;
    }

    /// <summary>
    /// Return/add to queue this object to the pool
    /// Called once, when returning back to pool
    /// </summary>
    public virtual void Return()
    {
        m_objectPool.ReturnObject(this);
    }
}
