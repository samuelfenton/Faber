using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PoolObject_HitMarker : PoolObject
{
    private const float HITMARKER_LIFETIME = 10.0f;

    private const float HITMARKER_GRAVITY = -5.0f; //x,z compont of velocity 
    private const float Y_VELOCITY = 5.0f; //x,z compont of velocity 
    private const float RANDOM_VELOCITY_OFFSET = 0.3f; //x,z compont of velocity 

    public TextMeshProUGUI m_text = null;
    
    private float m_lifeTimer = 0.0f;

    private Vector3 m_velocity = Vector3.zero;

    /// <summary>
    /// Initiliase this object pool object
    /// </summary>
    /// <param name="p_objectPool">Parent object pool</param>
    public override void Init(ObjectPool p_objectPool)
    {
        base.Init(p_objectPool);
    }

    /// <summary>
    /// Rent out/move object into use this object from the pool
    /// Called once, when first put inot use
    /// </summary>
    /// <param name="p_position">Position to spawn</param>
    /// <param name="p_rotation">Rotation to spwan at</param>
    public override void Rent(Vector3 p_position, Quaternion p_rotation)
    {
        base.Rent(p_position, p_rotation);

        m_velocity = new Vector3(Random.Range(-RANDOM_VELOCITY_OFFSET, RANDOM_VELOCITY_OFFSET), Y_VELOCITY, Random.Range(-RANDOM_VELOCITY_OFFSET, RANDOM_VELOCITY_OFFSET));
        m_lifeTimer = 0.0f;

        StartCoroutine(ApplyPhysics());
    }

    /// <summary>
    /// Setup required vairbles, 
    /// </summary>
    /// <param name="p_damageCount">Text for hit marker</param>
    public void SetHitMarkerVal(int p_damageCount)
    {
        m_text.text = p_damageCount.ToString();
    }

    /// <summary>
    /// Apply physics to hit marker
    /// </summary>
    /// <returns>Wait till next frame</returns>
    private IEnumerator ApplyPhysics()
    {
        yield return null;

        //Internal clock
        m_lifeTimer += Time.deltaTime;

        //Physics
        m_velocity.y += HITMARKER_GRAVITY * Time.deltaTime;

        Vector3 pos = transform.position;
        pos += m_velocity * Time.deltaTime;
        transform.position = pos;

        //Clock logic
        if (m_lifeTimer >= HITMARKER_LIFETIME)
            Return();
        else
            StartCoroutine(ApplyPhysics());
    }
}
