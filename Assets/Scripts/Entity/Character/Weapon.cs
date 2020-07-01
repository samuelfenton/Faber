using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float m_baseDamage = 10.0f;

    private Collider m_colldier = null;
    private Character m_character = null;

    private float m_currentDamageModifier = 1.0f;

    private List<Character> m_hitCharacters = new List<Character>();

    /// <summary>
    /// Setup varibles for later use
    /// </summary>
    /// <param name="p_character">Parent character</param>
    public void Init(Character p_character)
    {
        m_character = p_character;

        m_colldier = GetComponent<Collider>();
        if (m_colldier != null)
        {
            m_colldier.enabled = false;
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("No collider found on " + gameObject.name + ", this weapon will be ignored");
#endif
            Destroy(gameObject);
        }    
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
