using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    public GameObject m_equipedWeapon = null;
    private WeaponSocket m_weaponSocket = null;

    public void InitInventory()
    {
        m_weaponSocket = GetComponentInChildren<WeaponSocket>();

        EquipWeapon(m_equipedWeapon);
    }

    public void EquipWeapon(GameObject p_weaponPrefab)
    {
        if (m_weaponSocket != null && p_weaponPrefab != null)
        {
            Instantiate(p_weaponPrefab, m_weaponSocket.transform);
        }
    }
}
