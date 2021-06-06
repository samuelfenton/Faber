using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Aerial : Character_NPC
{
    [Header("Aerial Stats")]
    public GameObject m_hoveringModel = null;
    [Tooltip("How high off the ground will the drone hover?")]
    public float m_hoverHeight = 2.0f;
    [Tooltip("How far the drone will bob up/down?")]
    public float m_hoverDistance = 0.2f;
    [Tooltip("How much the drone will bob up/down?")]
    public float m_hoverErraticness = 1.0f;

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

    /// <summary>
    /// Update an entity, this should be called from scene controller
    /// Used to handle different scene state, pause vs in game etc
    /// </summary>
    public override void UpdateEntity()
    {
        base.UpdateEntity();

        //Hovering bob up and down
        if (m_hoveringModel != null)
        {
            Vector3 localPosition = m_hoveringModel.transform.localPosition;
            localPosition.y = m_hoverHeight + Mathf.Sin(Time.time * m_hoverErraticness) * m_hoverDistance;
            m_hoveringModel.transform.localPosition = localPosition;
        }
    }
}
