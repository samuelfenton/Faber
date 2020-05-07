using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrigger : MonoBehaviour
{
    public float m_lightDamage = 0.0f;
    public float m_heavyDamage = 0.0f;

    private enum WEAPON_STATE {WINDUP, ATTACK, COOLOFF}
    
    private WEAPON_STATE m_currentState = WEAPON_STATE.WINDUP;
    
    private Collider m_colldier = null;
    private Character m_character = null;

    private float m_damageStart = 0.0f;
    private float m_damageEnd = 0.0f;

    private CustomAnimation_Humanoid.ATTACK_STANCE m_currentStance = CustomAnimation_Humanoid.ATTACK_STANCE.LIGHT;

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
    public void StartManoeuvre(float p_damageStart, float p_damageEnd, CustomAnimation_Humanoid.ATTACK_STANCE p_currentStance)
    {
        m_damageStart = p_damageStart;
        m_damageEnd = p_damageEnd;
        
        m_currentStance = p_currentStance;

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
    /// <param name="p_character">Chaarcter to deal damge to</param>
    private void DealDamage(Character p_character)
    {
        float damage = m_currentStance == CustomAnimation_Humanoid.ATTACK_STANCE.LIGHT ? m_lightDamage  : m_heavyDamage;

        m_character.DealDamage(damage, p_character);
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
            {
                DealDamage(character);
                m_hitCharacters.Add(character);
            }
        }
    }
}
