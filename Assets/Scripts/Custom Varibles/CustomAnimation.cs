using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation : MonoBehaviour
{
    public const float END_ANIMATION_TIME = 0.99f;
    public const int IDLE_COUNT = 4;

    private static string m_nullString = "Null";

    //Base animations
    public enum ANIM_TYPE {INTERRUPT, LOCOMOTION, ATTACK }
    private static Dictionary<ANIM_TYPE, string> m_animTypeToString = new Dictionary<ANIM_TYPE, string>();

    //Interrupt
    public enum INTERRUPT_ANIM {DEATH, KNOCKBACK, RECOIL}
    private static Dictionary<INTERRUPT_ANIM, string> m_interruptAnimToString = new Dictionary<INTERRUPT_ANIM, string>();
    //Locomotion
    public enum LOCOMOTION_ANIM {NULL, IDLE, DODGE, JUMP, DOUBLE_JUMP, IN_AIR, LAND, WALL_GRAB, WALL_FLIP, ROLL, BLOCK}
    private static Dictionary<LOCOMOTION_ANIM, string> m_locomotionAnimToString = new Dictionary<LOCOMOTION_ANIM, string>();

    //Attack
    public enum ATTACK_STANCE { LIGHT, HEAVY }
    private static Dictionary<ATTACK_STANCE, string> m_attackStanceToString = new Dictionary<ATTACK_STANCE, string>();
    public enum ATTACK_TYPE { GROUND, IN_AIR, SPRINTING }
    private static Dictionary<ATTACK_TYPE, string> m_attackTypeToString = new Dictionary<ATTACK_TYPE, string>();

    public enum VARIBLE_ANIM {CURRENT_VELOCITY, DESIRED_VELOCITY, ABSOLUTE_VELOCITY, RANDOM_IDLE, WEAPON}
    private static Dictionary<VARIBLE_ANIM, string> m_varibleAnimToString = new Dictionary<VARIBLE_ANIM, string>();

    /// <summary>
    /// setup dicionaries used
    /// </summary>
    static CustomAnimation()
    {
        m_animTypeToString.Add(ANIM_TYPE.INTERRUPT, "Interrupt");
        m_animTypeToString.Add(ANIM_TYPE.LOCOMOTION, "Loco");
        m_animTypeToString.Add(ANIM_TYPE.ATTACK, "Attack");

        m_interruptAnimToString.Add(INTERRUPT_ANIM.DEATH, "Death");
        m_interruptAnimToString.Add(INTERRUPT_ANIM.KNOCKBACK, "Knockback");
        m_interruptAnimToString.Add(INTERRUPT_ANIM.RECOIL, "Recoil");

        m_locomotionAnimToString.Add(LOCOMOTION_ANIM.IDLE, "Idle");
        m_locomotionAnimToString.Add(LOCOMOTION_ANIM.DODGE, "Dodge");
        m_locomotionAnimToString.Add(LOCOMOTION_ANIM.JUMP, "Jump");
        m_locomotionAnimToString.Add(LOCOMOTION_ANIM.DOUBLE_JUMP, "DoubleJump");
        m_locomotionAnimToString.Add(LOCOMOTION_ANIM.IN_AIR, "InAir");
        m_locomotionAnimToString.Add(LOCOMOTION_ANIM.LAND, "Land");
        m_locomotionAnimToString.Add(LOCOMOTION_ANIM.WALL_GRAB, "WallGrab");
        m_locomotionAnimToString.Add(LOCOMOTION_ANIM.WALL_FLIP, "WallFlip");
        m_locomotionAnimToString.Add(LOCOMOTION_ANIM.ROLL, "Roll");
        m_locomotionAnimToString.Add(LOCOMOTION_ANIM.BLOCK, "Block");

        m_attackStanceToString.Add(ATTACK_STANCE.LIGHT, "Light");
        m_attackStanceToString.Add(ATTACK_STANCE.HEAVY, "Heavy");

        m_attackTypeToString.Add(ATTACK_TYPE.GROUND, "Ground");
        m_attackTypeToString.Add(ATTACK_TYPE.IN_AIR, "InAir");
        m_attackTypeToString.Add(ATTACK_TYPE.SPRINTING, "Sprint");

        m_varibleAnimToString.Add(VARIBLE_ANIM.CURRENT_VELOCITY, "CurrentVelocity");
        m_varibleAnimToString.Add(VARIBLE_ANIM.DESIRED_VELOCITY, "DesiredVelocity");
        m_varibleAnimToString.Add(VARIBLE_ANIM.ABSOLUTE_VELOCITY, "AbsoluteVelocity");
        m_varibleAnimToString.Add(VARIBLE_ANIM.RANDOM_IDLE, "RandomIdle");
        m_varibleAnimToString.Add(VARIBLE_ANIM.WEAPON, "WeaponInt");
    }

    /// <summary>
    /// Constuct interrupt animation string for animator
    /// </summary>
    /// <param name="p_interruptAnim">animation that is desired</param>
    /// <returns>Constructed string followiong naming convention</returns>
    public static string GetInterrupt(INTERRUPT_ANIM p_interruptAnim)
    {
        return m_animTypeToString[ANIM_TYPE.INTERRUPT] + "_" + m_interruptAnimToString[p_interruptAnim];
    }

    /// <summary>
    /// Constuct locomotion animation string for animator
    /// </summary>
    /// <param name="p_locomotionAnim">animation that is desired</param>
    /// <returns>Constructed string followiong naming convention</returns>
    public static string GetLocomotion(LOCOMOTION_ANIM p_locomotionAnim)
    {
        return m_animTypeToString[ANIM_TYPE.LOCOMOTION] + "_" + m_locomotionAnimToString[p_locomotionAnim];
    }

    /// <summary>
    /// Constuct attacking animation string for animator    
    /// </summary>
    /// <param name="p_atackStance">Stance to be using light or heavy</param>
    /// <param name="p_attackType">The attacking type</param>
    /// <param name="p_comboIndex">In the case of combos between 0 and 3</param>
    /// <returns>Constructed string followiong naming convention</returns>
    public static string GetAttack(ATTACK_TYPE p_attackType, ATTACK_STANCE p_atackStance, int p_comboIndex)
    {
        p_comboIndex = Mathf.Clamp(p_comboIndex, 0, 3);
        return m_animTypeToString[ANIM_TYPE.ATTACK] + "_" + m_attackTypeToString[p_attackType] + "_" + m_attackStanceToString[p_atackStance] + p_comboIndex;
    }

    /// <summary>
    /// Constuct varible string for animator
    /// </summary>
    /// <param name="p_varibleAnim">animation that is desired</param>
    /// <returns>Constructed string followiong naming convention</returns>
    public static string GetVarible(VARIBLE_ANIM p_varibleAnim)
    {
        return m_varibleAnimToString[p_varibleAnim];
    }

    /// <summary>
    /// Set the varible value 
    /// </summary>
    /// <param name="p_animator">Animator to play on</param>
    /// <param name="p_animString">Animation string</param>
    /// <param name="p_value">Value of varible to set to</param>
    public static void SetVarible(Animator p_animator, string p_animString, float p_value)
    {
        p_animator.SetFloat(p_animString, p_value);
    }


    /// <summary>
    /// Get the current normalized time of the animation
    /// </summary>
    /// <param name="p_animator">Animator to check against</param>
    /// <returns>Animator normalised time, defaults to 0.0f</returns>
    public static float GetAnimationPercent(Animator p_animator)
    {
        if (p_animator == null)
            return 0.0f;

        return p_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    /// <summary>
    /// Determine if animation is done
    /// </summary>
    /// <param name="p_animator">Animator to check against</param>
    /// <returns>True when normalized time is great than END_ANIMATION_TIME, defaults to false</returns>
    public static bool IsAnimationDone(Animator p_animator)
    {
        if (p_animator == null)
            return false;

        return p_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > END_ANIMATION_TIME;
    }

    /// <summary>
    /// Play a given animation
    /// </summary>
    /// <param name="p_animator">Animator to play on</param>
    /// <param name="p_animString">Animation string</param>
    public static void PlayAnimtion(Animator p_animator, string p_animString)
    {
        p_animator.SetLayerWeight(1, 0.0f);
        p_animator.Play(p_animString, 0);
    }

    /// <summary>
    /// End of animation, return to null on layer 0
    /// </summary>
    /// <param name="p_animator">Animator to default back to null</param>
    public static void EndAnimation(Animator p_animator)
    {
        p_animator.SetLayerWeight(1, 1.0f);
        p_animator.Play(m_nullString, 0);
    }
}
