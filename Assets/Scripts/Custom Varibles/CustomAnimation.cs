﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation : MonoBehaviour
{
    public enum BLEND_TIME {INSTANT, SHORT, LONG }

    private const float END_ANIMATION_TIME = 0.9f;

    private const float INSTANT_BLEND_TIME = 0.05f;
    private const float SHORT_BLEND_TIME = 0.15f;
    private const float LONG_BLEND_TIME = 0.3f;

    public const string BASE_LAYER_STRING = "BaseLayer";
    public const string ATTACK_LAYER_STRING = "AttackLayer";
    public const string INTERRUPT_LAYER_STRING = "InterruptLayer";

    public const string NULL_STRING = "Null";
    public const string END_ATTACK_BLEND = "EndAttackBlend";

    //Used Varibles
    public enum LAYER {BASE = 0, ATTACK, INTERRUPT, LAYER_COUNT}
    private int[] m_layerToInt = new int[(int)LAYER.LAYER_COUNT];

    public enum VARIBLE_FLOAT { CURRENT_VELOCITY, ABSOLUTE_VELOCITY, VERTICAL_VELOCITY, RANDOM_IDLE, KNOCKBACK_IMPACT, FLOAT_COUNT}
    private string[] m_floatToString = new string[(int)VARIBLE_FLOAT.FLOAT_COUNT];
    public enum BASE_DEFINES {LOCOMOTION, SPRINT, RUN_TO_SPRINT, DASH, JUMP, INAIR, DOUBLE_JUMP, INAIR_DASH, LANDING_TO_IDLE, LANDING_TO_RUN, WALL_JUMP, BLOCK, BLOCK_FROM_IDLE, BLOCK_TO_IDLE, PRAY_START, PRAY_LOOP, PRAY_END, BASE_COUNT }
    private string[] m_baseToString = new string[(int)BASE_DEFINES.BASE_COUNT];

    public enum INTERRUPT_DEFINES {RECOIL, KNOCKBACK, KNOCKFORWARD, DEATH, IDLE_EMOTE, INTERRUPT_COUNT}
    private string[] m_interruptToString = new string[(int)INTERRUPT_DEFINES.INTERRUPT_COUNT];

    private Animator m_animator = null;

    //Blending
    private LAYER m_currentLayer = LAYER.BASE;
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
            m_layerToInt[(int)LAYER.BASE] = m_animator.GetLayerIndex(BASE_LAYER_STRING);
            m_layerToInt[(int)LAYER.ATTACK] = m_animator.GetLayerIndex(ATTACK_LAYER_STRING);
            m_layerToInt[(int)LAYER.INTERRUPT] = m_animator.GetLayerIndex(INTERRUPT_LAYER_STRING);
        }

        //Assign strings, in the case a string/aniamtion is not found in the animator default to empty string ""
        m_floatToString[(int)VARIBLE_FLOAT.CURRENT_VELOCITY] = ContainsParam(m_animator, "CurrentVelocity") ? "CurrentVelocity" : "";
        m_floatToString[(int)VARIBLE_FLOAT.ABSOLUTE_VELOCITY] = ContainsParam(m_animator, "AbsoluteVelocity") ? "AbsoluteVelocity" : "";
        m_floatToString[(int)VARIBLE_FLOAT.VERTICAL_VELOCITY] = ContainsParam(m_animator, "VerticalVelocity") ? "VerticalVelocity" : "";
        m_floatToString[(int)VARIBLE_FLOAT.RANDOM_IDLE] = ContainsParam(m_animator, "RandomIdle") ? "RandomIdle" : "";
        m_floatToString[(int)VARIBLE_FLOAT.KNOCKBACK_IMPACT] = ContainsParam(m_animator, "KnockbackImpact") ? "KnockbackImpact" : "";

        m_baseToString[(int)BASE_DEFINES.LOCOMOTION] = HasAnimation(m_animator, "Locomotion", m_layerToInt[(int)LAYER.BASE]) ? "Locomotion" : "";
        m_baseToString[(int)BASE_DEFINES.SPRINT] = HasAnimation(m_animator, "Sprint", m_layerToInt[(int)LAYER.BASE]) ? "Sprint" : "";
        m_baseToString[(int)BASE_DEFINES.RUN_TO_SPRINT] = HasAnimation(m_animator, "RunToSprint", m_layerToInt[(int)LAYER.BASE]) ? "RunToSprint" : "";
        m_baseToString[(int)BASE_DEFINES.DASH] = HasAnimation(m_animator, "Dash", m_layerToInt[(int)LAYER.BASE]) ? "Dash" : "";
        m_baseToString[(int)BASE_DEFINES.JUMP] = HasAnimation(m_animator, "Jump", m_layerToInt[(int)LAYER.BASE]) ? "Jump" : "";
        m_baseToString[(int)BASE_DEFINES.INAIR] = HasAnimation(m_animator, "InAir", m_layerToInt[(int)LAYER.BASE]) ? "InAir" : "";
        m_baseToString[(int)BASE_DEFINES.DOUBLE_JUMP] = HasAnimation(m_animator, "DoubleJump", m_layerToInt[(int)LAYER.BASE]) ? "DoubleJump" : "";
        m_baseToString[(int)BASE_DEFINES.INAIR_DASH] = HasAnimation(m_animator, "InAirDash", m_layerToInt[(int)LAYER.BASE]) ? "InAirDash" : "";
        m_baseToString[(int)BASE_DEFINES.LANDING_TO_IDLE] = HasAnimation(m_animator, "LandingToIdle", m_layerToInt[(int)LAYER.BASE]) ? "LandingToIdle" : "";
        m_baseToString[(int)BASE_DEFINES.LANDING_TO_RUN] = HasAnimation(m_animator, "LandingToRun", m_layerToInt[(int)LAYER.BASE]) ? "LandingToRun" : "";
        m_baseToString[(int)BASE_DEFINES.WALL_JUMP] = HasAnimation(m_animator, "WallJump", m_layerToInt[(int)LAYER.BASE]) ? "WallJump" : "";
        m_baseToString[(int)BASE_DEFINES.BLOCK] = HasAnimation(m_animator, "Block", m_layerToInt[(int)LAYER.BASE]) ? "Block" : "";
        m_baseToString[(int)BASE_DEFINES.BLOCK_FROM_IDLE] = HasAnimation(m_animator, "BlockFromIdle", m_layerToInt[(int)LAYER.BASE]) ? "BlockFromIdle" : "";
        m_baseToString[(int)BASE_DEFINES.BLOCK_TO_IDLE] = HasAnimation(m_animator, "BlockToIdle", m_layerToInt[(int)LAYER.BASE]) ? "BlockToIdle" : "";
        m_baseToString[(int)BASE_DEFINES.PRAY_START] = HasAnimation(m_animator, "PrayStart", m_layerToInt[(int)LAYER.BASE]) ? "PrayStart" : "";
        m_baseToString[(int)BASE_DEFINES.PRAY_LOOP] = HasAnimation(m_animator, "PrayLoop", m_layerToInt[(int)LAYER.BASE]) ? "PrayLoop" : "";
        m_baseToString[(int)BASE_DEFINES.PRAY_END] = HasAnimation(m_animator, "PrayEnd", m_layerToInt[(int)LAYER.BASE]) ? "PrayEnd" : "";

        m_interruptToString[(int)INTERRUPT_DEFINES.RECOIL] = HasAnimation(m_animator, "Recoil", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Recoil" : "";
        m_interruptToString[(int)INTERRUPT_DEFINES.KNOCKBACK] = HasAnimation(m_animator, "Knockback", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Knockback" : "";
        m_interruptToString[(int)INTERRUPT_DEFINES.KNOCKFORWARD] = HasAnimation(m_animator, "Knockforward", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Knockforward" : "";
        m_interruptToString[(int)INTERRUPT_DEFINES.DEATH] = HasAnimation(m_animator, "Death", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Death" : "";
        m_interruptToString[(int)INTERRUPT_DEFINES.IDLE_EMOTE] = HasAnimation(m_animator, "IdleEmote", m_layerToInt[(int)LAYER.INTERRUPT]) ? "IdleEmote" : "";
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
        if (m_animator == null || p_layer == LAYER.LAYER_COUNT || m_layerToInt[(int)p_layer] == -1)
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

        return !IsAnimatorBlending() && m_animator.GetCurrentAnimatorStateInfo(m_layerToInt[(int)p_layer]).normalizedTime > END_ANIMATION_TIME;
    }

    /// <summary>
    /// Determine if animator is currently blending
    /// </summary>
    /// <returns>True when animator is not blending</returns>
    public bool IsAnimatorBlending()
    {
        return m_blendCoroutine != null;
    }

    /// <summary>
    /// Playing of animation for base animations 
    /// </summary>
    /// <param name="p_anim">Animation to play</param>
    /// <param name="p_blendTime">Time to blend between</param>
    public void PlayAnimation(BASE_DEFINES p_anim, BLEND_TIME p_blendTime = BLEND_TIME.SHORT)
    {
        if(p_anim!= BASE_DEFINES.BASE_COUNT && m_baseToString[(int)p_anim] != "")
            PlayAnimation(m_baseToString[(int)p_anim], LAYER.BASE, p_blendTime);
    }

    /// <summary>
    /// Playing of animation for attack animations 
    /// </summary>
    /// <param name="p_attackString">string of attack animation</param>
    /// <param name="p_blendTime">Time to blend between</param>
    public void PlayAnimation(string p_attackString, BLEND_TIME p_blendTime = BLEND_TIME.SHORT)
    {
        if(p_attackString != "" && HasAnimation(m_animator, p_attackString, m_layerToInt[(int)LAYER.ATTACK]))
            PlayAnimation(p_attackString, LAYER.ATTACK, p_blendTime);
    }

    /// <summary>
    /// Playing of animation for interrupt animations 
    /// </summary>
    /// <param name="p_anim">Animation to play</param>
    /// <param name="p_blendTime">Time to blend between</param>
    public void PlayAnimation(INTERRUPT_DEFINES p_anim, BLEND_TIME p_blendTime = BLEND_TIME.INSTANT)
    {
        if(p_anim != INTERRUPT_DEFINES.INTERRUPT_COUNT && m_interruptToString[(int)p_anim] != "")
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
        //Crossfade requires layer in name
        string layerAnimationString = p_layer == LAYER.BASE ? BASE_LAYER_STRING : (p_layer == LAYER.ATTACK ? ATTACK_LAYER_STRING : INTERRUPT_LAYER_STRING);
        layerAnimationString = layerAnimationString + "." + p_animationString;

        if(m_blendingTo.Key == layerAnimationString && m_blendingTo.Value == p_layer) //Attempting to blend into same animation, break out early
        {
            return;
        }

        if(m_blendCoroutine != null) //If another animation call comes in, end previous blending harshly, and start new
        {
            m_animator.Play(m_blendingTo.Key, m_layerToInt[(int)m_blendingTo.Value]);

            EndBlend(m_blendingTo.Value, 0.0f);
        }

        m_blendingTo = new KeyValuePair<string, LAYER>(layerAnimationString, p_layer);
        
        float blendTime = p_blendTime == BLEND_TIME.INSTANT ? INSTANT_BLEND_TIME : p_blendTime == BLEND_TIME.SHORT ? SHORT_BLEND_TIME : LONG_BLEND_TIME;

        m_animator.CrossFadeInFixedTime(layerAnimationString, blendTime);

        if(m_currentLayer == p_layer) //Not blending between layers
        {
            m_blendCoroutine = StartCoroutine(BlendAnimations(blendTime));
        }
        else //Blending between layers
        {
            m_blendCoroutine = StartCoroutine(BlendLayers(p_layer, blendTime));
        }
    }

    /// <summary>
    /// Coroutine to crossfade/blend layers
    /// </summary>
    /// <param name="p_layer">Layer to blend into</param>
    /// <param name="p_blendTime">Time to blend</param>
    private IEnumerator BlendAnimations(float p_blendTime)
    {
        float currentBlendTime = 0.0f;

        while (currentBlendTime < p_blendTime)
        {
            currentBlendTime += Time.deltaTime;

            yield return null;
        }

        currentBlendTime += Time.deltaTime;

        EndBlend(m_currentLayer, currentBlendTime);
    }

    /// <summary>
    /// Coroutine to crossfade/blend layers
    /// </summary>
    /// <param name="p_layer">Layer to blend into</param>
    /// <param name="p_blendTime">Time to blend</param>
    private IEnumerator BlendLayers(LAYER p_layer, float p_blendTime)
    {        
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

        currentBlendTime += Time.deltaTime;

        EndBlend(p_layer, currentBlendTime);
    }

    /// <summary>
    /// End the current blending
    /// </summary>
    /// <param name="p_currentLayer">New current layer to have</param>
    /// <param name="p_currentBlendDuration">How long the blend was, new animation will start this far through</param>
    private void EndBlend(LAYER p_currentLayer, float p_currentBlendDuration)
    {
        m_currentLayer = p_currentLayer;

        if(m_blendingTo.Value != LAYER.LAYER_COUNT)
            m_animator.Play(m_blendingTo.Key, m_layerToInt[(int)m_blendingTo.Value], p_currentBlendDuration);

        m_blendCoroutine = null;

        m_blendingTo = new KeyValuePair<string, LAYER>("", LAYER.LAYER_COUNT);

        m_animator.SetLayerWeight(m_layerToInt[(int)m_currentLayer], 1.0f);
        SetLayersToNull(m_currentLayer);
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
                m_animator.SetLayerWeight(m_layerToInt[layerIndex], 0.0f);
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

    /// <summary>
    /// Does the animator has a given animation
    /// </summary>
    /// <param name="p_animator">Aniamtor to check against</param>
    /// <param name="p_animation">Animation to check for</param>
    /// <param name="p_layer">Layer to check against</param>
    /// <returns>true when animaiton is found</returns>
    public static bool HasAnimation(Animator p_animator, string p_animation, int p_layer)
    {
        int animationID = Animator.StringToHash(p_animation);
        return p_animator.HasState(p_layer, animationID);
    }
}
