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

#if UNITY_EDITOR || DEVELOPMENT_BUILD

    private void OnDrawGizmosSelected()
    {
        if(!Application.isPlaying)
        {
            if (MOARDebugging.GetSplinePosition(m_nodeA, m_nodeB, m_currentSplinePercent, out Vector3 position))
            {
                transform.position = position;
            }
        }
    }
#endif
}
