using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public enum ANIMATIONS {LOCOMOTION, LIGHT_ATTACK, HEAVY_ATTACK, DODGE, JUMP, IN_AIR, LAND}
    public Dictionary<ANIMATIONS, string> m_animationStringDicitonary = new Dictionary<ANIMATIONS, string>();

    public enum VARIBLES {MOVEMENT_SPEED}
    public Dictionary<VARIBLES, string> m_varibleStringDicitonary = new Dictionary<VARIBLES, string>();

    public enum TRIGGERS { LIGHT_ATTACK_COMBO, HEAVY_ATTACK_COMBO }
    public Dictionary<TRIGGERS, string> m_triggerStringDicitonary = new Dictionary<TRIGGERS, string>();

    private Character m_parentCharacter = null;
    public bool m_currentlyAnimating = false;
    public bool m_canCombo = false;
    public ANIMATIONS m_currentAnimation = ANIMATIONS.LOCOMOTION;
    public Animator m_animator = null;

    private void Start()
    {
        m_animationStringDicitonary.Add(ANIMATIONS.LOCOMOTION, "Locomotion");
        m_animationStringDicitonary.Add(ANIMATIONS.LIGHT_ATTACK, "LightAttack");
        m_animationStringDicitonary.Add(ANIMATIONS.HEAVY_ATTACK, "HeavyAttack");
        m_animationStringDicitonary.Add(ANIMATIONS.DODGE, "Dodge");
        m_animationStringDicitonary.Add(ANIMATIONS.JUMP, "Jump");
        m_animationStringDicitonary.Add(ANIMATIONS.IN_AIR, "InAir");
        m_animationStringDicitonary.Add(ANIMATIONS.LAND, "Land");

        m_varibleStringDicitonary.Add(VARIBLES.MOVEMENT_SPEED, "Speed");

        m_triggerStringDicitonary.Add(TRIGGERS.LIGHT_ATTACK_COMBO, "LightAttackCombo");
        m_triggerStringDicitonary.Add(TRIGGERS.HEAVY_ATTACK_COMBO, "HeavyAttackCombo");

        m_parentCharacter = transform.parent.GetComponent<Character>();

        m_animator = GetComponentInChildren<Animator>();
    }

    public void SetBool(ANIMATIONS p_animation, bool p_val)
    {
        m_currentAnimation = p_animation;
        m_currentlyAnimating = p_val;

        m_animator.SetBool(m_animationStringDicitonary[p_animation], p_val);
    }

    public void SetTrigger(TRIGGERS p_trigger)
    {
        m_animator.SetTrigger(m_triggerStringDicitonary[p_trigger]);
    }

    public void SetVarible(VARIBLES p_varible, float p_val)
    {
        m_animator.SetFloat(m_varibleStringDicitonary[p_varible], p_val);
    }

    public void EndOfAnimation()
    {
        m_currentlyAnimating = false;
    }

    public void ComboAttack()
    {
        m_parentCharacter.PerformCombo();
    }

    public void DealDamage()
    {
        m_parentCharacter.DealDamage();
    }
}
