using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public enum ANIMATIONS {LIGHT_ATTACK, HEAVY_ATTACK, ALT_ATTACK, DODGE, JUMP, IN_AIR}
    public Dictionary<ANIMATIONS, string> m_boolDictionary = new Dictionary<ANIMATIONS, string>();

    public enum VARIBLES {CURRENT_VELOCITY, ABSOLUTE_VELOCTIY, DESIRED_VELOCITY, RANDOM_IDLE}
    public Dictionary<VARIBLES, string> m_floatDicitonary = new Dictionary<VARIBLES, string>();

    public Animator m_animator = null;

    /// <summary>
    /// setup dicionaries used
    /// </summary>
    public void InitAnimationController()
    {
        m_boolDictionary.Add(ANIMATIONS.LIGHT_ATTACK, "LightAttack");
        m_boolDictionary.Add(ANIMATIONS.HEAVY_ATTACK, "HeavyAttack");
        m_boolDictionary.Add(ANIMATIONS.ALT_ATTACK, "AltAttack");

        m_boolDictionary.Add(ANIMATIONS.DODGE, "Dodge");
        m_boolDictionary.Add(ANIMATIONS.JUMP, "Jump");
        m_boolDictionary.Add(ANIMATIONS.IN_AIR, "InAir");

        m_floatDicitonary.Add(VARIBLES.CURRENT_VELOCITY, "CurrentVelocity");
        m_floatDicitonary.Add(VARIBLES.ABSOLUTE_VELOCTIY, "AbsoluteVelocity");
        m_floatDicitonary.Add(VARIBLES.DESIRED_VELOCITY, "DesiredVelocity");
        m_floatDicitonary.Add(VARIBLES.RANDOM_IDLE, "RandomIdleVal");

        m_animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Set animator boolean values
    /// </summary>
    /// <param name="p_val">boolean value to set in animator</param>
    public void SetBool(ANIMATIONS p_animation, bool p_val)
    {
        m_animator.SetBool(m_boolDictionary[p_animation], p_val);
    }

    /// <summary>
    /// Set animator float values
    /// </summary>
    /// <param name="p_varible">Dictionary animation enum, used to convert to string values</param>
    /// <param name="p_val">float value to set in animator</param>
    public void SetVarible(VARIBLES p_varible, float p_val)
    {
        m_animator.SetFloat(m_floatDicitonary[p_varible], p_val);
    }

    /// <summary>
    /// Determine if it is currently the end of the animation
    /// </summary>
    /// <returns> true when normalized time is greater than 1</returns>
    public bool EndOfAnimation()
    {
        return m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !m_animator.IsInTransition(0);
    }

    /// <summary>
    /// Get the normalzed time for the animation
    /// </summary>
    /// <returns>Normalized time, 0-1</returns>
    public float GetNormalizedTime()
    {
        return m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f;
    }
}
