using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrigger : MonoBehaviour
{
    public float m_lightDamage = 0.0f;
    public float m_heavyDamage = 0.0f;

    private const int HITMARKER_COUNT = 5;

    private enum WEAPON_STATE {WINDUP, ATTACK, COOLOFF}
    
    private WEAPON_STATE m_currentState = WEAPON_STATE.WINDUP;
    
    private Collider m_colldier = null;
    private Character m_character = null;

    private float m_damageStart = 0.0f;
    private float m_damageEnd = 0.0f;

    private CustomAnimation.ATTACK_STANCE m_currentStance = CustomAnimation.ATTACK_STANCE.LIGHT;

    private float m_damageInterval = 0.0f;
    private float m_damageNextInterval = 0.0f;

    private List<Character> m_hitCharacters = new List<Character>();

    /// <summary>
    /// Setup varibles for later use
    /// </summary>
    /// <param name="p_character">Parent character</param>
    public void Init(Character p_character)
    {
        m_character = p_character;

        m_colldier = GetComponent<Collider>();
        ToggleTrigger(false);
    }

    /// <summary>
    /// Start a weapons manoeuvre
    /// Setup all required varibles
    /// </summary>
    /// <param name="p_damageStart">When damage wil start given a percentage 0-1</param>
    /// <param name="p_damageEnd">When damage wil end given a percentage 0-1</param>
    /// <param name="p_currentStance">Current weapon stance</param>
    public void StartManoeuvre(float p_damageStart, float p_damageEnd, CustomAnimation.ATTACK_STANCE p_currentStance)
    {
        m_damageStart = p_damageStart;
        m_damageEnd = p_damageEnd;
        
        m_currentStance = p_currentStance;

        m_damageNextInterval = m_damageStart;
        m_damageInterval = (p_damageEnd - p_damageStart) / HITMARKER_COUNT;

        m_currentState = WEAPON_STATE.WINDUP;

        m_hitCharacters.Clear();
    }

    /// <summary>
    /// Update a weapons manouevre
    /// Change to different states based off when it should deal damage
    /// </summary>
    /// <param name="p_animationPercent">Current animation percentage</param>
    public void UpdateManoeuvre(float p_animationPercent)
    {
        switch (m_currentState)
        {
            case WEAPON_STATE.WINDUP:
                if (p_animationPercent > m_damageStart)
                {
                    ToggleTrigger(true);
                    m_currentState = WEAPON_STATE.ATTACK;
                }
                break;
            case WEAPON_STATE.ATTACK:
                DealDamage(p_animationPercent);
                if (p_animationPercent > m_damageEnd)
                {
                    ToggleTrigger(false);
                    m_currentState = WEAPON_STATE.COOLOFF;
                }
                break;
            case WEAPON_STATE.COOLOFF:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Deal damage to all chraters that are in wepaon range
    /// </summary>
    /// <param name="p_animationPercent">Current aniamtion percentage</param>
    private void DealDamage(float p_animationPercent)
    {
        if(m_damageNextInterval < p_animationPercent)
        {
            m_damageNextInterval += m_damageInterval;

            float damage = m_currentStance == CustomAnimation.ATTACK_STANCE.LIGHT ? m_lightDamage / HITMARKER_COUNT : m_heavyDamage / HITMARKER_COUNT;

            foreach (Character character in m_hitCharacters)
            {
                m_character.DealDamage(damage, character);
            }
        }
    }

    /// <summary>
    /// Set colldier state
    /// </summary>
    /// <param name="p_val">true for enabled</param>
    public void ToggleTrigger(bool p_val)
    {
        m_colldier.enabled = p_val;
    }

    /// <summary>
    /// Object trigger enter
    /// Add to list of characters to damage if valid target
    /// </summary>
    /// <param name="other">Other collider</param>
    private void OnTriggerEnter(Collider other)
    {
        Character character = other.GetComponent<Character>();

        if(character != null && character != m_character && m_character.m_team != character.m_team) //Is chaarcter collider, not parent, different team
        {
            if (!m_hitCharacters.Contains(character))
                m_hitCharacters.Add(character);
        }
    }

    /// <summary>
    /// Object trigger exit
    /// Remove from the list of characters to damage
    /// </summary>
    /// <param name="other">Other collider</param>
    private void OnTriggerExit(Collider other)
    {
        Character character = other.GetComponent<Character>();

        if (character != null) //Is chaarcter collider
        {
            m_hitCharacters.Remove(character);
        }
    }

}
