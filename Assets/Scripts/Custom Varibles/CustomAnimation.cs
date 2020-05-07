using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation : MonoBehaviour
{
    //Base animations
    public enum ANIM_TYPE { INTERRUPT, LOCOMOTION, ATTACK }
    protected Dictionary<ANIM_TYPE, string> m_animTypeToString = new Dictionary<ANIM_TYPE, string>();

    //Attack
    public enum ATTACK_STANCE { LIGHT, HEAVY }
    protected Dictionary<ATTACK_STANCE, string> m_attackStanceToString = new Dictionary<ATTACK_STANCE, string>();
    public enum ATTACK_TYPE { GROUND, IN_AIR, SPRINTING }
    protected Dictionary<ATTACK_TYPE, string> m_attackTypeToString = new Dictionary<ATTACK_TYPE, string>();

    //Used Varibles
    public enum VARIBLE_ANIM { CURRENT_VELOCITY, DESIRED_VELOCITY, ABSOLUTE_VELOCITY, RANDOM_IDLE, WEAPON }
    protected Dictionary<VARIBLE_ANIM, string> m_varibleAnimToString = new Dictionary<VARIBLE_ANIM, string>();

    public const float CROSSFADE_TIME = 0.1f;

    public const float END_ANIMATION_TIME = 0.99f;
    public const int IDLE_COUNT = 4;

    protected string m_baseLayer = "BaseLayer";
    protected string m_locomotionLayer = "LocomotionLayer";
    protected string m_nullString = "Null";

    protected int m_locomotionIndex = 0;
    protected int m_baseIndex = 0;

    protected bool m_crossfadeFlag = false;
    protected bool m_fadeoutFlag = false;

    protected Coroutine m_fadeoutCoroutine = null;

    protected Animator m_animator = null;

    /// <summary>
    /// Setup dicionaries used
    /// </summary>
    /// <param name="p_animator">Animator to check against</param>
    public virtual void Init(Animator p_animator)
    {
        m_animator = p_animator;

        m_locomotionIndex = m_animator.GetLayerIndex(m_locomotionLayer);
        m_baseIndex = m_animator.GetLayerIndex(m_baseLayer);


        m_animTypeToString.Add(ANIM_TYPE.INTERRUPT, "Interrupt");
        m_animTypeToString.Add(ANIM_TYPE.LOCOMOTION, "Loco");
        m_animTypeToString.Add(ANIM_TYPE.ATTACK, "Attack");

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
    /// <param name="p_animString">Animation string</param>
    /// <param name="p_value">Value of varible to set to</param>
    public void SetVarible(string p_animString, float p_value)
    {
        m_animator.SetFloat(p_animString, p_value);
    }


    /// <summary>
    /// Get the current normalized time of the animation
    /// </summary>
    /// <returns>Animator normalised time, defaults to 0.0f</returns>
    public float GetAnimationPercent()
    {
        if (m_animator == null)
            return 0.0f;

        return m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    /// <summary>
    /// Determine if animation is done
    /// </summary>
    /// <returns>True when normalized time is great than END_ANIMATION_TIME, defaults to false</returns>
    public bool IsAnimationDone()
    {
        if (m_animator == null)
            return false;

        return m_animator.GetCurrentAnimatorStateInfo(m_baseIndex).normalizedTime > END_ANIMATION_TIME && !m_crossfadeFlag;
    }

    /// <summary>
    /// Play a given animation
    /// </summary>
    /// <param name="p_animString">Animation string</param>
    public void PlayAnimation(string p_animString)
    {
        if (!m_crossfadeFlag) //Early breakout if ever trying to cross fade
        {
            if(m_fadeoutFlag) //Check if already fading out
            {
                m_fadeoutFlag = false;
                if (m_fadeoutCoroutine != null)
                    StopCoroutine(m_fadeoutCoroutine);
            }

            StartCoroutine(CrossFadeInto(p_animString));
        }
    }

    /// <summary>
    /// End of animation, return to null on layer 0
    /// </summary>
    public void EndAnimation()
    {
        if (!m_crossfadeFlag && !m_fadeoutFlag) //Early breakout if ever trying to cross fade
        {
            m_fadeoutCoroutine = StartCoroutine(CrossFadeOut());
        }
    }

    /// <summary>
    /// Cross fade into animation from idle
    /// </summary>
    /// <param name="p_animString">Animation string</param>
    /// <returns>waits till end of frame repeativly till timer is reached</returns>
    public IEnumerator CrossFadeInto(string p_animString)
    {
        m_crossfadeFlag = true;
        float locomotionWeight = m_animator.GetLayerWeight(m_locomotionIndex);

        float crossfadeTimer = (1 - locomotionWeight) * CROSSFADE_TIME; //Start where weight already is

        m_animator.CrossFade(p_animString, CROSSFADE_TIME - crossfadeTimer, m_baseIndex);

        while (crossfadeTimer < CROSSFADE_TIME)
        {
            m_animator.SetLayerWeight(m_locomotionIndex, 1 - crossfadeTimer / CROSSFADE_TIME);

            yield return null;

            crossfadeTimer += Time.deltaTime;
        }

        m_animator.Play(p_animString, m_baseIndex, m_animator.GetCurrentAnimatorStateInfo(m_baseIndex).normalizedTime);

        m_animator.SetLayerWeight(m_locomotionIndex, 0.0f);
        m_crossfadeFlag = false;
    }

    /// <summary>
    /// Cross fade out of action into jsut locomotion
    /// </summary>
    /// <returns>Fade back into locomotion</returns>
    public IEnumerator CrossFadeOut()
    {
        m_fadeoutFlag = true;
        float locomotionWeight = m_animator.GetLayerWeight(m_locomotionIndex);

        float crossfadeTimer = locomotionWeight * CROSSFADE_TIME; //Start where weight already is

        while (crossfadeTimer < CROSSFADE_TIME)
        {
            m_animator.SetLayerWeight(m_locomotionIndex, crossfadeTimer / CROSSFADE_TIME);
            crossfadeTimer += Time.deltaTime;

            yield return null;

            if (m_crossfadeFlag) //Early breakout if ever trying to cross fade back in
            {
                yield break;
            }
        }
        m_animator.Play(m_nullString, m_baseIndex);

        m_animator.SetLayerWeight(m_locomotionIndex, 1.0f);
        m_fadeoutFlag = false;
    }
}
