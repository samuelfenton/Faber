using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Character_SplinePhysics : SplinePhysics
{
    /// <summary>
    /// Initilise entity physics
    /// Setup collider extents for future use
    /// </summary>
    public override void Init()
    {
        base.Init();

        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

        m_colliderExtents.x = m_colliderExtents.z = capsuleCollider.radius;
        m_colliderExtents.y = capsuleCollider.height / 2.0f;
    }
}
