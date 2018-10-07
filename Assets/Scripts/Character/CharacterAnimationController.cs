using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public enum ANIMATION {IDLE, RUN, LIGHT_ATTACK, HEAVY_ATTACK, DODGE, JUMP, IN_AIR, NONE}
    public Dictionary<ANIMATION, string> m_animationStringDicitonary = new Dictionary<ANIMATION, string>();

    private Character m_parentCharacter = null;
    public bool m_currentlyAnimating = false;
    public ANIMATION m_currentAnimation = ANIMATION.NONE;
    public Animator m_animator = null;

    private void Start()
    {
        m_animationStringDicitonary.Add(ANIMATION.IDLE, "Idle");
        m_animationStringDicitonary.Add(ANIMATION.RUN, "Run");
        m_animationStringDicitonary.Add(ANIMATION.LIGHT_ATTACK, "LightAttack");
        m_animationStringDicitonary.Add(ANIMATION.HEAVY_ATTACK, "HeavyAttack");
        m_animationStringDicitonary.Add(ANIMATION.DODGE, "Dodge");
        m_animationStringDicitonary.Add(ANIMATION.JUMP, "Jump");
        m_animationStringDicitonary.Add(ANIMATION.IN_AIR, "InAir");

        m_parentCharacter = transform.parent.GetComponent<Character>();

        m_animator = GetComponentInChildren<Animator>();
    }

    public void PlayAnimation(ANIMATION p_animation)
    {
        if (m_currentAnimation != ANIMATION.NONE) 
            m_animator.SetBool(m_animationStringDicitonary[m_currentAnimation], false);
        m_currentAnimation = p_animation;
        m_animator.SetBool(m_animationStringDicitonary[m_currentAnimation], true);
        m_currentlyAnimating = true;
    }

    public void EndOfAnimation()
    {
        m_currentlyAnimating = false;
    }

    public void DealDamage()
    {
        m_parentCharacter.DealDamage();
    }
}
