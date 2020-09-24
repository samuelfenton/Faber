using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation : MonoBehaviour
{
    private const float END_ANIMATION_TIME = 0.9f;
    private const float BLEND_TIME = 0.2f;

    private const string NULL_STRING = "Null";

    private const string SECTION01_STRING = "_Section1";
    private const string SECTION02_STRING = "_Section2";

    //Used Varibles
    public enum LAYER {BASE = 0, ATTACK, INTERRUPT, LAYER_COUNT}
    private int[] m_layerToInt = new int[(int)LAYER.LAYER_COUNT];

    public enum VARIBLE_FLOAT { CURRENT_VELOCITY, ABSOLUTE_VELOCITY, VERTICAL_VELOCITY, RANDOM_IDLE, FLOAT_COUNT}
    private string[] m_floatToString = new string[(int)VARIBLE_FLOAT.FLOAT_COUNT];
    public enum BASE_DEFINES {LOCOMOTION, SPRINT, RUN_TO_SPRINT, DASH, JUMP, INAIR, DOUBLE_JUMP, INAIR_DASH, LANDING_TO_IDLE, LANDING_TO_RUN, BLOCK, BLOCK_FROM_IDLE, BLOCK_TO_IDLE, END_ATTACK_BLEND, BASE_COUNT }
    private string[] m_baseToString = new string[(int)BASE_DEFINES.BASE_COUNT];

    public enum INTERRUPT_DEFINES {RECOIL, KNOCKBACK, DEATH, IDLE_EMOTE, INTERRUPT_COUNT}
    private string[] m_interruptToString = new string[(int)INTERRUPT_DEFINES.INTERRUPT_COUNT];

    private Animator m_animator = null;

    //Blending
    private KeyValuePair<LAYER, float> m_currentBlendToLayer = new KeyValuePair<LAYER, float>(LAYER.LAYER_COUNT, 0.0f);
    private List<KeyValuePair<LAYER, float>> m_currentBlendFromLayers = new List<KeyValuePair<LAYER, float>>();

    private bool m_currentlyCrossfading = false;

    private Coroutine m_blendCoroutine = null;
    private Coroutine m_crossFadeCoroutine = null;
    /// <summary>
    /// Setup dicionaries used
    /// </summary>
    /// <param name="p_animator">Animator to check against</param>
    public virtual void Init(Animator p_animator)
    {
        m_animator = p_animator;

        m_layerToInt[(int)LAYER.BASE] = m_animator.GetLayerIndex("BaseLayer");
        m_layerToInt[(int)LAYER.ATTACK] = m_animator.GetLayerIndex("AttackLayer");
        m_layerToInt[(int)LAYER.INTERRUPT] = m_animator.GetLayerIndex("InterruptLayer");

        m_floatToString[(int)VARIBLE_FLOAT.CURRENT_VELOCITY] ="Current Velocity";
        m_floatToString[(int)VARIBLE_FLOAT.ABSOLUTE_VELOCITY] = "Absolute Velocity";
        m_floatToString[(int)VARIBLE_FLOAT.VERTICAL_VELOCITY] ="Vertical Velocity";
        m_floatToString[(int)VARIBLE_FLOAT.RANDOM_IDLE] = "Random Idle";

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
        m_baseToString[(int)BASE_DEFINES.END_ATTACK_BLEND] = "EndAttackBlend";

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
        if(p_floatVarible != VARIBLE_FLOAT.FLOAT_COUNT)
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

        return !m_currentlyCrossfading && m_animator.GetCurrentAnimatorStateInfo(m_layerToInt[(int)p_layer]).normalizedTime > END_ANIMATION_TIME;
    }

    /// <summary>
    /// Play base animation
    /// </summary>
    /// <param name="p_anim"></param>
    public void PlayBase(BASE_DEFINES p_anim)
    {
        StartCrossFade(m_baseToString[(int)p_anim], m_layerToInt[(int)LAYER.BASE]);
    }

    /// <summary>
    /// Play a given attack animation
    /// </summary>
    /// <param name="p_animationString">String for attack animation</param>
    public void PlayAttack(string p_animationString)
    {
        ChangeLayers(LAYER.ATTACK, false);

        m_animator.Play(p_animationString, m_layerToInt[(int)LAYER.ATTACK]);
    }

    /// <summary>
    /// Play section 1 of an attack
    /// </summary>
    /// <param name="p_animationString">String for attack animation</param>
    public void PlayAttackSection01(string p_animationString)
    {
        StartCrossFade(p_animationString + SECTION01_STRING, m_layerToInt[(int)LAYER.ATTACK]);
    }

    /// <summary>
    /// Play section 2 of an attack
    /// </summary>
    /// <param name="p_animationString">String for attack animation</param>
    public void PlayAttackSection02(string p_animationString)
    {
        StartCrossFade(p_animationString + SECTION02_STRING, m_layerToInt[(int)LAYER.ATTACK]);
    }

    /// <summary>
    /// End of attacking, move back to base animator layer
    /// </summary>
    /// <param name="p_blendSheathing">Does this attack require a sheathing blend</param>
    public void EndAttack(bool p_blendSheathing)
    {
        if(p_blendSheathing)
            m_animator.Play(m_baseToString[(int)BASE_DEFINES.END_ATTACK_BLEND], m_layerToInt[(int)LAYER.BASE]);

        ChangeLayers(LAYER.BASE, true);
    }

    /// <summary>
    /// Play an interrupt
    /// Sets layer weight
    /// </summary>
    /// <param name="p_interrupt">Interrupt to set</param>
    public void PlayInterrupt(INTERRUPT_DEFINES p_interrupt)
    {
        if (p_interrupt != INTERRUPT_DEFINES.INTERRUPT_COUNT)
        {
            ChangeLayers(LAYER.INTERRUPT, false);

            m_animator.Play(m_interruptToString[(int)p_interrupt], m_layerToInt[(int)LAYER.INTERRUPT]);
        }
    }

    /// <summary>
    /// End an interrupt
    /// Sets layer weight
    /// </summary>
    public void EndInterrupt()
    {
        ChangeLayers(LAYER.BASE, true);
    }

    /// <summary>
    /// Perform a crossfade
    /// Setups coroutine used to determine when crossfade has ended
    /// </summary>
    /// <param name="p_animation"></param>
    /// <param name="p_layer"></param>
    private void StartCrossFade(string p_animation, int p_layer)
    {
        m_animator.CrossFade(p_animation, BLEND_TIME, p_layer);

        m_currentlyCrossfading = true;

        if (m_crossFadeCoroutine != null)
            StopCoroutine(m_crossFadeCoroutine);

        m_crossFadeCoroutine = StartCoroutine(EndCrossFade());
    }

    private IEnumerator EndCrossFade()
    {
        yield return new WaitForSeconds(BLEND_TIME);

        m_currentlyCrossfading = false;
    }

    /// <summary>
    /// Change between layers
    /// </summary>
    /// <param name="p_layer">Desired layer to be max weight</param>
    /// <param name="p_blend">Should this be a hard transistion or blended</param>
    public void ChangeLayers(LAYER p_layer, bool p_blend)
    {
        if (p_blend)
        {
            if (m_currentBlendToLayer.Key == p_layer)//Already blending
                return;

            if (m_blendCoroutine != null) //End previous blending
            {
                StopCoroutine(m_blendCoroutine);
                m_blendCoroutine = null;
            }

            m_currentBlendToLayer = new KeyValuePair<LAYER, float>(p_layer, m_animator.GetLayerWeight(m_layerToInt[(int)p_layer]));

            if (m_currentBlendToLayer.Value == 1.0f) //Already at max weight
            {
                EndBlend();
                return;
            }

            m_currentBlendFromLayers.Clear();

            //Build blend
            for (int layerIndex = 0; layerIndex < (int)LAYER.LAYER_COUNT; layerIndex++)
            {
                if (layerIndex != (int)p_layer)
                {
                    float layerWeight = m_animator.GetLayerWeight(m_layerToInt[layerIndex]);
                    if(layerWeight != 0.0f)
                    {
                        m_currentBlendFromLayers.Add(new KeyValuePair<LAYER, float>((LAYER)layerIndex, layerWeight));
                    }
                }
            }

            m_blendCoroutine = StartCoroutine(BlendCoroutine());
        }
        else //Hard set weight, everythign to 0.0f excluding desired
        {
            SetCurrentLayer(p_layer);
            EndBlend();
        }
    }

    /// <summary>
    /// Blend all weights till either blending to layer is at a weigth of 1.0f, or all blending from weights are at 0.0f
    /// </summary>
    /// <returns></returns>
    private IEnumerator BlendCoroutine()
    {
        float changeInWeight = Time.deltaTime/ BLEND_TIME;

        m_currentBlendToLayer = new KeyValuePair<LAYER, float>(m_currentBlendToLayer.Key, m_currentBlendToLayer.Value + changeInWeight);

        if (m_currentBlendToLayer.Value >= 1.0f) //End of blend as going to has reached weight of 1.0f
        {
            EndBlend();
            yield break;
        }
        else
        {
            m_animator.SetLayerWeight(m_layerToInt[(int)m_currentBlendToLayer.Key], m_currentBlendToLayer.Value);
        }

        //Check all layers that will blend to 0.0f
        for (int layerIndex = 0; layerIndex < m_currentBlendFromLayers.Count; layerIndex++)
        {
            KeyValuePair<LAYER, float> newLayerWeight = new KeyValuePair<LAYER, float>(m_currentBlendFromLayers[layerIndex].Key, m_currentBlendFromLayers[layerIndex].Value - changeInWeight);

            m_currentBlendFromLayers[layerIndex] = newLayerWeight;

            if (newLayerWeight.Value <= 0.0f) //Layers finished, set to 0.0f weight and stop checking it later
            {
                m_animator.SetLayerWeight(m_layerToInt[(int)newLayerWeight.Key], 0.0f);
                m_currentBlendFromLayers.RemoveAt(layerIndex);
                layerIndex--;
            }
            else
            {
                m_animator.SetLayerWeight(m_layerToInt[(int)newLayerWeight.Key], newLayerWeight.Value);
            }
        }

        //All blending from layers have got a weight of 0.0f, so end of blend
        if (m_currentBlendFromLayers.Count == 0) 
        {
            EndBlend();
            yield break;
        }

        yield return null;

        m_blendCoroutine = StartCoroutine(BlendCoroutine());
    }

    /// <summary>
    /// End the blend process and reset values as needed
    /// </summary>
    private void EndBlend()
    {
        SetCurrentLayer(m_currentBlendToLayer.Key);

        //ResetValues
        m_currentBlendToLayer = new KeyValuePair<LAYER, float>(LAYER.LAYER_COUNT, 0.0f);

        if (m_blendCoroutine != null)
            StopCoroutine(m_blendCoroutine);

        m_blendCoroutine = null;
    }

    /// <summary>
    /// Set all other layer weights to be 0.0f, with the player animaiton as null
    /// </summary>
    /// <param name="p_layer"></param>
    private void SetCurrentLayer(LAYER p_layer)
    {
        if (p_layer == LAYER.LAYER_COUNT)
            return;

        for (int layerIndex = 0; layerIndex < (int)LAYER.LAYER_COUNT; layerIndex++)
        {
            if (layerIndex == (int)p_layer)
                m_animator.SetLayerWeight(m_layerToInt[layerIndex], 1.0f);
            else
            {
                m_animator.SetLayerWeight(m_layerToInt[layerIndex], 0.0f);
                NullLayer((LAYER)layerIndex);
            }
        }
    }



    /// <summary>
    /// Set the layer to the null state in animator
    /// </summary>
    /// <param name="p_layer"></param>
    public void NullLayer(LAYER p_layer)
    {
        if (p_layer == LAYER.BASE || p_layer == LAYER.LAYER_COUNT)//Base will never have a null layer 
            return;

        m_animator.Play(NULL_STRING, m_layerToInt[(int)p_layer]);
    }
}
