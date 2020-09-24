using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManoeuvreHitbox : MonoBehaviour
{
    private Character m_character = null;
    private float m_damage = 0.0f;

    private BoxCollider m_collider = null;

    private List<Character> m_hitCharacters = new List<Character>();

    /// <summary>
    /// Initialise hitbox
    /// </summary>
    /// <param name="p_character">Character controlling hitbox</param>
    /// <param name="p_damage">Damage this attack will do per hit box</param>
    public void Init(Character p_character, float p_damage)
    {
        m_character = p_character;
        m_damage = p_damage;

        m_collider = GetComponent<BoxCollider>();
    }

    /// <summary>
    /// Start of manouvre
    /// Enable collider
    /// Clear old list of hit characters
    /// </summary>
    public void StartOfManouvre()
    {
        m_collider.enabled = true;
        m_hitCharacters.Clear();
    }

    /// <summary>
    /// End of attakc manouvre
    /// Disable collider
    /// </summary>
    public void EndOfManouvre()
    {
        m_collider.enabled = false;
    }

    /// <summary>
    /// Object trigger enter
    /// Add to list of characters to damage if valid target
    /// </summary>
    /// <param name="other">Other collider</param>
    private void OnTriggerEnter(Collider other)
    {
        Character otherCharacter = other.GetComponent<Character>();

        if (otherCharacter != null && otherCharacter != m_character && m_character.m_team != otherCharacter.m_team) //Is character collider, not parent, different team
        {
            if (!m_hitCharacters.Contains(otherCharacter))
            {
                //TODO get proper damage calc
                m_character.DealDamage(m_damage, otherCharacter);
                m_hitCharacters.Add(otherCharacter);
            }
        }
    }

}
