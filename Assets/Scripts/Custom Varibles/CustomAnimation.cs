using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation : MonoBehaviour
{
    public enum BLEND_TIME {INSTANT, SHORT, LONG }

    private const float END_ANIMATION_TIME = 0.9f;

    private const float INSTANT_BLEND_TIME = 0.05f;
    private const float SHORT_BLEND_TIME = 0.15f;
    private const float LONG_BLEND_TIME = 0.3f;

    public const string NULL_STRING = "Null";
    public const string END_ATTACK_BLEND = "EndAttackBlend";

    //Used Varibles
    public enum LAYER {BASE = 0, ATTACK, INTERRUPT, LAYER_COUNT}
    private int[] m_layerToInt = new int[(int)LAYER.LAYER_COUNT];

    public enum VARIBLE_FLOAT { CURRENT_VELOCITY, ABSOLUTE_VELOCITY, VERTICAL_VELOCITY, RANDOM_IDLE, KNOCKBACK_IMPACT, FLOAT_COUNT}
    private string[] m_floatToString = new string[(int)VARIBLE_FLOAT.FLOAT_COUNT];
    public enum BASE_DEFINES {LOCOMOTION, SPRINT, RUN_TO_SPRINT, DASH, JUMP, INAIR, DOUBLE_JUMP, INAIR_DASH, LANDING_TO_IDLE, LANDING_TO_RUN, BLOCK, BLOCK_FROM_IDLE, BLOCK_TO_IDLE, BASE_COUNT }
    private string[] m_baseToString = new string[(int)BASE_DEFINES.BASE_COUNT];

    public enum INTERRUPT_DEFINES {RECOIL, KNOCKBACK, DEATH, IDLE_EMOTE, INTERRUPT_COUNT}
    private string[] m_interruptToString = new string[(int)INTERRUPT_DEFINES.INTERRUPT_COUNT];

    private Animator m_animator = null;

    //Blending
    [HideInInspector]
    public bool m_currentlyBlending = false;

    private Coroutine m_blendCoroutine = null;
    private KeyValuePair<string, LAYER> m_blendingTo = new KeyValuePair<string, LAYER>();

    /// <summary>
    /// Setup dicionaries used
    /// </summary>
    /// <param name="p_animator">Animator to check against</param>
    public virtual void Init(Animator p_animator)
    {
        m_animator = p_animator;

        if (m_animator != null && p_animator.runtimeAnimatorController != null)
        {
            m_layerToInt[(int)LAYER.BASE] = m_animator.GetLayerIndex("BaseLayer");
            m_layerToInt[(int)LAYER.ATTACK] = m_animator.GetLayerIndex("AttackLayer");
            m_layerToInt[(int)LAYER.INTERRUPT] = m_animator.GetLayerIndex("InterruptLayer");
        }

        m_floatToString[(int)VARIBLE_FLOAT.CURRENT_VELOCITY] = ContainsParam(m_animator, "Current Velocity") ? "Current Velocity" : "";
        m_floatToString[(int)VARIBLE_FLOAT.ABSOLUTE_VELOCITY] = ContainsParam(m_animator, "Absolute Velocity") ? "Absolute Velocity" : "";
        m_floatToString[(int)VARIBLE_FLOAT.VERTICAL_VELOCITY] = ContainsParam(m_animator, "Vertical Velocity") ? "Vertical Velocity" : "";
        m_floatToString[(int)VARIBLE_FLOAT.RANDOM_IDLE] = ContainsParam(m_animator, "Random Idle") ? "Random Idle" : "";
        m_floatToString[(int)VARIBLE_FLOAT.KNOCKBACK_IMPACT] = ContainsParam(m_animator, "Knockback Impact") ? "Knockback Impact" : "";

        m_baseToString[(int)BASE_DEFINES.LOCOMOTION] = "Locomotion";
        m_baseToString[(int)BASE_DEFINES.SPRINT] = "Sprint";
        m_baseToString[(int)BASE_DEFINES.RUN_TO_SPRINT] = "RunToSprint";
        m_baseToString[(int)BASE_DEFINES.DASH] = "Dash";
        m_baseToString[(int)BASE_DEFINES.JUMP] = "Jump";
        m_baseToString[(int)BASE_DEFINES.INAIR] = "InAir";
        m_baseToString[(int)BASE_DEFINES.DOUBLE_JUMP] = "DoubleJump";
        m_baseToString[(int)BASE_DEFINES.INAIR_DASH] = "InAirDash";
        m_baseToString[(int)BASE_DEFINES.LANDING_TO_IDLE] = "LandingToIdle";
        m_baseToString[(int)BASE_DEFINES.LANDING_TO_RUN] = "LandingToRun";
        m_baseToString[(int)BASE_DEFINES.BLOCK] = "Block";
        m_baseToString[(int)BASE_DEFINES.BLOCK_FROM_IDLE] = "BlockFromIdle";
        m_baseToString[(int)BASE_DEFINES.BLOCK_TO_IDLE] = "BlockToIdle";

        m_interruptToString[(int)INTERRUPT_DEFINES.RECOIL]="Recoil";
        m_interruptToString[(int)INTERRUPT_DEFINES.KNOCKBACK] = "Knockback";
        m_interruptToString[(int)INTERRUPT_DEFINES.DEATH] = "Death";
        m_interruptToString[(int)INTERRUPT_DEFINES.IDLE_EMOTE] = "Idle Emote";
    }

    /// <summary>
    /// Set the varible value 
    /// </summary>
    /// <param name="p_floatVarible">Varible to set</param>
    /// <param name="p_value">Value of varible to set to</param>
    public void SetVaribleFloat(VARIBLE_FLOAT p_floatVarible, float p_value)
    {
        if(p_floatVarible != VARIBLE_FLOAT.FLOAT_COUNT && m_floatToString[(int)p_floatVarible] != "")
            m_animator.SetFloat(m_floatToString[(int)p_floatVarible], p_value);
    }

    /// <summary>
    /// Get the current normalized time of the animation
    /// </summary>
    /// <param name="p_layer">Layer to check</param>
    /// <returns>Animator normalised time, defaults to 0.0f</returns>
    public float GetAnimationPercent(LAYER p_layer)
    {
        if (m_animator == null || p_layer == LAYER.LAYER_COUNT)
            return 0.0f;

        return m_animator.GetCurrentAnimatorStateInfo(m_layerToInt[(int)p_layer]).normalizedTime;
    }

    /// <summary>
    /// Determine if animation is done
    /// </summary>
    /// <param name="p_layer">Layer to check</param>
    /// <returns>True when normalized time is great than END_ANIMATION_TIME, defaults to false</returns>
    public bool IsAnimationDone(LAYER p_layer)
    {
        if (m_animator == null || p_layer == LAYER.LAYER_COUNT)
            return false;

        return !m_currentlyBlending && m_animator.GetCurrentAnimatorStateInfo(m_layerToInt[(int)p_layer]).normalizedTime > END_ANIMATION_TIME;
    }

    /// <summary>
    /// Playing of animation for base animations 
    /// </summary>
    /// <param name="p_anim">Animation to play</param>
    /// <param name="p_blendTime">Time to blend between</param>
    public void PlayAnimation(BASE_DEFINES p_anim, BLEND_TIME p_blendTime = BLEND_TIME.SHORT)
    {
        if(p_anim!= BASE_DEFINES.BASE_COUNT)
            PlayAnimation(m_baseToString[(int)p_anim], LAYER.BASE, p_blendTime);
    }

    /// <summary>
    /// Playing of animation for attack animations 
    /// </summary>
    /// <param name="p_attackString">string of attack animation</param>
    /// <param name="p_blendTime">Time to blend between</param>
    public void PlayAnimation(string p_attackString, BLEND_TIME p_blendTime = BLEND_TIME.SHORT)
    {
        if(p_attackString != "")
            PlayAnimation(p_attackString, LAYER.ATTACK, p_blendTime);
    }

    /// <summary>
    /// Playing of animation for interrupt animations 
    /// </summary>
    /// <param name="p_anim">Animation to play</param>
    /// <param name="p_blendTime">Time to blend between</param>
    public void PlayAnimation(INTERRUPT_DEFINES p_anim, BLEND_TIME p_blendTime = BLEND_TIME.INSTANT)
    {
        if(p_anim != INTERRUPT_DEFINES.INTERRUPT_COUNT)
            PlayAnimation(m_interruptToString[(int)p_anim], LAYER.INTERRUPT, p_blendTime);
    }

    /// <summary>
    /// Play a given animation. This fucntion should only be called from one of hte above playanimation functions
    /// </summary>
    /// <param name="p_animationString">String of animation to crossfade</param>
    /// <param name="p_layer">Layer to crossfade into</param>
    /// <param name="p_blendTime">Time to crossfade and blend layers</param>
    private void PlayAnimation(string p_animationString, LAYER p_layer, BLEND_TIME p_blendTime)
    {
        if(m_blendCoroutine != null)
        {
            m_animator.CrossFade(m_blendingTo.Key, m_layerToInt[(int)m_blendingTo.Value]);

            StopCoroutine(m_blendCoroutine);
        }

        m_blendingTo = new KeyValuePair<string, LAYER>(p_animationString, p_layer);
        
        float blendTime = p_blendTime == BLEND_TIME.INSTANT ? INSTANT_BLEND_TIME : p_blendTime == BLEND_TIME.SHORT ? SHORT_BLEND_TIME : LONG_BLEND_TIME;

        m_animator.CrossFade(p_animationString, m_layerToInt[(int)p_layer]);

        m_blendCoroutine = StartCoroutine(BlendAnimation(p_animationString, p_layer, blendTime));
    }

    /// <summary>
    /// Coroutein to crossfade/blend layers
    /// </summary>
    /// <param name="p_animationString">String of animation to play after transistion time</param>
    /// <param name="p_layer">Layer to blend into</param>
    /// <param name="p_blendTime">Time to blend</param>
    private IEnumerator BlendAnimation(string p_animationString, LAYER p_layer, float p_blendTime)
    {
        m_currentlyBlending = true;
        
        float currentBlendTime = 0.0f;

        while (currentBlendTime < p_blendTime)
        {
            currentBlendTime += Time.deltaTime;
            float changeInWeight = Time.deltaTime / p_blendTime;

            bool compledtedFlag = true;

            //Apply new weight
            for (int layerIndex = 0; layerIndex < (int)LAYER.LAYER_COUNT; layerIndex++)
            {
                float currentWeight = m_animator.GetLayerWeight(m_layerToInt[layerIndex]);
    
                if ((int)p_layer == layerIndex) //Layer changing to
                {
                    currentWeight += changeInWeight;
                    if (currentWeight < 1.0f)
                        compledtedFlag = false;
                }
                else
                {
                    currentWeight -= changeInWeight;
                    if (currentWeight > 0.0f)
                        compledtedFlag = false;
                }

                currentWeight = Mathf.Clamp(currentWeight, 0.0f, 1.0f);

                m_animator.SetLayerWeight(m_layerToInt[layerIndex], currentWeight);
            }

            if (compledtedFlag)
                break;

            yield return null;
        }

        m_currentlyBlending = false;
        
        SetLayersToNull(p_layer);

        m_animator.Play(p_animationString, m_layerToInt[(int)p_layer], currentBlendTime);
    }

    /// <summary>
    /// Set all layers to null excluding p_layerToIgnore.
    /// This should be the current layer in use
    /// </summary>
    /// <param name="p_layerToIgnore">Layer to not set to null</param>
    private void SetLayersToNull(LAYER p_layerToIgnore)
    {
        //Apply new weight
        for (int layerIndex = 0; layerIndex < (int)LAYER.LAYER_COUNT; layerIndex++)
        {
            if ((int)p_layerToIgnore != layerIndex)
            {
                m_animator.Play(NULL_STRING, m_layerToInt[layerIndex]);
            }
        }
    }

    /// <summary>
    /// Cehck if the aniamtor has the parameter present
    /// </summary>
    /// <param name="p_animator">Aniamtor to check against</param>
    /// <param name="p_string">stirng of parameter</param>
    /// <returns>true when parameter is found</returns>
    public static bool ContainsParam(Animator p_animator, string p_string)
    {
        if (p_animator == null || p_animator.runtimeAnimatorController == null) //Invalid animator
            return false;

        foreach (AnimatorControllerParameter param in p_animator.parameters)
        {
            if (param.name == p_string) return true;
        }
        return false;
    }
}
