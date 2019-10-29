using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Character_SplinePhysics : SplinePhysics
{
    private Character m_parentCharacter = null;

    /// <summary>
    /// Initilise entity physics
    /// Setup collider extents for future use
    /// </summary>
    protected override void Start()
    {
        base.Start();

        m_parentCharacter = GetComponent<Character>();

        CapsuleCollider capculeCollider = GetComponent<CapsuleCollider>();

        m_colliderExtents.x = m_colliderExtents.z = capculeCollider.radius;
        m_colliderExtents.y = capculeCollider.height / 2.0f;
    }
}
