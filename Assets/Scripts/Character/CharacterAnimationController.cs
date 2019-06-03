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

    //-------------------
    //Initilise controller, setup dicionaries used
    //-------------------
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

        m_parentCharacter = transform.parent.GetComponent<Character>();

        m_animator = GetComponentInChildren<Animator>();
    }

    //-------------------
    //Set animator boolean values
    //
    //Param p_animation: Dictionary animation enum, used to convert to string values
    //      p_val: boolean value to set in animator
    //-------------------
    public void SetBool(ANIMATIONS p_animation, bool p_val)
    {
        m_currentAnimation = p_animation;

        m_animator.SetBool(m_animationStringDicitonary[p_animation], p_val);
    }

    //-------------------
    //Set animator float values
    //
    //Param p_varible: Dictionary animation enum, used to convert to string values
    //      p_val: float value to set in animator
    //-------------------
    public void SetVarible(VARIBLES p_varible, float p_val)
    {
        m_animator.SetFloat(m_varibleStringDicitonary[p_varible], p_val);
    }

    //-------------------
    //Determine if it is currently the end of the animation
    //Return bool: true when normalized time is greater than 1
    //-------------------
    public bool EndOfAnimation()
    {
        return m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1;
    }

    //-------------------
    //Enable damage for the weapon
    //-------------------
    public void EnableDamage()
    {
        m_parentCharacter.ToggleWeapon(true);
    }

    //-------------------
    //Disable damage for the weapon
    //-------------------
    public void DisableDamage()
    {
        m_parentCharacter.ToggleWeapon(false);
    }

    //-------------------
    //Set combo boolean varible to true
    //-------------------
    public void EnableComboAttack()
    {
        m_canCombo = true;
    }

    //-------------------
    //Set combo boolean varible to false
    //-------------------
    public void DisableComboAttack()
    {
        m_canCombo = false;
    }
}
