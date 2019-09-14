using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation_Spline : MonoBehaviour
{
    public Navigation_Trigger m_splineStart = null;
    public Navigation_Trigger m_splineEnd = null;

    [HideInInspector]
    public float m_splineLength = 1.0f;

    public virtual void Start()
    {
        if(m_splineStart == null || m_splineEnd == null)
        {
            gameObject.SetActive(false);
#if UNITY_EDITOR
            Debug.Log(name + " has no start or end spline assinged");
#endif
            return;
        }
    }

    public virtual Vector3 GetSplinePosition(float p_splinePercent)
    {
        return Vector3.zero;
    }

    public virtual Vector3 GetForwardsDir(Vector3 p_splinePosition)
    {
        return Vector3.zero;
    }

    public float GetSplinePercent(float p_movement)
    {
        return p_movement / m_splineLength;
    }

    public float GetPositionOfSplineTransform(Navigation_Trigger p_spline)
    {
        if (p_spline == m_splineStart)
            return 0;
        return 1;
    }
}
