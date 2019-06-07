using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Navigation_SplineBuilder : MonoBehaviour
{
    public GameObject m_one2OneTriggerPrefab = null;
    public GameObject m_junctionTriggerPrefab = null;
    public GameObject m_strightSplinePrefab = null;
    public GameObject m_curvedSplinePrefab = null;

    public enum TRIGGER_TYPE {ONE_2_ONE, JUNCTION };
    [SerializeField]
    public TRIGGER_TYPE m_triggerType = TRIGGER_TYPE.ONE_2_ONE;
    private TRIGGER_TYPE m_previousTriggerType = TRIGGER_TYPE.ONE_2_ONE;

    [SerializeField]
    private Navigation_Trigger m_navigationTrigger = null;

    public enum SPAWN_DIR {FORWARD, BACKWARD };
    [SerializeField]
    public SPAWN_DIR m_spawnDir = SPAWN_DIR.FORWARD;

    [Header("One to One Settings")]
    [Tooltip("Based off local")]
    public Vector3 m_one2OneSplineDir = Vector3.zero;

    public enum JUNCTION_TYPE {T_SECTION, X_SECTION };
    [Header("Junction Settings")]
    [SerializeField]
    public JUNCTION_TYPE m_junctionType = JUNCTION_TYPE.T_SECTION;
    public enum T_SECTION_START { LEFT_T, CENTER_T, RIGHT_T };
    public T_SECTION_START m_tSectionStart = T_SECTION_START.CENTER_T;
    public float m_junctionRadius = 2.0f;

    [Header("Spawning")]
    public bool m_spawn = false;

    private void Start()
    {
        m_navigationTrigger = GetComponent<Navigation_Trigger>();
        if(m_navigationTrigger!= null)//Started with trigger attached
        {
            if(GetComponent<Navigation_Trigger_One2One>() != null)
            {
                m_triggerType = TRIGGER_TYPE.ONE_2_ONE;
                m_previousTriggerType = TRIGGER_TYPE.ONE_2_ONE;
            }
            else
            {
                m_triggerType = TRIGGER_TYPE.JUNCTION;
                m_previousTriggerType = TRIGGER_TYPE.JUNCTION;
            }
        }
    }

    private void Update()
    {
        if(m_triggerType != m_previousTriggerType)//Changed type
        {
            m_previousTriggerType = m_triggerType;

            DestroyImmediate(m_navigationTrigger);

            if (m_triggerType == TRIGGER_TYPE.ONE_2_ONE)
                m_navigationTrigger = gameObject.AddComponent<Navigation_Trigger_One2One>();
            else
                m_navigationTrigger = gameObject.AddComponent<Navigation_Trigger_Junction>();
        }

        if(m_spawn)//Spawning of section
        {
            m_spawn = false;

            Navigation_Trigger_One2One one2One = GetComponent<Navigation_Trigger_One2One>();
            Navigation_Trigger_Junction junction = GetComponent<Navigation_Trigger_Junction>();

            //Ensure we can actually add
            if (m_triggerType == TRIGGER_TYPE.ONE_2_ONE && one2One !=null)
            {
                if (m_spawnDir == SPAWN_DIR.FORWARD && one2One.m_forwardSplineInfo.m_spline == null)
                {
                    Vector3 spawnDir = transform.worldToLocalMatrix * m_one2OneSplineDir;

                    //Spawning
                    GameObject nextTrigger = Instantiate(m_one2OneTriggerPrefab, transform.position + spawnDir, transform.rotation);
                    Navigation_Trigger_One2One nextTriggerScript = nextTrigger.GetComponent<Navigation_Trigger_One2One>();
                    GameObject spline = Instantiate(m_strightSplinePrefab, transform.position + spawnDir * 0.5f, transform.rotation);
                    Navigation_Spline_Line splineScript = spline.GetComponent<Navigation_Spline_Line>();

                    nextTriggerScript.m_backwardSplineInfo.m_spline = splineScript;

                    splineScript.m_splineStart = one2One;
                    splineScript.m_splineEnd = nextTriggerScript;

                    return;
                }
                if (m_spawnDir == SPAWN_DIR.BACKWARD && one2One.m_backwardSplineInfo.m_spline == null)


                    return;
            }
            else if(m_triggerType == TRIGGER_TYPE.JUNCTION && junction != null)
            {
                if (m_spawnDir == SPAWN_DIR.FORWARD && junction.m_forwardSplineInfo.m_spline == null && junction.m_forwardLeftSplineInfo.m_spline == null && junction.m_forwardRightSplineInfo.m_spline == null)
                    return;
                if (m_spawnDir == SPAWN_DIR.BACKWARD && junction.m_backwardSplineInfo.m_spline == null)
                    return;
            }
        }
    }
}
