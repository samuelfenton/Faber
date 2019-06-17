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

    public enum SPLINE_TYPE {ONE_2_ONE, ONE_2_ONE_CURVE, JUNCTION };
    [SerializeField]
    public SPLINE_TYPE m_splineType = SPLINE_TYPE.ONE_2_ONE;
    private SPLINE_TYPE m_previousTriggerType = SPLINE_TYPE.ONE_2_ONE;

    [SerializeField]
    private Navigation_Trigger m_navigationTrigger = null;

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

    private void Start()
    {
        m_navigationTrigger = GetComponent<Navigation_Trigger>();
        if(m_navigationTrigger!= null)//Started with trigger attached
        {
            if(GetComponent<Navigation_Trigger_One2One>() != null)
            {
                m_splineType = SPLINE_TYPE.ONE_2_ONE;
                m_previousTriggerType = SPLINE_TYPE.ONE_2_ONE;
            }
            else
            {
                m_splineType = SPLINE_TYPE.JUNCTION;
                m_previousTriggerType = SPLINE_TYPE.JUNCTION;
            }
        }
    }

    private void Update()
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

            //Creation of connections
            if (m_splineType == SPLINE_TYPE.ONE_2_ONE)
            {
                if(m_spawnDir == SPAWN_DIR.FORWARD && junction !=null)//currently junction moving forward, swap from junction to one to one
                {
                    one2One = SwapToOne2One(junction);
                }

                //Create
                Vector3 spawnDir = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.localToWorldMatrix * m_one2OneSplineDir);

                GameObject nextTrigger = Instantiate(m_one2OneTriggerPrefab, transform.position + spawnDir, transform.rotation);
                Navigation_Trigger_One2One nextTriggerScript = nextTrigger.GetComponent<Navigation_Trigger_One2One>();
                GameObject spline = Instantiate(m_strightSplinePrefab, transform.position + spawnDir * 0.5f, transform.rotation);
                Navigation_Spline_Line splineScript = spline.GetComponent<Navigation_Spline_Line>();


                //Setup triggers/Spline varibles
                if(m_spawnDir == SPAWN_DIR.FORWARD)
                {
                    if(one2One !=null)//One to one trigger
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
                        splineScript.m_splineEnd = one2One;
                        one2One.m_backwardSplineInfo.m_spline = splineScript;
                    }
                    else //One to one trigger
                    {
                        splineScript.m_splineEnd = junction;
                        junction.m_backwardSplineInfo.m_spline = splineScript;
                    }

                    nextTriggerScript.m_forwardSplineInfo.m_spline = splineScript;
                    splineScript.m_splineStart = nextTriggerScript;
                }

                //Add to game object to help with management
                spline.transform.parent = GameObject.FindGameObjectWithTag("Splines").transform;
                nextTrigger.transform.parent = GameObject.FindGameObjectWithTag("Triggers").transform;
            }
            else if (m_splineType == SPLINE_TYPE.ONE_2_ONE_CURVE)
            {
                if (m_spawnDir == SPAWN_DIR.FORWARD && junction != null)//currently junction moving forward, swap from junction to one to one
                {
                    one2One = SwapToOne2One(junction);
                }

                //Create
                Vector3 triggerSpawnDir = Vector3.zero;
                Vector3 splineSpawnDir = Vector3.zero;

                GameObject nextTrigger = null;
                Navigation_Trigger_One2One nextTriggerScript = null;
                GameObject spline = null;
                Navigation_Spline_Curve splineScript = null;

                if (m_curveType == CURVE_SETTING.CLOCKWISE)
                {
                    triggerSpawnDir = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * ((transform.forward + transform.right) * m_curveRadius);
                    splineSpawnDir = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.right * m_curveRadius);
                    nextTrigger = Instantiate(m_one2OneTriggerPrefab, transform.position + triggerSpawnDir, transform.rotation * Quaternion.Euler(Vector3.up * 90));
                }
                else
                {
                    triggerSpawnDir = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) *(transform.forward - transform.right) * m_curveRadius;
                    splineSpawnDir = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * -transform.right * m_curveRadius;

                    nextTrigger = Instantiate(m_one2OneTriggerPrefab, transform.position + triggerSpawnDir, transform.rotation * Quaternion.Euler(Vector3.up * -90));
                }


                //Grab varibles
                nextTriggerScript = nextTrigger.GetComponent<Navigation_Trigger_One2One>();
                spline = Instantiate(m_curvedSplinePrefab, transform.position + splineSpawnDir, transform.rotation);
                splineScript = spline.GetComponent<Navigation_Spline_Curve>();

                if (m_curveType == CURVE_SETTING.CLOCKWISE)
                    splineScript.m_rotationDirection = MOAREnums.ROT_DIRECTION.CLOCKWISE;
                else
                    splineScript.m_rotationDirection = MOAREnums.ROT_DIRECTION.ANTI_CLOCKWISE;

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
                        splineScript.m_splineEnd = one2One;
                        one2One.m_backwardSplineInfo.m_spline = splineScript;
                    }
                    else //One to one trigger
                    {
                        splineScript.m_splineEnd = junction;
                        junction.m_backwardSplineInfo.m_spline = splineScript;
                    }

                    nextTriggerScript.m_forwardSplineInfo.m_spline = splineScript;
                    splineScript.m_splineStart = nextTriggerScript;
                }

                //Add to game object to help with management
                spline.transform.parent = GameObject.FindGameObjectWithTag("Splines").transform;
                nextTrigger.transform.parent = GameObject.FindGameObjectWithTag("Triggers").transform;
            }
            else if (m_splineType == SPLINE_TYPE.JUNCTION)
            {
                if (one2One != null)//currently one to one needs to swap to junction
                {
                    junction = SwapToJunction(one2One);
                }

                if(m_junctionType == JUNCTION_TYPE.X_SECTION)
                {
                    //Creation
                    Vector3 xLeftTriggerSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (-transform.right * m_curveRadius + transform.forward * m_curveRadius);
                    Vector3 xForwardTriggerSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.forward * m_curveRadius * 2);
                    Vector3 xRightTriggerSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.right * m_curveRadius + transform.forward * m_curveRadius);

                    Vector3 x_blSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (-transform.right * m_curveRadius);//bottom left
                    Vector3 x_brSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.right * m_curveRadius);//bottom right
                    Vector3 x_tlSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (-transform.right * m_curveRadius + transform.forward * m_curveRadius * 2);//top left
                    Vector3 x_trSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.right * m_curveRadius + transform.forward * m_curveRadius * 2);//top right

                    GameObject xLeftJunction = Instantiate(m_junctionTriggerPrefab, transform.position + xLeftTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * 90));
                    GameObject xForwardJunction = Instantiate(m_junctionTriggerPrefab, transform.position + xForwardTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * 180));
                    GameObject xRightJunction = Instantiate(m_junctionTriggerPrefab, transform.position + xRightTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * -90));

                    GameObject xSplineBL = Instantiate(m_curvedSplinePrefab, transform.position + x_blSplineSpawn, transform.rotation);
                    GameObject xSplineBR = Instantiate(m_curvedSplinePrefab, transform.position + x_brSplineSpawn, transform.rotation);
                    GameObject xSplineTL = Instantiate(m_curvedSplinePrefab, transform.position + x_tlSplineSpawn, transform.rotation);
                    GameObject xSplineTR = Instantiate(m_curvedSplinePrefab, transform.position + x_trSplineSpawn, transform.rotation);

                    Vector3 xLeft2RightSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * ((-transform.right + transform.forward) * m_junctionRadius);
                    Vector3 xTop2BottomSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.forward * m_junctionRadius * 0.9f);

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
                        tRightTriggerSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.forward * m_junctionRadius * 2);
                        tCenterTriggerSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * ((transform.forward + transform.right) * m_junctionRadius);

                        tLeftSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.right * m_junctionRadius);
                        tRightSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.forward * m_junctionRadius * 2 + transform.right * m_junctionRadius);
                        tLeft2RightSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.forward * m_junctionRadius * 1.8f);

                        tLeftJunction = gameObject;
                        tRightJunction = Instantiate(m_junctionTriggerPrefab, transform.position + tRightTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * 180));
                        tCenterJunction = Instantiate(m_junctionTriggerPrefab, transform.position + tCenterTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * 90));
                    }
                    else if (m_tSectionStart == T_SECTION_START.CENTER_T)
                    {
                        tLeftTriggerSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * ((transform.forward - transform.right) * m_junctionRadius);
                        tRightTriggerSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * ((transform.forward + transform.right) * m_junctionRadius);
                        tCenterTriggerSpawn = Vector3.zero;

                        tLeftSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (-transform.right * m_junctionRadius);
                        tRightSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.right * m_junctionRadius);
                        tLeft2RightSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * ((transform.forward * m_junctionRadius) + (-transform.right * 0.9f * m_junctionRadius));

                        tLeftJunction = Instantiate(m_junctionTriggerPrefab, transform.position + tLeftTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * 90)); ;
                        tRightJunction = Instantiate(m_junctionTriggerPrefab, transform.position + tRightTriggerSpawn, transform.rotation * Quaternion.Euler(Vector3.up * -90));
                        tCenterJunction = gameObject;
                    }
                    else if (m_tSectionStart == T_SECTION_START.RIGHT_T)
                    {
                        tLeftTriggerSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.forward * m_junctionRadius * 2);
                        tRightTriggerSpawn = Vector3.zero;
                        tCenterTriggerSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * ((transform.forward - transform.right) * m_junctionRadius);

                        tLeftSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.forward * m_junctionRadius * 2 - transform.right * m_junctionRadius); 
                        tRightSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (-transform.right * m_junctionRadius);
                        tLeft2RightSplineSpawn = (m_spawnDir == SPAWN_DIR.FORWARD ? 1 : -1) * (transform.forward * m_junctionRadius * 0.2f);

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

                    tLeftJunctionScript.m_forwardSplineInfo.m_spline = tLeft2RightSplineScript;
                    tLeftJunctionScript.m_forwardRightSplineInfo.m_spline = tSplineLeftScript;

                    tCenterJunctionScript.m_forwardLeftSplineInfo.m_spline = tSplineLeftScript;
                    tCenterJunctionScript.m_forwardRightSplineInfo.m_spline = tSplineRightScript;

                    tRightJunctionScript.m_forwardLeftSplineInfo.m_spline = tSplineRightScript;
                    tRightJunctionScript.m_forwardSplineInfo.m_spline = tLeft2RightSplineScript;
                }
            }
        }

        if(m_merge)
        {
            m_merge = false;



        }
    }

    private Navigation_Trigger_One2One SwapToOne2One(Navigation_Trigger_Junction p_junction)
    {
        Navigation_Trigger_One2One one2One = p_junction.gameObject.AddComponent<Navigation_Trigger_One2One>();

        one2One.m_backwardSplineInfo.m_spline = p_junction.m_backwardSplineInfo.m_spline;

        if (p_junction.m_backwardSplineInfo.m_spline.m_splineEnd == p_junction)
            p_junction.m_backwardSplineInfo.m_spline.m_splineEnd = one2One;
        else
            p_junction.m_backwardSplineInfo.m_spline.m_splineStart = one2One;

        DestroyImmediate(p_junction);
        return one2One;
    }

    private Navigation_Trigger_Junction SwapToJunction(Navigation_Trigger_One2One p_one2One)
    {
        Navigation_Trigger_Junction junction = p_one2One.gameObject.AddComponent<Navigation_Trigger_Junction>();

        junction.m_backwardSplineInfo.m_spline = p_one2One.m_backwardSplineInfo.m_spline;
        if (p_one2One.m_backwardSplineInfo.m_spline.m_splineEnd == p_one2One)
            p_one2One.m_backwardSplineInfo.m_spline.m_splineEnd = junction;
        else
            p_one2One.m_backwardSplineInfo.m_spline.m_splineStart = junction;
        DestroyImmediate(p_one2One);
        return junction;
    }
}
