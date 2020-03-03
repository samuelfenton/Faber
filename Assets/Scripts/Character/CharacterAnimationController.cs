using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public enum ANIMATIONS {LOCOMOTION, LIGHT_ATTACK, HEAVY_ATTACK, LIGHT_ATTACK_COMBO, HEAVY_ATTACK_COMBO, DODGE, JUMP, IN_AIR, LAND}
    public Dictionary<ANIMATIONS, string> m_animationStringDicitonary = new Dictionary<ANIMATIONS, string>();

    public enum VARIBLES {WEAPON_SLOT, MOVEMENT_SPEED}
    public Dictionary<VARIBLES, string> m_varibleStringDicitonary = new Dictionary<VARIBLES, string>();

    private Character m_parentCharacter = null;

    public bool m_canCombo = false;
    public ANIMATIONS m_currentAnimation = ANIMATIONS.LOCOMOTION;
    public Animator m_animator = null;

    /// <summary>
    /// setup dicionaries used
    /// </summary>
    public void InitAnimationController()
    {
        m_animationStringDicitonary.Add(ANIMATIONS.LOCOMOTION, "Locomotion");
        m_animationStringDicitonary.Add(ANIMATIONS.LIGHT_ATTACK, "LightAttack");
        m_animationStringDicitonary.Add(ANIMATIONS.HEAVY_ATTACK, "HeavyAttack");
        m_animationStringDicitonary.Add(ANIMATIONS.LIGHT_ATTACK_COMBO, "LightAttackCombo");
        m_animationStringDicitonary.Add(ANIMATIONS.HEAVY_ATTACK_COMBO, "HeavyAttackCombo");
        m_animationStringDicitonary.Add(ANIMATIONS.DODGE, "Dodge");
        m_animationStringDicitonary.Add(ANIMATIONS.JUMP, "Jump");
        m_animationStringDicitonary.Add(ANIMATIONS.IN_AIR, "InAir");
        m_animationStringDicitonary.Add(ANIMATIONS.LAND, "Land");

        m_varibleStringDicitonary.Add(VARIBLES.WEAPON_SLOT, "WeaponSlot");
        m_varibleStringDicitonary.Add(VARIBLES.MOVEMENT_SPEED, "Speed");

        m_parentCharacter = GetComponent<Character>();

        m_animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Set animator boolean values
    /// </summary>
    /// <param name="p_animation">Dictionary animation enum, used to convert to string values</param>
    /// <param name="p_val">boolean value to set in animator</param>
    public void SetBool(ANIMATIONS p_animation, bool p_val)
    {
        m_currentAnimation = p_animation;

        m_animator.SetBool(m_animationStringDicitonary[p_animation], p_val);
    }

    /// <summary>
    /// Set animator float values
    /// </summary>
    /// <param name="p_varible">Dictionary animation enum, used to convert to string values</param>
    /// <param name="p_val">float value to set in animator</param>
    public void SetVarible(VARIBLES p_varible, float p_val)
    {
        m_animator.SetFloat(m_varibleStringDicitonary[p_varible], p_val);
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
    /// Enable damage for the weapon
    /// </summary>
    public void EnableDamage()
    {
        m_parentCharacter.ToggleWeapon(true);
    }

    /// <summary>
    /// Disable damage for the weapon
    /// </summary>
    public void DisableDamage()
    {
        m_parentCharacter.ToggleWeapon(false);
    }

    /// <summary>
    /// Set combo boolean varible to true
    /// </summary>
    public void EnableComboAttack()
    {
        m_canCombo = true;
    }

    /// <summary>
    /// Set combo boolean varible to false
    /// </summary>
    public void DisableComboAttack()
    {
        m_canCombo = false;
    }
}
