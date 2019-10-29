using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public enum TURNING_DIR { CENTER, LEFT, RIGHT }

    public Vector3 m_localVelocity = Vector3.zero;

    [SerializeField]
    private const float DESTRUCTION_TIME = 1.0f;

    [HideInInspector]
    public SplinePhysics m_splinePhysics = null;

    protected virtual void Start()
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
    /// <summary>
    /// Get turning direction for junction navigation, based off current input
    /// </summary>
    /// <param name="p_trigger">junction entity will pass through</param>
    /// <returns>Path entity will desire to take</returns>
    public virtual TURNING_DIR GetDesiredTurning(Navigation_Trigger_Junction p_trigger)
    {
        return TURNING_DIR.CENTER;
    }

    public void DestroyEntity(float p_time = DESTRUCTION_TIME)
    {
        StartCoroutine(EntityDestroy(p_time));
    }

    /// <summary>
    /// Apply delay for entity destruction to allow any effects
    /// </summary>
    /// <returns>m_destructionTime seconds</returns>
    private IEnumerator EntityDestroy(float p_time)
    {
        EntityDestroyed();
        yield return new WaitForSeconds(p_time);
        Destroy(gameObject);
    }

    /// <summary>
    /// Called as an entity is destroyed intialy
    /// </summary>
    protected virtual void EntityDestroyed()
    {

    }
}
