using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    public GameObject m_equipedWeapon = null;
    private WeaponSocket m_weaponSocket = null;

    //-------------------
    //Initiliase characters inventory
    //  Equip weapon
    //-------------------
    public void InitInventory()
    {
        m_weaponSocket = GetComponentInChildren<WeaponSocket>();

        EquipWeapon(m_equipedWeapon);
    }

    //-------------------
    //Equiping of weapon, weapon is instatiated.
    //  TODO crete all weapons at start and object pool
    //
    //Param GameObject: weapon to create and equip
    //-------------------
    public void EquipWeapon(GameObject p_weaponPrefab)
    {
        if (m_weaponSocket != null && p_weaponPrefab != null)
        {
            Instantiate(p_weaponPrefab, m_weaponSocket.transform);
        }
    }
}
