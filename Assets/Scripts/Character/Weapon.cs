using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Weapon : MonoBehaviour
{
    [Tooltip("Weapon index for swinging animation, 0 = unarmed, 1 = axe")]
    public int m_animationIndex = 0;

    [HideInInspector]
    public List<Character> m_hitCharacters = new List<Character>();

    public BoxCollider m_hitCollider = null;
    public BoxCollider m_physicsCollider = null;
    private Rigidbody m_rigidBody = null;
    [Header("Weapon Stats")]
    public float m_weaponLightDamage = 1.0f;
    public float m_weaponHeavyDamage = 2.0f;

    public Character m_parentCharacter = null;

    private void Start()
    {
        m_hitCollider.enabled = false;
        m_physicsCollider.enabled = false;
        m_rigidBody = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider p_other)
    {
        Character otherCharacter = p_other.GetComponentInChildren<Character>();
        if (otherCharacter != null && !m_hitCharacters.Contains(otherCharacter))
        {
            if (otherCharacter.m_characterTeam != m_parentCharacter.m_characterTeam)
            {
                //determine damage
                float totalDamage = m_parentCharacter.m_currentAttackType == Character.ATTACK_TYPE.LIGHT ? - m_weaponLightDamage : -m_weaponHeavyDamage;

                otherCharacter.ModifyHealth(totalDamage);
            }

            m_hitCharacters.Add(otherCharacter);
        }
    }

    private void OnTriggerExit(Collider p_other)
    {
        Character otherCharacter = p_other.GetComponentInChildren<Character>();
        if (otherCharacter != null && m_hitCharacters.Contains(otherCharacter))
        {
            m_hitCharacters.Remove(otherCharacter);
        }
    }

    public void EnableWeaponCollisions()
    {
        m_hitCollider.enabled = true;
        m_hitCharacters.Clear();
    }

    public void DisableWeaponCollisions()
    {
        m_hitCollider.enabled = false;
    }

    public void DropWeapon()
    {
        transform.parent = null;
        m_physicsCollider.enabled = true;
        m_rigidBody.useGravity = true;
    }
}
