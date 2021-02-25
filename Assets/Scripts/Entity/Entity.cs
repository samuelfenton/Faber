using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    private const float DESTRUCTION_TIME = 1.0f;

    public enum TURNING_DIR { CENTER, RIGHT, LEFT }

    [HideInInspector]
    public SplinePhysics m_splinePhysics = null;

    [HideInInspector]
    public SceneController_InGame m_sceneController = null;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public virtual void InitEntity()
    {
        m_splinePhysics = GetComponent<SplinePhysics>();
        m_sceneController = (SceneController_InGame)MasterController.Instance.m_currentSceneController;

        if (m_splinePhysics == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(name + " has no spline physics attached, considering adding the spline physcis");
#endif
            Destroy(this);
            return;
        }

        m_splinePhysics.Init();

        //Use voxeliser as needed
        Voxeliser_Burst[] voxelisers = GetComponents<Voxeliser_Burst>();

        for (int voxeliserIndex = 0; voxeliserIndex < voxelisers.Length; voxeliserIndex++)
        {
            voxelisers[voxeliserIndex].InitVoxeliser();
        }
    }

    /// <summary>
    /// Update an entity, this should be called from scene controller
    /// Used to handle different scene state, pause vs in game etc
    /// </summary>
    public virtual void UpdateEntity()
    {

    }

    /// <summary>
    /// Update an entity, this should be called from scene controller
    /// Used to handle fixed updates, tranfroms etc
    /// </summary>
    public virtual void FixedUpdateEntity()
    {
        m_splinePhysics.ResolvePhysics();
    }

    /// <summary>
    /// Swap entity to new spline
    /// </summary>
    /// <param name="p_transferNode">Node that is transfering entity</param>
    /// <param name="p_newSpline">The new spline to be on</param>
    public void SwapSplines(Pathing_Node p_transferNode, Pathing_Spline p_newSpline)
    {
        //Setup percent
        m_splinePhysics.m_currentSplinePercent = p_newSpline.GetPercentForNode(p_transferNode);

        m_splinePhysics.m_currentSpline = p_newSpline;
    }

    /// <summary>
    /// Translate the entity 
    /// Should be relative to forwards
    /// </summary>
    /// <param name="p_val">Translation distance</param>
    public virtual void SplineTranslate(float p_val)
    {
        if(p_val > 0.0f && !m_splinePhysics.m_forwardCollision || p_val < 0.0f && !m_splinePhysics.m_backwardCollision)
        {
            if(AllignedToSpline())
            {
                m_splinePhysics.m_currentSplinePercent += p_val / m_splinePhysics.m_currentSpline.m_splineLength;
            }
            else
            {
                m_splinePhysics.m_currentSplinePercent -= p_val / m_splinePhysics.m_currentSpline.m_splineLength;
            }
        }
    }

    /// <summary>
    /// Determine if entity is alligned to same forward direction as the spline they are on
    /// </summary>
    /// <returns>True when facing same direction</returns>
    public bool AllignedToSpline()
    {
        Vector3 splineForwards = m_splinePhysics.m_currentSpline.GetForwardDir(m_splinePhysics.m_currentSplinePercent);
        return (Vector3.Dot(splineForwards, transform.forward) > 0);
    }

    /// <summary>
    /// Get turning direction for junction navigation, based off current input
    /// </summary>
    /// <param name="p_node">junction entity will pass through</param>
    /// <returns>Path entity will desire to take</returns>
    public virtual TURNING_DIR GetDesiredTurning(Pathing_Node p_node)
    {
        return TURNING_DIR.CENTER;
    }

    /// <summary>
    /// Destory a given entity, will call immeditate and delayed destruciton functions
    /// </summary>
    /// <returns>Waits DESTRUCTION_TIME</returns>
    public IEnumerator DestroyEntity()
    {
        EntityInitialDestory();
        yield return new WaitForSeconds(DESTRUCTION_TIME); 
        EntityDelayedDestroy();
        //Destroy(gameObject);
    }

    /// <summary>
    /// Called as an entity is destroyed intialy
    /// </summary>
    protected virtual void EntityInitialDestory()
    {

    }

    /// <summary>
    /// Apply delay for entity destruction to allow any effects
    /// </summary>
    protected virtual void EntityDelayedDestroy()
    {

    }

}
