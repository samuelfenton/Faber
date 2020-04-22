using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation : MonoBehaviour
{
    public const float CROSSFADE_TIME = 0.1f;

    public const float END_ANIMATION_TIME = 0.99f;
    public const int IDLE_COUNT = 4;

    private static string m_baseLayer = "BaseLayer";
    private static string m_locomotionLayer = "LocomotionLayer";
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

    private static CustomAnimation m_Instance;

    private bool m_crossfadeFlag = false;

    /// <summary>
    /// Access singleton instance through this propriety.
    /// </summary>
    public static CustomAnimation Instance
    {
        get
        {
            if (m_Instance == null)
            {
                // Search for existing instance.
                m_Instance = FindObjectOfType<CustomAnimation>();

                // Create new instance if one doesn't already exist.
                if (m_Instance == null)
                {
                    // Need to create a new GameObject to attach the singleton to.
                    GameObject singletonObject = new GameObject();
                    m_Instance = singletonObject.AddComponent<CustomAnimation>();
                    singletonObject.name = "Custom Animation Object";

                    m_Instance.InitStrings();

                    // Make instance persistent.
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return m_Instance;
        }
    }

    /// <summary>
    /// setup dicionaries used
    /// </summary>
    private void InitStrings()
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
    public string GetInterrupt(INTERRUPT_ANIM p_interruptAnim)
    {
        return m_animTypeToString[ANIM_TYPE.INTERRUPT] + "_" + m_interruptAnimToString[p_interruptAnim];
    }

    /// <summary>
    /// Constuct locomotion animation string for animator
    /// </summary>
    /// <param name="p_locomotionAnim">animation that is desired</param>
    /// <returns>Constructed string followiong naming convention</returns>
    public string GetLocomotion(LOCOMOTION_ANIM p_locomotionAnim)
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
    public string GetAttack(ATTACK_TYPE p_attackType, ATTACK_STANCE p_atackStance, int p_comboIndex)
    {
        p_comboIndex = Mathf.Clamp(p_comboIndex, 0, 3);
        return m_animTypeToString[ANIM_TYPE.ATTACK] + "_" + m_attackTypeToString[p_attackType] + "_" + m_attackStanceToString[p_atackStance] + p_comboIndex;
    }

    /// <summary>
    /// Constuct varible string for animator
    /// </summary>
    /// <param name="p_varibleAnim">animation that is desired</param>
    /// <returns>Constructed string followiong naming convention</returns>
    public string GetVarible(VARIBLE_ANIM p_varibleAnim)
    {
        return m_varibleAnimToString[p_varibleAnim];
    }

    /// <summary>
    /// Set the varible value 
    /// </summary>
    /// <param name="p_animator">Animator to play on</param>
    /// <param name="p_animString">Animation string</param>
    /// <param name="p_value">Value of varible to set to</param>
    public void SetVarible(Animator p_animator, string p_animString, float p_value)
    {
        p_animator.SetFloat(p_animString, p_value);
    }


    /// <summary>
    /// Get the current normalized time of the animation
    /// </summary>
    /// <param name="p_animator">Animator to check against</param>
    /// <returns>Animator normalised time, defaults to 0.0f</returns>
    public float GetAnimationPercent(Animator p_animator)
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
    public bool IsAnimationDone(Animator p_animator)
    {
        if (p_animator == null)
            return false;

        int baseIndex = p_animator.GetLayerIndex(m_baseLayer);

        return p_animator.GetCurrentAnimatorStateInfo(baseIndex).normalizedTime > END_ANIMATION_TIME && !m_crossfadeFlag;
    }

    /// <summary>
    /// Play a given animation
    /// </summary>
    /// <param name="p_animator">Animator to play on</param>
    /// <param name="p_animString">Animation string</param>
    public void PlayAnimation(Animator p_animator, string p_animString)
    {
        StartCoroutine(CrossFadeInto(p_animator, p_animString));
    }

    /// <summary>
    /// End of animation, return to null on layer 0
    /// </summary>
    /// <param name="p_animator">Animator to default back to null</param>
    public void EndAnimation(Animator p_animator)
    {
        int locomotionIndex = p_animator.GetLayerIndex(m_locomotionLayer);
        int baseIndex = p_animator.GetLayerIndex(m_baseLayer);

        p_animator.SetLayerWeight(locomotionIndex, 1.0f);

        p_animator.Play(m_nullString, baseIndex);

        m_crossfadeFlag = false;
    }

    /// <summary>
    /// Cross fade into animation from idle
    /// </summary>
    /// <param name="p_animator">Animator to play on</param>
    /// <param name="p_animString">Animation string</param>
    /// <returns>waits till end of frame repeativly till timer is reached</returns>
    public IEnumerator CrossFadeInto(Animator p_animator, string p_animString)
    {
        m_crossfadeFlag = true;
        
        float crossfadeTimer = 0.0f;
        int locomotionIndex = p_animator.GetLayerIndex(m_locomotionLayer);
        int baseIndex = p_animator.GetLayerIndex(m_baseLayer);

        p_animator.CrossFade(p_animString, CROSSFADE_TIME, baseIndex);
        
        while (crossfadeTimer < CROSSFADE_TIME)
        {
            p_animator.SetLayerWeight(locomotionIndex, 1 - crossfadeTimer / CROSSFADE_TIME);

            yield return null;

            crossfadeTimer += Time.deltaTime;

            if(!m_crossfadeFlag) //Early breakout
            {
                yield break;
            }
        }

        p_animator.Play(p_animString, baseIndex, CROSSFADE_TIME);

        p_animator.SetLayerWeight(locomotionIndex, 0.0f);
        m_crossfadeFlag = false;
    }
}
