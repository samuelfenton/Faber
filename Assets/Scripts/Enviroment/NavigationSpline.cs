using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationSpline : MonoBehaviour
{
    public Transform m_splineStart = null;
    public Transform m_splineEnd = null;
    public float m_splineLength = 1.0f;

    public float GetSplinePercent(float p_movement)
    {
        return p_movement / m_splineLength;
    }

    public Vector3 GetSplinePosition(float p_splinePercent)
    {
        return Vector3.zero;
    }
}
