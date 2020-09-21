using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Weapon : MonoBehaviour
{
    public float m_baseDamage = 10.0f;

    private Collider m_colldier = null;
    private Character m_character = null;

    private float m_currentDamageModifier = 1.0f;

    private List<Character> m_hitCharacters = new List<Character>();

    private void Awake()
    {
        m_colldier = GetComponent<Collider>();

        DisableWeaponDamage();
    }

    /// <summary>
    /// Setup varibles for later use
    /// </summary>
    /// <param name="p_character">Parent character</param>
    public void Init(Character p_character)
    {
        m_character = p_character;  
    }

    /// <summary>
    /// Start a weapons manoeuvre
    /// Setup all required varibles
    /// </summary>
    /// <param name="p_damageModifier">How much damage is modified based off the leafs settings</param>
    public void EnableWeaponDamage(float p_damageModifier)
    {
        m_colldier.enabled = true;
        m_currentDamageModifier = p_damageModifier;
        m_hitCharacters.Clear();
    }

    /// <summary>
    /// Deal damage to all chraters that are in wepaon range
    /// </summary>
    public void DisableWeaponDamage()
    {
        m_colldier.enabled = false;
    }

    /// <summary>
    /// Object trigger enter
    /// Add to list of characters to damage if valid target
    /// </summary>
    /// <param name="other">Other collider</param>
    private void OnTriggerEnter(Collider other)
    {
        Character otherCharacter = other.GetComponent<Character>();

        if(otherCharacter != null && otherCharacter != m_character && m_character.m_team != otherCharacter.m_team) //Is character collider, not parent, different team
        {
            if (!m_hitCharacters.Contains(otherCharacter))
            {
                //TODO get proper damage calc
                m_character.DealDamage(m_baseDamage * m_currentDamageModifier, otherCharacter);
                m_hitCharacters.Add(otherCharacter);
            }
        }
    }
}
