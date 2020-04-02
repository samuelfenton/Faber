using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrigger : MonoBehaviour
{
    public float m_baseLightDamage = 3.0f;
    public float m_baseHeavyDamage = 5.0f;

    private Collider m_colldier = null;
    private WeaponManager m_weaponManager = null;

    /// <summary>
    /// Setup varibles for later use
    /// </summary>
    /// <param name="p_weaponManager">Manager handing collisions</param>
    public void Init(WeaponManager p_weaponManager)
    {
        m_weaponManager = p_weaponManager;

        m_colldier = GetComponent<Collider>();
    }

    /// <summary>
    /// Set colldier state
    /// </summary>
    /// <param name="p_val">true for enabled</param>
    public void ToggleTrigger(bool p_val)
    {
        m_colldier.enabled = p_val;
    }

    private void OnTriggerEnter(Collider other)
    {
        m_weaponManager.ColliderEntered(other);
    }
}
