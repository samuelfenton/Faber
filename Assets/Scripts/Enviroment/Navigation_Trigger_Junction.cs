﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation_Trigger_Junction : Navigation_Trigger
{
    public SplineInfo m_backwardSplineInfo = new SplineInfo();
    public SplineInfo m_forwardSplineInfo = new SplineInfo();
    public SplineInfo m_forwardRightSplineInfo = new SplineInfo();
    public SplineInfo m_forwardLeftSplineInfo = new SplineInfo();

    protected override void Start()
    {
#if UNITY_EDITOR
        if ((m_forwardSplineInfo.m_spline == null && m_forwardRightSplineInfo.m_spline == null && m_forwardLeftSplineInfo.m_spline == null) || m_backwardSplineInfo.m_spline == null)
        {
            Debug.Log("Junction trigger is missing exit spline or at least one entering spline");
        }
#endif
        base.Start();
    }

    protected override void HandleTrigger(Character p_character, TRIGGER_DIRECTION p_direction)
    {
        if (p_direction == TRIGGER_DIRECTION.ENTERING)
        {
            SplineInfo selectedSpline = GetEnteringSpline(p_character);
            SwapSplines(p_character, selectedSpline.m_spline, selectedSpline.m_splinePercent);
        }
        else
        {
            SwapSplines(p_character, m_backwardSplineInfo.m_spline, m_backwardSplineInfo.m_splinePercent);
        }
    }

    private SplineInfo GetEnteringSpline(Character p_character)
    {
        NavigationController.TURNING turningDir = p_character.GetDesiredTurning();

        //Get desired turning dir
        switch (turningDir)
        {
            case NavigationController.TURNING.CENTER:
                if (m_forwardSplineInfo.m_spline != null)
                    return m_forwardSplineInfo;
                break;
            case NavigationController.TURNING.RIGHT:
                if (m_forwardRightSplineInfo.m_spline != null)
                    return m_forwardRightSplineInfo;
                break;
            case NavigationController.TURNING.LEFT:
                if (m_forwardLeftSplineInfo.m_spline != null)
                    return m_forwardLeftSplineInfo;
                break;
            default:
                break;
        }

        //Grab any that isnt null, TODO could probably make this better, maybe place the alternates in the switch itself. Not very pretty but hey.
        if (m_forwardSplineInfo.m_spline != null)
            return m_forwardSplineInfo;
        if (m_forwardRightSplineInfo.m_spline != null)
            return m_forwardRightSplineInfo;
        return m_forwardLeftSplineInfo;
    }
}