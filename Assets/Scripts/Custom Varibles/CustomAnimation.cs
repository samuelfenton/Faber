using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation : MonoBehaviour
{
    //Manouevre String
    public const string SECTION01_STRING = "_Section1";
    public const string SECTION02_STRING = "_Section2";

    protected const string TYPE_GROUND = "Ground";
    protected const string TYPE_INAIR = "InAir";
    protected const string TYPE_SPRINT = "Sprint";

    protected const string STANCE_LIGHT = "Light";
    protected const string STANCE_HEAVY = "Heavy";

    //Blending
    public enum BLEND_TIME {INSTANT, SHORT, LONG }

    protected const float END_ANIMATION_TIME = 0.9f;

    protected const float INSTANT_BLEND_TIME = 0.05f;
    protected const float SHORT_BLEND_TIME = 0.15f;
    protected const float LONG_BLEND_TIME = 0.3f;

    //Layers
    public const string BASE_LAYER_STRING = "BaseLayer";
    public const string ATTACK_LAYER_STRING = "AttackLayer";
    public const string INTERRUPT_LAYER_STRING = "InterruptLayer";

    public const string NULL_STRING = "Null";
    public const string END_ATTACK_BLEND = "EndAttackBlend";

    //Used Varibles
    public enum LAYER {BASE = 0, ATTACK, INTERRUPT, LAYER_COUNT}
    protected int[] m_layerToInt = new int[(int)LAYER.LAYER_COUNT];

    //To be filled up in init by derrived class
    protected string[] m_floatToString = new string[0];
    protected string[] m_baseToString = new string[0];
    protected string[] m_interruptToString = new string[0];

    protected Animator m_animator = null;

    //Blending
    protected LAYER m_currentLayer = LAYER.BASE;
    protected Coroutine m_blendCoroutine = null;
    protected KeyValuePair<string, LAYER> m_blendingTo = new KeyValuePair<string, LAYER>();

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

        //Derrived classes need to define m_floatToString, m_baseToString, m_interruptToString
        //See CustomAnimation_Player.cs for example
    }

    /// <summary>
    /// Given a manoeuvres variables, build a string for the animator to play
    /// </summary>
    /// <param name="p_type">Type of manoeuvre</param>
    /// <param name="p_stance">Stance of manoeuvre</param>
    /// <param name="p_index">Index of manoeuvre</param>
    /// <returns>String for animation, it may not exist in the animator</returns>
    public static string BuildManoeuvreString(Manoeuvre.MANOEUVRE_TYPE p_type, Manoeuvre.MANOEUVRE_STANCE p_stance, int p_index)
    {
        string type = p_type == Manoeuvre.MANOEUVRE_TYPE.GROUND ? TYPE_GROUND : p_type == Manoeuvre.MANOEUVRE_TYPE.INAIR ? TYPE_INAIR : TYPE_SPRINT;
        string stance = p_stance == Manoeuvre.MANOEUVRE_STANCE.LIGHT ? STANCE_LIGHT : STANCE_HEAVY;

        return type + "_" + stance + p_index;
    }

    /// <summary>
    /// Set the varible value 
    /// </summary>
    /// <param name="p_floatIndex">Varible to set, typically use enums</param>
    /// <param name="p_value">Value of varible to set to</param>
    public void SetVaribleFloat(int p_floatIndex, float p_value)
    {
        if(p_floatIndex > 0 && p_floatIndex < m_floatToString.Length && m_floatToString[p_floatIndex] != "")
            m_animator.SetFloat(m_floatToString[p_floatIndex], p_value);
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
    /// <param name="p_animationIndex">Animation to play, typically use enums in derived class to define its index</param>
    /// <param name="p_layer">Layer to play on</param>
    /// <param name="p_blendTime">Time to blend between</param>
    public void PlayAnimation(int p_animationIndex, LAYER p_layer, BLEND_TIME p_blendTime = BLEND_TIME.SHORT)
    {
        switch (p_layer)
        {
            case LAYER.BASE:
                if(p_animationIndex >= 0 && p_animationIndex < m_baseToString.Length && m_baseToString[p_animationIndex] != "")
                    PlayAnimationSetup(m_baseToString[p_animationIndex], p_layer, p_blendTime);
                break;
            case LAYER.INTERRUPT:
                if (p_animationIndex >= 0 && p_animationIndex < m_interruptToString.Length && m_interruptToString[p_animationIndex] != "")
                    PlayAnimationSetup(m_interruptToString[p_animationIndex], p_layer, p_blendTime);
                break;
            case LAYER.ATTACK:
            case LAYER.LAYER_COUNT:
            default:
                break;
        }

    }

    /// <summary>
    /// Playing of animation based off strings
    /// </summary>
    /// <param name="p_attackString">string of attack animation</param>
    /// <param name="p_layer">Layer to play on</param>
    /// <param name="p_blendTime">Time to blend between</param>
    public void PlayAnimation(string p_animationString, LAYER p_layer, BLEND_TIME p_blendTime = BLEND_TIME.SHORT)
    {
        switch (p_layer)
        {
            case LAYER.BASE:
            case LAYER.ATTACK:
            case LAYER.INTERRUPT:
                if (p_animationString != "" && HasAnimation(m_animator, p_animationString, m_layerToInt[(int)p_layer]))
                    PlayAnimationSetup(p_animationString, p_layer, p_blendTime);
                break;
            case LAYER.LAYER_COUNT:
            default:
                break;
        }
    }

    /// <summary>
    /// Play a given animation. This fucntion should only be called from one of hte above play animation functions
    /// </summary>
    /// <param name="p_animationString">String of animation to crossfade</param>
    /// <param name="p_layer">Layer to crossfade into</param>
    /// <param name="p_blendTime">Time to crossfade and blend layers</param>
    private void PlayAnimationSetup(string p_animationString, LAYER p_layer, BLEND_TIME p_blendTime)
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

        if(m_blendingTo.Value != LAYER.LAYER_COUNT) //This is the equivalent of null
            m_animator.Play(m_blendingTo.Key, m_layerToInt[(int)m_blendingTo.Value], p_currentBlendDuration);

        if(m_blendCoroutine!= null)
        {
            StopCoroutine(m_blendCoroutine);
            m_blendCoroutine = null;
        }

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
