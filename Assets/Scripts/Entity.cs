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

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public virtual void InitEntity()
    {
        m_splinePhysics = GetComponent<SplinePhysics>();

        if (m_splinePhysics == null)
        {
#if UNITY_EDITOR
            Debug.Log(name + " has no spline physics attached, considering adding the spline physcis");
#endif
            Destroy(this);
            return;
        }
    }

    protected virtual void Update()
    {
        //Apply Velocity
        if(m_gravity) 
            m_localVelocity.y += PhysicsController.m_gravity * Time.deltaTime;

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
    /// Get turning direction for junction navigation, based off current input
    /// </summary>
    /// <param name="p_node">junction entity will pass through</param>
    /// <returns>Path entity will desire to take</returns>
    public virtual TURNING_DIR GetDesiredTurning(Pathing_Node p_node)
    {
        return TURNING_DIR.CENTER;
    }

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
