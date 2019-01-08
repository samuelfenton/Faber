using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation_Trigger_One2One : Navigation_Trigger
{
    public SplineInfo m_forwardSplineInfo = new SplineInfo();
    public SplineInfo m_backwardSplineInfo = new SplineInfo();

    protected override void Start()
    {
#if UNITY_EDITOR
        if(m_forwardSplineInfo.m_spline == null || m_backwardSplineInfo.m_spline == null)
        {
            Debug.Log("One to One spline trigger has no attached splines");
        }
        else //Connected splines exist, so set up percentages of connected splines for when exiting
        {
            if (m_backwardSplineInfo.m_spline != null)
                m_backwardSplineInfo.m_splinePercent = m_backwardSplineInfo.m_spline.GetPositionOfSplineTransform(transform);

            if (m_forwardSplineInfo.m_spline != null)
                m_forwardSplineInfo.m_splinePercent = m_forwardSplineInfo.m_spline.GetPositionOfSplineTransform(transform);
        }
#endif
        base.Start();
    }

    protected override void HandleTrigger(Character p_character, TRIGGER_DIRECTION p_direction)
    {
        if (p_direction == TRIGGER_DIRECTION.ENTERING)
        {
            SwapSplines(p_character, m_forwardSplineInfo.m_spline, m_forwardSplineInfo.m_splinePercent);
        }
        else
        {
            SwapSplines(p_character, m_backwardSplineInfo.m_spline, m_backwardSplineInfo.m_splinePercent);
        }
    }
}
