using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Character_SplinePhysics : SplinePhysics
{
    /// <summary>
    /// Initilise entity physics
    /// Setup collider extents for future use
    /// </summary>
    public override void Init()
    {
        base.Init();

        Collider collider = GetComponent<Collider>();

        m_colliderExtents = collider.bounds.extents;
    }
}
