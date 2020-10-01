using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject_DamageEffect : PoolObject
{
    private const float PARTICLE_LIFETIME = 4.0f;

    private ParticleSystem m_particleSystem = null;

    /// <summary>
    /// Initiliase this object pool object
    /// </summary>
    /// <param name="p_objectPool">Parent object pool</param>
    public override void Init(ObjectPool p_objectPool)
    {
        base.Init(p_objectPool);

        m_particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    /// <summary>
    /// Assigns color to the particle effect
    /// </summary>
    /// <param name="p_color">Color to use</param>
    public void SetupDamageEffect(Color p_color)
    {
        SetupDamageEffect(p_color, p_color);
    }

    /// <summary>
    /// Assigns colors to the particle effect
    /// Will be using random between two
    /// </summary>
    /// <param name="p_randomColor1">First random color to use</param>
    /// <param name="p_randomColor2">Second random color to use</param>
    public void SetupDamageEffect(Color p_randomColor1, Color p_randomColor2)
    {
        ParticleSystem.MainModule module = m_particleSystem.main;

        ParticleSystem.MinMaxGradient colorGradient = new ParticleSystem.MinMaxGradient(p_randomColor1, p_randomColor2);
        colorGradient.mode = ParticleSystemGradientMode.TwoColors;

        //Reassign varibles
        module.startColor = colorGradient;
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
