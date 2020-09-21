using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public enum TURNING_DIR { CENTER, RIGHT, LEFT }

    public Vector3 m_localVelocity = Vector3.zero;
    public bool m_gravity = false;

    [SerializeField]
    private const float DESTRUCTION_TIME = 1.0f;

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
    }

    /// <summary>
    /// Update an entity, this should be called from scene controller
    /// Used to handle different scene state, pause vs in game etc
    /// </summary>
    public virtual void UpdateEntity()
    {
        //Stop colliding with objects
        m_splinePhysics.UpdatePhysics();
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
    /// Hard set the value of velocity
    /// </summary>
    /// <param name="p_val">velocity</param>
    public void HardSetVelocity(float p_val)
    {
        m_localVelocity.x = p_val;
    }

    /// <summary>
    /// Hard set the value of velocity
    /// </summary>
    /// <param name="p_val">velocity</param>
    public void HardSetUpwardsVelocity(float p_val)
    {
        m_localVelocity.y = p_val;
    }

    /// <summary>
    /// Translate the entity 
    /// </summary>
    /// <param name="p_val">Translation distance</param>
    public void Translate(float p_val)
    {
        m_splinePhysics.m_currentSplinePercent += p_val / m_splinePhysics.m_currentSpline.m_splineLength;
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
        EntityImmediateDestory();
        yield return new WaitForSeconds(DESTRUCTION_TIME); 
        EntityDelayedDestroy();
        Destroy(gameObject);
    }

    /// <summary>
    /// Apply delay for entity destruction to allow any effects
    /// </summary>
    protected virtual void EntityDelayedDestroy()
    {

    }

    /// <summary>
    /// Called as an entity is destroyed intialy
    /// </summary>
    protected virtual void EntityImmediateDestory()
    {

    }
}
