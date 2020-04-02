using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    public List<GameObject> m_weaponPrefabs = new List<GameObject>();
    private List<WeaponManager> m_weapons = new List<WeaponManager>();

    private int m_currentWeapon = 0;

    /// <summary>
    /// Init inventory, equip first weapon
    /// </summary>
    /// <param name="p_character">Parent character</param>
    public void InitInventory(Character p_character)
    {
        for (int weaponIndex = 0; weaponIndex < m_weaponPrefabs.Count; weaponIndex++)
        {
            GameObject newWeapon = Instantiate(m_weaponPrefabs[weaponIndex]);

            WeaponManager weaponManager = newWeapon.GetComponent<WeaponManager>();

            if(weaponManager == null)
            {
#if UNITY_EDITOR
                Debug.Log(m_weaponPrefabs[weaponIndex].name + " is an invalid prefab for weapon as it does not contrain the weapon manager");
#endif
            }
            else
            {
                m_weapons.Add(weaponManager);
                weaponManager.Init(p_character);
                newWeapon.gameObject.SetActive(false);
            }
        }

        EquipWeapon(m_currentWeapon);
    }

    /// <summary>
    /// Equip weapon
    /// </summary>
    /// <param name="p_weaponIndex">Index of weapon to equip</param>
    public void EquipWeapon(int p_weaponIndex)
    {
        if (p_weaponIndex < 0 || p_weaponIndex >= m_weapons.Count)
            return;

        m_weapons[m_currentWeapon].gameObject.SetActive(false);
        m_currentWeapon = p_weaponIndex;
        m_weapons[m_currentWeapon].gameObject.SetActive(true);
    }

    public WeaponManager GetCurrentWeapon()
    {
        return m_weapons[m_currentWeapon];
    }
}
