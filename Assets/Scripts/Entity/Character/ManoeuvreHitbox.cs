using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManoeuvreHitbox : MonoBehaviour
{
    private const string HITBOX_ANIMATION = "Hitbox Animation";

    private Character m_character = null;

    private BoxCollider m_collider = null;

    private List<Character> m_hitCharacters = new List<Character>();

    private Manoeuvre m_parentController = null;
    private Animator m_animator = null;

    /// <summary>
    /// Initialise hitbox
    /// </summary>
    /// <param name="p_character">Character controlling hitbox</param>
    /// <param name="p_damage">Damage this attack will do per hit box</param>
    public void Init(Character p_character, Manoeuvre p_parentController)
    {
        m_character = p_character;
        m_parentController = p_parentController;

        m_collider = GetComponent<BoxCollider>();

        m_animator = GetComponent<Animator>();

        DisableHitbox(); //Used to setup intial state
    }

    /// <summary>
    /// Start of manouvre
    /// Enable collider
    /// Clear old list of hit characters
    /// </summary>
    public void EnableHitbox()
    {
        gameObject.SetActive(true);
        m_hitCharacters.Clear();

        m_animator.Play(HITBOX_ANIMATION, 0, 0.0f);
    }

    /// <summary>
    /// End of attakc manouvre
    /// Disable collider
    /// </summary>
    public void DisableHitbox()
    {
        gameObject.SetActive(false);
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
                Vector3 otherColliderCenter = other.bounds.center;

                Vector3 collisionPoint = otherColliderCenter; //Using idea of x,z centered on other colldier, so spawnging inside object
                collisionPoint.y = m_collider.bounds.center.y; //y is the current colliders y position.

                m_character.DealDamage(m_parentController, otherCharacter, collisionPoint);
                m_hitCharacters.Add(otherCharacter);
            }
        }
    }

}
