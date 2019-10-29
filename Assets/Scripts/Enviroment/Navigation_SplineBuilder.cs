using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation_SplineBuilder : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    public enum SPLINE_TYPE {ONE_2_ONE, ONE_2_ONE_CURVE, JUNCTION };

    [Header("Spawning Settings")]
    public SPLINE_TYPE m_spawnSplineType = SPLINE_TYPE.ONE_2_ONE;

    public enum SPAWN_DIR {FORWARD, BACKWARD };
    [SerializeField]
    public SPAWN_DIR m_spawnDir = SPAWN_DIR.FORWARD;

    [Header("One to One Settings")]
    [Tooltip("Based off local")]
    public Vector3 m_one2OneSplineDir = Vector3.zero;

    public enum CURVE_SETTING { ANTI_CLOCKWISE, CLOCKWISE };
    [Header("One to One Curve Settings")]
    [SerializeField]
    public CURVE_SETTING m_curveType = CURVE_SETTING.ANTI_CLOCKWISE;
    public float m_curveRadius = 2.0f;
    public float m_curveAngle = 90.0f;
    public float m_curveHeight = 0.0f;

    public enum JUNCTION_TYPE {T_SECTION, X_SECTION };
    [Header("Junction Settings")]
    [SerializeField]
    public JUNCTION_TYPE m_junctionType = JUNCTION_TYPE.T_SECTION;
    public enum T_SECTION_START { LEFT_T, CENTER_T, RIGHT_T };
    public T_SECTION_START m_tSectionStart = T_SECTION_START.CENTER_T;
    public float m_junctionRadius = 2.0f;

    [Header("Spawning")]
    public bool m_spawn = false;
    public bool m_merge = false;

    [Header("Assignedd Prefabs")]
    public GameObject m_one2OneTriggerPrefab = null;
    public GameObject m_junctionTriggerPrefab = null;
    public GameObject m_strightSplinePrefab = null;
    public GameObject m_curvedSplinePrefab = null;

    /// <summary>
    /// Changes on inspector
    /// Used to generate aditional spline throughout the level
    /// Placed on a navigation trigger
    /// </summary>
    private void OnValidate()
    {
        if(m_spawn)//Spawning of section
        {
            m_spawn = false;

            Navigation_Trigger_One2One one2One = GetComponent<Navigation_Trigger_One2One>();
            Navigation_Trigger_Junction junction = GetComponent<Navigation_Trigger_Junction>();

            //Ensure can do it
            if (m_spawnDir == SPAWN_DIR.FORWARD)
            {
                if (one2One != null && one2One.HasForwardSpline())
                    return;
                if (junction != null && junction.HasForwardSpline())
                    return;
            }
            if (m_spawnDir == SPAWN_DIR.BACKWARD)
            {
                if (one2One != null && one2One.HasBackwardsSpline())
                    return;
                if (junction != null && junction.HasBackwardsSpline())
                    return;
            }

            GameObject splineParent = GameObject.FindGameObjectWithTag("Splines");
            GameObject triggerParent = GameObject.FindGameObjectWithTag("Triggers");

            //Creation of connections
            if (m_spawnSplineType == SPLINE_TYPE.ONE_2_ONE)
            {
                if (m_one2OneSplineDir.magnitude == 0)//Valid inputs
                    return;
                //Create
                Vector3 spawnDir = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.localToWorldMatrix * m_one2OneSplineDir);

                GameObject nextTrigger = Instantiate(m_one2OneTriggerPrefab, transform.position + spawnDir, transform.rotation);
                Navigation_Trigger_One2One nextTriggerScript = nextTrigger.GetComponent<Navigation_Trigger_One2One>();
                GameObject spline = Instantiate(m_strightSplinePrefab, transform.position + spawnDir * 0.5f, transform.rotation);
                Navigation_Spline_Line splineScript = spline.GetComponent<Navigation_Spline_Line>();

                //Setup triggers/Spline varibles
                if (m_spawnDir == SPAWN_DIR.FORWARD)
                {
                    if (one2One != null)//One to one trigger
                    {
                        splineScript.m_splineStart = one2One;
                        one2One.m_forwardSplineInfo.m_spline = splineScript;
                    }
                    else //One to one trigger
                    {
                        splineScript.m_splineStart = junction;
                        junction.m_forwardSplineInfo.m_spline = splineScript;
                    }

                    nextTriggerScript.m_backwardSplineInfo.m_spline = splineScript;
                    splineScript.m_splineEnd = nextTriggerScript;
                }
                else
                {
                    if (one2One != null)//One to one trigger
                    {
                        splineScript.m_splineStart = one2One;
                        one2One.m_backwardSplineInfo.m_spline = splineScript;
                    }
                    else //One to one trigger
                    {
                        splineScript.m_splineStart = junction;
                        junction.m_backwardSplineInfo.m_spline = splineScript;
                    }

                    nextTriggerScript.m_forwardSplineInfo.m_spline = splineScript;
                    splineScript.m_splineEnd = nextTriggerScript;
                }

                //Setup Parents
                if (splineParent != null)
                {
                    spline.transform.SetParent(splineParent.transform, true);
                }
                if (triggerParent != null)
                {
                    nextTrigger.transform.SetParent(triggerParent.transform, true);
                }

                //Names
                nextTrigger.name = "TriggerOne2One";
                spline.name = "SplineLine";
            }
            else if (m_spawnSplineType == SPLINE_TYPE.ONE_2_ONE_CURVE)
            {
                if ((m_curveAngle % 360 == 0.0f && m_curveHeight != 0.0f) || m_curveRadius == 0.0f)//Valid Inputs
                    return;

                //Create
                Vector3 triggerSpawnDir = Vector3.zero;
                Vector3 splineSpawnDir = Vector3.zero;

                GameObject nextTrigger = null;
                Navigation_Trigger_One2One nextTriggerScript = null;
                GameObject spline = null;
                Navigation_Spline_Curve splineScript = null;

                if (m_curveType == CURVE_SETTING.CLOCKWISE)
                {
                    splineSpawnDir = (m_spawnDir == SPAWN_DIR.FORWARD ? m_curveRadius : -m_curveRadius) * (transform.right);
                    triggerSpawnDir = splineSpawnDir + (m_spawnDir == SPAWN_DIR.FORWARD ? m_curveRadius : -m_curveRadius) * (Quaternion.Euler(Vector3.up * -m_curveAngle) * transform.forward);

                    nextTrigger = Instantiate(m_one2OneTriggerPrefab, transform.position + triggerSpawnDir, transform.rotation * Quaternion.Euler(Vector3.up * m_curveAngle));
                }
                else
                {
                    splineSpawnDir = (m_spawnDir == SPAWN_DIR.FORWARD ? m_curveRadius : -m_curveRadius) * -transform.right;
                    triggerSpawnDir = splineSpawnDir + (m_spawnDir == SPAWN_DIR.FORWARD ? m_curveRadius : -m_curveRadius) * (Quaternion.Euler(Vector3.up * m_curveAngle) * transform.forward);

                    nextTrigger = Instantiate(m_one2OneTriggerPrefab, transform.position + triggerSpawnDir, transform.rotation * Quaternion.Euler(Vector3.up * -m_curveAngle));
                }

                //Grab varibles
                nextTriggerScript = nextTrigger.GetComponent<Navigation_Trigger_One2One>();
                spline = Instantiate(m_curvedSplinePrefab, transform.position + splineSpawnDir, transform.rotation);
                splineScript = spline.GetComponent<Navigation_Spline_Curve>();

                splineScript.m_totalDegrees = m_curveAngle;

                if (m_curveType == CURVE_SETTING.CLOCKWISE)
                    splineScript.m_rotationDirection = MOAREnums.ROT_DIRECTION.CLOCKWISE;
                else
                    splineScript.m_rotationDirection = MOAREnums.ROT_DIRECTION.ANTI_CLOCKWISE;

                splineScript.m_height = m_curveHeight;

                //Setup triggers/Spline varibles
                if (m_spawnDir == SPAWN_DIR.FORWARD)
                {
                    if (one2One != null)//One to one trigger
                    {
                        splineScript.m_splineStart = one2One;
                        one2One.m_forwardSplineInfo.m_spline = splineScript;
                    }
                    else //One to one trigger
                    {
                        splineScript.m_splineStart = junction;
                        junction.m_forwardSplineInfo.m_spline = splineScript;
                    }

                    nextTriggerScript.m_backwardSplineInfo.m_spline = splineScript;
                    splineScript.m_splineEnd = nextTriggerScript;
                }
                else
                {
                    if (one2One != null)//One to one trigger
                    {
                        splineScript.m_splineStart = one2One;
                        one2One.m_backwardSplineInfo.m_spline = splineScript;
                    }
                    else //One to one trigger
                    {
                        splineScript.m_splineStart = junction;
                        junction.m_backwardSplineInfo.m_spline = splineScript;
                    }

                    nextTriggerScript.m_forwardSplineInfo.m_spline = splineScript;
                    splineScript.m_splineEnd = nextTriggerScript;
                }

                //Setup Parents
                if (splineParent != null)
                {
                    spline.transform.SetParent(splineParent.transform, true);
                }
                if (triggerParent != null)
                {
                    nextTrigger.transform.SetParent(triggerParent.transform, true);
                }

                splineScript.SetupEndNode();//Move new trigger node into place

                //Names
                nextTrigger.name = "TriggerOne2One";
                spline.name = "SplineCurve";

            }
            else if (m_spawnSplineType == SPLINE_TYPE.JUNCTION)
            {
                if (m_junctionRadius == 0.0f)//Valid Inputs
                    return;

                if (one2One != null)//currently one to one needs to swap to junction
                {
                    junction = SwapToJunction(one2One, m_spawnDir);
                }

                if (m_junctionType == JUNCTION_TYPE.X_SECTION)
                {
                    //Creation
                    Vector3 xLeftTriggerSpawn = -transform.right * m_curveRadius + transform.forward * m_curveRadius;
                    Vector3 xForwardTriggerSpawn = transform.forward * m_curveRadius * 2;
                    Vector3 xRightTriggerSpawn = transform.right * m_curveRadius + transform.forward * m_curveRadius;

                    Vector3 x_blSplineSpawn = -transform.right * m_curveRadius;//bottom left
                    Vector3 x_brSplineSpawn = transform.right * m_curveRadius;//bottom right
                    Vector3 x_tlSplineSpawn = -transform.right * m_curveRadius + transform.forward * m_curveRadius * 2;//top left
                    Vector3 x_trSplineSpawn = transform.right * m_curveRadius + transform.forward * m_curveRadius * 2;//top right

                    GameObject xLeftJunction = Instantiate(m_junctionTriggerPrefab, transform.position + xLeftTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * 90));
                    GameObject xForwardJunction = Instantiate(m_junctionTriggerPrefab, transform.position + xForwardTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * 180));
                    GameObject xRightJunction = Instantiate(m_junctionTriggerPrefab, transform.position + xRightTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * -90));

                    GameObject xSplineBL = Instantiate(m_curvedSplinePrefab, transform.position + x_blSplineSpawn, transform.rotation);
                    GameObject xSplineBR = Instantiate(m_curvedSplinePrefab, transform.position + x_brSplineSpawn, transform.rotation);
                    GameObject xSplineTL = Instantiate(m_curvedSplinePrefab, transform.position + x_tlSplineSpawn, transform.rotation);
                    GameObject xSplineTR = Instantiate(m_curvedSplinePrefab, transform.position + x_trSplineSpawn, transform.rotation);

                    Vector3 xLeft2RightSpawn = (-transform.right + transform.forward) * m_junctionRadius;
                    Vector3 xTop2BottomSpawn = transform.forward * m_junctionRadius * 0.9f;

                    GameObject xLeft2RightSpline = Instantiate(m_strightSplinePrefab, transform.position - xLeft2RightSpawn, transform.rotation);
                    GameObject xTop2BottomSpline = Instantiate(m_strightSplinePrefab, transform.position + xTop2BottomSpawn, transform.rotation);

                    Navigation_Trigger_Junction xLeftJunctionScript = xLeftJunction.GetComponent<Navigation_Trigger_Junction>();
                    Navigation_Trigger_Junction xForwardJunctionScript = xForwardJunction.GetComponent<Navigation_Trigger_Junction>();
                    Navigation_Trigger_Junction xRightJunctionScript = xRightJunction.GetComponent<Navigation_Trigger_Junction>();

                    //Apply varibles to splines
                    Navigation_Spline_Curve xSplineBLScript = xSplineBL.GetComponent<Navigation_Spline_Curve>();
                    xSplineBLScript.m_rotationDirection = MOAREnums.ROT_DIRECTION.ANTI_CLOCKWISE;
                    xSplineBLScript.m_splineStart = junction;
                    xSplineBLScript.m_splineEnd = xLeftJunctionScript;

                    Navigation_Spline_Curve xSplineBRScript = xSplineBR.GetComponent<Navigation_Spline_Curve>();
                    xSplineBRScript.m_rotationDirection = MOAREnums.ROT_DIRECTION.CLOCKWISE;
                    xSplineBRScript.m_splineStart = junction;
                    xSplineBRScript.m_splineEnd = xRightJunctionScript;

                    Navigation_Spline_Curve xSplineTLScript = xSplineTL.GetComponent<Navigation_Spline_Curve>();
                    xSplineTLScript.m_rotationDirection = MOAREnums.ROT_DIRECTION.CLOCKWISE;
                    xSplineTLScript.m_splineStart = xForwardJunctionScript;
                    xSplineTLScript.m_splineEnd = xLeftJunctionScript;

                    Navigation_Spline_Curve xSplineTRScript = xSplineTR.GetComponent<Navigation_Spline_Curve>();
                    xSplineTRScript.m_rotationDirection = MOAREnums.ROT_DIRECTION.ANTI_CLOCKWISE;
                    xSplineTRScript.m_splineStart = xForwardJunctionScript;
                    xSplineTRScript.m_splineEnd = xRightJunctionScript;

                    Navigation_Spline_Line xLeft2RightSplineScript = xLeft2RightSpline.GetComponent<Navigation_Spline_Line>();
                    xLeft2RightSplineScript.m_splineStart = xLeftJunctionScript;
                    xLeft2RightSplineScript.m_splineEnd = xRightJunctionScript;

                    Navigation_Spline_Line xTop2BottomSplineScript = xTop2BottomSpline.GetComponent<Navigation_Spline_Line>();
                    xTop2BottomSplineScript.m_splineStart = xForwardJunctionScript;
                    xTop2BottomSplineScript.m_splineEnd = junction;

                    //Setup Parents
                    if (splineParent != null)
                    {
                        xSplineBLScript.transform.SetParent(splineParent.transform, true);
                        xSplineBRScript.transform.SetParent(splineParent.transform, true);
                        xSplineTLScript.transform.SetParent(splineParent.transform, true);
                        xSplineTRScript.transform.SetParent(splineParent.transform, true);
                    }
                    if (triggerParent != null)
                    {
                        xLeft2RightSplineScript.transform.SetParent(triggerParent.transform, true);
                        xTop2BottomSplineScript.transform.SetParent(triggerParent.transform, true);
                    }

                    //Names
                    xLeftJunction.name = "TriggerJunction";
                    xForwardJunction.name = "TriggerJunction";
                    xRightJunction.name = "TriggerJunction";

                    xSplineBL.name = "SplineCurve";
                    xSplineBR.name = "SplineCurve";
                    xSplineTL.name = "SplineCurve";
                    xSplineTR.name = "SplineCurve";

                    xLeft2RightSpline.name = "SplineLine";
                    xTop2BottomSpline.name = "SplineLine";

                    //Apply varibles to triggers
                    junction.m_forwardLeftSplineInfo.m_spline = xSplineBLScript;
                    junction.m_forwardSplineInfo.m_spline = xTop2BottomSplineScript;
                    junction.m_forwardRightSplineInfo.m_spline = xSplineBRScript;

                    xLeftJunctionScript.m_forwardLeftSplineInfo.m_spline = xSplineTLScript;
                    xLeftJunctionScript.m_forwardSplineInfo.m_spline = xLeft2RightSplineScript;
                    xLeftJunctionScript.m_forwardRightSplineInfo.m_spline = xSplineBLScript;

                    xForwardJunctionScript.m_forwardLeftSplineInfo.m_spline = xSplineTRScript;
                    xForwardJunctionScript.m_forwardSplineInfo.m_spline = xTop2BottomSplineScript;
                    xForwardJunctionScript.m_forwardRightSplineInfo.m_spline = xSplineTLScript;

                    xRightJunctionScript.m_forwardLeftSplineInfo.m_spline = xSplineBRScript;
                    xRightJunctionScript.m_forwardSplineInfo.m_spline = xLeft2RightSplineScript;
                    xRightJunctionScript.m_forwardRightSplineInfo.m_spline = xSplineTRScript;
                }
                else//Tsection
                {
                    Vector3 tLeftTriggerSpawn = Vector3.zero;
                    Vector3 tRightTriggerSpawn = Vector3.zero;
                    Vector3 tCenterTriggerSpawn = Vector3.zero;

                    Vector3 tLeftSplineSpawn = Vector3.zero;
                    Vector3 tRightSplineSpawn = Vector3.zero;
                    Vector3 tLeft2RightSplineSpawn = Vector3.zero;

                    GameObject tLeftJunction = null;
                    GameObject tCenterJunction = null;
                    GameObject tRightJunction = null;

                    //Setup used varibles
                    if (m_tSectionStart == T_SECTION_START.LEFT_T)
                    {
                        tLeftTriggerSpawn = Vector3.zero;
                        tRightTriggerSpawn = transform.forward * m_junctionRadius * 2;
                        tCenterTriggerSpawn = (transform.forward + transform.right) * m_junctionRadius;

                        tLeftSplineSpawn = transform.right * m_junctionRadius;
                        tRightSplineSpawn = transform.forward * m_junctionRadius * 2 + transform.right * m_junctionRadius;
                        tLeft2RightSplineSpawn = transform.forward * m_junctionRadius * 1.8f;

                        tLeftJunction = gameObject;
                        tRightJunction = Instantiate(m_junctionTriggerPrefab, transform.position + tRightTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * 180));
                        tCenterJunction = Instantiate(m_junctionTriggerPrefab, transform.position + tCenterTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * 90));
                    }
                    else if (m_tSectionStart == T_SECTION_START.CENTER_T)
                    {
                        tLeftTriggerSpawn = (transform.forward - transform.right) * m_junctionRadius;
                        tRightTriggerSpawn = (transform.forward + transform.right) * m_junctionRadius;
                        tCenterTriggerSpawn = Vector3.zero;

                        tLeftSplineSpawn = -transform.right * m_junctionRadius;
                        tRightSplineSpawn = transform.right * m_junctionRadius;
                        tLeft2RightSplineSpawn = (transform.forward * m_junctionRadius) + (-transform.right * 0.9f * m_junctionRadius);

                        tLeftJunction = Instantiate(m_junctionTriggerPrefab, transform.position + tLeftTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * 90)); ;
                        tRightJunction = Instantiate(m_junctionTriggerPrefab, transform.position + tRightTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * -90));
                        tCenterJunction = gameObject;
                    }
                    else if (m_tSectionStart == T_SECTION_START.RIGHT_T)
                    {
                        tLeftTriggerSpawn = transform.forward * m_junctionRadius * 2;
                        tRightTriggerSpawn = Vector3.zero;
                        tCenterTriggerSpawn = (transform.forward - transform.right) * m_junctionRadius;

                        tLeftSplineSpawn = transform.forward * m_junctionRadius * 2 - transform.right * m_junctionRadius;
                        tRightSplineSpawn = -transform.right * m_junctionRadius;
                        tLeft2RightSplineSpawn = transform.forward * m_junctionRadius * 0.2f;

                        tLeftJunction = Instantiate(m_junctionTriggerPrefab, transform.position + tLeftTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * 180)); ;
                        tRightJunction = gameObject;
                        tCenterJunction = Instantiate(m_junctionTriggerPrefab, transform.position + tCenterTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * 180));
                    }

                    GameObject tLeftSpline = Instantiate(m_curvedSplinePrefab, transform.position + tLeftSplineSpawn, transform.rotation);
                    GameObject tRightSpline = Instantiate(m_curvedSplinePrefab, transform.position + tRightSplineSpawn, transform.rotation);

                    GameObject tStrightSpline = Instantiate(m_strightSplinePrefab, transform.position + tLeft2RightSplineSpawn, transform.rotation);

                    //Build connetions
                    Navigation_Trigger_Junction tLeftJunctionScript = tLeftJunction.GetComponent<Navigation_Trigger_Junction>();
                    Navigation_Trigger_Junction tCenterJunctionScript = tCenterJunction.GetComponent<Navigation_Trigger_Junction>();
                    Navigation_Trigger_Junction tRightJunctionScript = tRightJunction.GetComponent<Navigation_Trigger_Junction>();

                    Navigation_Spline_Curve tSplineLeftScript = tLeftSpline.GetComponent<Navigation_Spline_Curve>();
                    tSplineLeftScript.m_rotationDirection = MOAREnums.ROT_DIRECTION.ANTI_CLOCKWISE;
                    tSplineLeftScript.m_splineStart = tCenterJunctionScript;
                    tSplineLeftScript.m_splineEnd = tLeftJunctionScript;

                    Navigation_Spline_Curve tSplineRightScript = tRightSpline.GetComponent<Navigation_Spline_Curve>();
                    tSplineRightScript.m_rotationDirection = MOAREnums.ROT_DIRECTION.CLOCKWISE;
                    tSplineRightScript.m_splineStart = tCenterJunctionScript;
                    tSplineRightScript.m_splineEnd = tRightJunctionScript;

                    Navigation_Spline_Line tLeft2RightSplineScript = tStrightSpline.GetComponent<Navigation_Spline_Line>();
                    tLeft2RightSplineScript.m_splineStart = tLeftJunctionScript;
                    tLeft2RightSplineScript.m_splineEnd = tRightJunctionScript;

                    //Setup Parents
                    if (splineParent != null)
                    {
                        tLeftSpline.transform.SetParent(splineParent.transform, true);
                        tRightSpline.transform.SetParent(splineParent.transform, true);
                        tStrightSpline.transform.SetParent(splineParent.transform, true);
                    }
                    if (triggerParent != null)
                    {
                        tLeftJunction.transform.SetParent(triggerParent.transform, true);
                        tCenterJunction.transform.SetParent(triggerParent.transform, true);
                        tRightJunction.transform.SetParent(triggerParent.transform, true);
                    }

                    //Names
                    tLeftJunction.name = "TriggerJunction";
                    tRightJunction.name = "TriggerJunction";
                    tCenterJunction.name = "TriggerJunction";

                    tLeftSpline.name = "SplineCurve";
                    tRightSpline.name = "SplineCurve";

                    tStrightSpline.name = "SplineLine";

                    //Apply varibles to triggers
                    tLeftJunctionScript.m_forwardSplineInfo.m_spline = tLeft2RightSplineScript;
                    tLeftJunctionScript.m_forwardRightSplineInfo.m_spline = tSplineLeftScript;

                    tCenterJunctionScript.m_forwardLeftSplineInfo.m_spline = tSplineLeftScript;
                    tCenterJunctionScript.m_forwardRightSplineInfo.m_spline = tSplineRightScript;

                    tRightJunctionScript.m_forwardLeftSplineInfo.m_spline = tSplineRightScript;
                    tRightJunctionScript.m_forwardSplineInfo.m_spline = tLeft2RightSplineScript;
                }
            }

            Navigation_Trigger baseTrigger = GetComponent<Navigation_Trigger>();
            baseTrigger.UpdateCollidier();
        }

        if(m_merge)
        {
            m_merge = false;



        }
    }

    private Navigation_Trigger_One2One SwapToOne2One(Navigation_Trigger_Junction p_junction)
    {
        Navigation_Trigger_One2One one2One = p_junction.gameObject.AddComponent<Navigation_Trigger_One2One>();

        one2One.m_forwardSplineInfo = p_junction.m_forwardSplineInfo;
        one2One.m_backwardSplineInfo = p_junction.m_backwardSplineInfo;

        if (one2One.m_forwardSplineInfo.m_spline != null)
        {
            if (one2One.m_forwardSplineInfo.m_spline.m_splineEnd == p_junction)
                one2One.m_forwardSplineInfo.m_spline.m_splineEnd = one2One;
            else
                one2One.m_forwardSplineInfo.m_spline.m_splineStart = one2One;
        }

        if (one2One.m_backwardSplineInfo.m_spline != null)
        {
            if (one2One.m_backwardSplineInfo.m_spline.m_splineEnd == p_junction)
                one2One.m_backwardSplineInfo.m_spline.m_splineEnd = one2One;
            else
                one2One.m_backwardSplineInfo.m_spline.m_splineStart = one2One;
        }

        //Cant call destroy immediatly
        UnityEditor.EditorApplication.delayCall += () =>
        {
            DestroyImmediate(p_junction);
        };

        return one2One;
    }

    private Navigation_Trigger_Junction SwapToJunction(Navigation_Trigger_One2One p_one2One, SPAWN_DIR p_dir)
    {
        Navigation_Trigger_Junction junction = p_one2One.gameObject.AddComponent<Navigation_Trigger_Junction>();

        if (p_dir == SPAWN_DIR.FORWARD)//Spawing forwards, setup same
        {
            junction.m_forwardSplineInfo = p_one2One.m_forwardSplineInfo;
            junction.m_backwardSplineInfo = p_one2One.m_backwardSplineInfo;
        }
        else//On backwards, junction will be backswards, so flip all
        {
            transform.Rotate(Vector3.up, 180);//Flip as junciton should face in
            junction.m_forwardSplineInfo = p_one2One.m_backwardSplineInfo;
            junction.m_backwardSplineInfo = p_one2One.m_forwardSplineInfo;
        }

        if (junction.m_forwardSplineInfo.m_spline != null)
        {
            if (junction.m_forwardSplineInfo.m_spline.m_splineEnd == p_one2One)
                junction.m_forwardSplineInfo.m_spline.m_splineEnd = junction;
            else
                junction.m_forwardSplineInfo.m_spline.m_splineStart = junction;
        }

        if (junction.m_backwardSplineInfo.m_spline != null)
        {
            if (junction.m_backwardSplineInfo.m_spline.m_splineEnd == p_one2One)
                junction.m_backwardSplineInfo.m_spline.m_splineEnd = junction;
            else
                junction.m_backwardSplineInfo.m_spline.m_splineStart = junction;
        }

        //Cant call destroy immediatly
        UnityEditor.EditorApplication.delayCall += () =>
        {
            DestroyImmediate(p_one2One);
        };

        return junction;
    }
#endif
}
