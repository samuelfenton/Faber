using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Aerial : Character_NPC
{
    [Header("Aerial Stats")]
    public GameObject m_hoveringModel = null;
    [Tooltip("How high off the ground will the drone hover?")]
    public float m_hoverHeight = 2.0f;

    [Header("Assigned GameObjects")]
    public GameObject m_weaponObject = null;

    [Header("Projectile")]
    public ObjectPool m_objectPoolLightProjectile = null;
    public ObjectPool m_objectPoolHeavyProjectile = null;
    public GameObject m_projectileSpawnAnchor = null;

    [Header("Firing Count")]
    public int m_lightProjectileCount = 3;
    public int m_heavyProjectileCount = 1;

    [Header("Firing Statistics")]
    public float m_maxFiringAngle = 40.0f;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();

        m_splinePhysics.m_gravity = false;

        if(m_hoveringModel != null)
        {
            Vector3 localPosition = m_hoveringModel.transform.localPosition;
            localPosition.y = m_hoverHeight;
            m_hoveringModel.transform.localPosition = localPosition;
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        else
        {
            Debug.LogError(name + " aerial character has no assigned hovering model");
        }
#endif

        BoxCollider collider = GetComponent<BoxCollider>();
        Vector3 currentCenter = collider.center;
        currentCenter.y = m_hoverHeight;
        collider.center = currentCenter;
    }
}
