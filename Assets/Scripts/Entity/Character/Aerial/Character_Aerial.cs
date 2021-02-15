using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Aerial : Character_NPC
{
    [Header("Aerial Stats")]
    [Tooltip("How high off the ground will the drone hover?")]
    public float m_hoverHeight = 2.0f;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();

        m_splinePhysics.m_gravity = false;
    }
}
