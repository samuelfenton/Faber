using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject_DamageEffect : PoolObject
{
    private const float PARTICLE_LIFETIME = 4.0f;

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

        StartCoroutine(ParticleLifeTime());
    }

    private IEnumerator ParticleLifeTime()
    {
        yield return new WaitForSeconds(PARTICLE_LIFETIME);

        Return();

    }
}
