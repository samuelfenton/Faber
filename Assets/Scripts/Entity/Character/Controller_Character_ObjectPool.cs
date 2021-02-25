using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Character_ObjectPool : MonoBehaviour
{
    [Header("General ObjectPools")]
    public ObjectPool m_hitmarker = null;

    [Header("Damage Object Pools")]
    public ObjectPool m_forwardDamage = null;
    public ObjectPool m_rightToLeftDamage = null;
    public ObjectPool m_leftToRightDamage = null;
    public ObjectPool m_upwardsDamage = null;
    public ObjectPool m_downwardsDamage = null;

    public void InitCharacterObjectPools()
    {
        //Object pooling
        if (m_hitmarker != null)
            m_hitmarker.Init();

        if (m_forwardDamage != null)
            m_forwardDamage.Init();
        if (m_rightToLeftDamage != null)
            m_rightToLeftDamage.Init();
        if (m_leftToRightDamage != null)
            m_leftToRightDamage.Init();
        if (m_upwardsDamage != null)
            m_upwardsDamage.Init();
        if (m_downwardsDamage != null)
            m_downwardsDamage.Init();
    }

    /// <summary>
    /// Spawn a hit marker at a given location
    /// </summary>
    /// <param name="p_position">Position to spawn</param>
    /// <param name="p_rotation">Rotation to spawn</param>
    /// <param name="p_hitmarkerVal">Text value of hit marker</param>
    public void SpawnHitMarker(Vector3 p_position, Quaternion p_rotation, int p_hitmarkerVal)
    {
        PoolObject poolObject = m_hitmarker.RentObject(p_position, p_rotation);

        if (poolObject != null)
        {
            //Get derived hitmarker class
            PoolObject_HitMarker hitMarker = poolObject.GetComponent<PoolObject_HitMarker>();

            if (hitMarker != null)
                hitMarker.SetHitMarkerVal(p_hitmarkerVal);
        }
    }

    /// <summary>
    /// Spawn a hit damage effect at a given location
    /// </summary>
    /// <param name="p_position">Position to spawn</param>
    /// <param name="p_rotation">Rotation to spawn</param>
    /// <param name="p_hitmarkerVal">Text value of hit marker</param>
    /// <param name="p_effectColor1">first color to use in particle system</param>
    /// <param name="p_effectColor1">second color to use in particle system</param>
    public void SpawnDamageParticles(Vector3 p_position, Quaternion p_rotation, Manoeuvre.DAMAGE_DIRECTION p_direction, Color p_effectColor1, Color p_effectColor2)
    {
        PoolObject poolObject = null;

        switch (p_direction)
        {
            case Manoeuvre.DAMAGE_DIRECTION.FORWARDS:
                poolObject = m_forwardDamage.RentObject(p_position, p_rotation);
                break;
            case Manoeuvre.DAMAGE_DIRECTION.HORIZONTAL_RIGHT:
                poolObject = m_rightToLeftDamage.RentObject(p_position, p_rotation);
                break;
            case Manoeuvre.DAMAGE_DIRECTION.HORIZONTAL_LEFT:
                poolObject = m_leftToRightDamage.RentObject(p_position, p_rotation);
                break;
            case Manoeuvre.DAMAGE_DIRECTION.VERTICAL_UPWARDS:
                poolObject = m_upwardsDamage.RentObject(p_position, p_rotation);
                break;
            case Manoeuvre.DAMAGE_DIRECTION.VERTICAL_DOWNWARDS:
                poolObject = m_downwardsDamage.RentObject(p_position, p_rotation);
                break;
            default:
                break;
        }

        if (poolObject != null)
        {
            PoolObject_DamageEffect damageEffect = poolObject.GetComponentInChildren<PoolObject_DamageEffect>();

            if (damageEffect != null)
                damageEffect.SetupDamageEffect(p_effectColor1, p_effectColor2);
        }
    }
}
