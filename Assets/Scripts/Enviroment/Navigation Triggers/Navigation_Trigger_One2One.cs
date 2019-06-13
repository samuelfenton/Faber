using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation_Trigger_One2One : Navigation_Trigger
{
    public SplineInfo m_forwardSplineInfo = new SplineInfo();
    public SplineInfo m_backwardSplineInfo = new SplineInfo();

    protected override void Start()
    {
        base.Start();

        if(m_forwardSplineInfo.m_spline == null || m_backwardSplineInfo.m_spline == null)
        {
            //Debug message, remove collider
            #if UNITY_EDITOR
            Debug.Log("One to One spline trigger has no attached splines");
#endif

            BoxCollider boxCollider = GetComponent<BoxCollider>();
            if (boxCollider != null)
                boxCollider.enabled = false;
        }
        else //Connected splines exist, so set up percentages of connected splines for when exiting, ensure colider is enabled
        {
            if (m_backwardSplineInfo.m_spline != null)
            {
                m_backwardSplineInfo.m_splinePercent = m_backwardSplineInfo.m_spline.GetPositionOfSplineTransform(this);
                m_adjacentSplines.Add(m_backwardSplineInfo.m_spline);
            }

            if (m_forwardSplineInfo.m_spline != null)
            {
                m_forwardSplineInfo.m_splinePercent = m_forwardSplineInfo.m_spline.GetPositionOfSplineTransform(this);
                m_adjacentSplines.Add(m_forwardSplineInfo.m_spline);
            }

            BoxCollider boxCollider = GetComponent<BoxCollider>();
            if(boxCollider!=null)
                boxCollider.enabled = true;
        }
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

    public override bool ContainsSpine(Navigation_Spline p_spline)
    {
        return
            m_backwardSplineInfo.m_spline == p_spline ||
            m_forwardSplineInfo.m_spline == p_spline;
    }

    public override bool HasForwardSpline()
    {
        return m_forwardSplineInfo.m_spline !=null;
    }

    public override bool HasBackwardsSpline()
    {
        return m_backwardSplineInfo.m_spline != null;
    }
}
