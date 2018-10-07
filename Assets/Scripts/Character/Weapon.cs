using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Weapon : MonoBehaviour
{
    [HideInInspector]
    public List<Character> m_hitCharacters = new List<Character>();

    private BoxCollider m_boxCollider = null;
    private Character m_parentCharacter = null;

    [Header("Weapon Stats")]
    public float m_weaponLightDamage = 1.0f;
    public float m_weaponHeavyDamage = 2.0f;

    private void Start()
    {
        m_boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider p_other)
    {
        Character otherCharacter = p_other.GetComponent<Character>();
        if (otherCharacter != null && !m_hitCharacters.Contains(otherCharacter))
        {
            m_hitCharacters.Add(otherCharacter);
        }
    }

    public void EnableWeaponCollisions()
    {
        m_boxCollider.enabled = true;
        m_hitCharacters.Clear();
    }

    public void DisableWeaponCollisions()
    {
        m_boxCollider.enabled = false;
    }
}
