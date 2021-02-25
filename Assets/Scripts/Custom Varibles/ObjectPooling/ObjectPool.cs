using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    //Using the active(disabling when not in use) method of object pooling
    public GameObject m_prefab = null;
    public int m_objectCount = 10;

    private Queue<PoolObject> m_objectQueue = new Queue<PoolObject>();

    /// <summary>
    /// Initlise the object pool
    /// </summary>
    /// <returns>true when successfully intilalised</returns>
    public bool Init()
    {
        m_objectQueue = new Queue<PoolObject>();

        if (m_prefab == null)
        {
            return false;
        }

        GameObject parentObject = new GameObject();
        parentObject.name = "Object Pool: " + m_prefab.name;

        //Spawn all objects
        List<GameObject> objects = new List<GameObject>();

        for (int objectIndex = 0; objectIndex < m_objectCount; objectIndex++)
        {
            objects.Add(Instantiate(m_prefab, parentObject.transform));

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
    /// In the case the object pool is full of entities setup them all up as such
    /// </summary>
    public void SetupAsEntities()
    {
        if (m_prefab == null || m_objectQueue.Count == 0) //Data verification
        {
            return;
        }

        List<Entity> poolEntities = new List<Entity>();

        for (int i = 0; i < m_objectCount; i++)
        {
            PoolObject poolObject = m_objectQueue.Dequeue();
            Entity entity = poolObject.GetComponent<Entity>();

            if(entity != null)
            {
                entity.InitEntity();

                poolEntities.Add(entity);
            }

            m_objectQueue.Enqueue(poolObject);
        }

        ((SceneController_InGame)MasterController.Instance.m_currentSceneController).AddEntities(poolEntities);
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

    public bool HasSpareObject()
    {
        return m_objectQueue.Count > 0;
    }
}
