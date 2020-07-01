using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation : MonoBehaviour
{
    private const string NULL_STRING = "Null";

    //Used Varibles
    public enum LAYER {BASE = 0, ATTACK, INTERRUPT, LAYER_COUNT}
    private int[] m_layerToInt = new int[(int)LAYER.LAYER_COUNT];

    public enum VARIBLE_FLOAT { CURRENT_VELOCITY, DESIRED_VELOCITY, ABSOLUTE_VELOCITY, VERTICAL_VELOCITY, RANDOM_IDLE, FLOAT_COUNT}
    private string[] m_varibleFloatToString = new string[(int)VARIBLE_FLOAT.FLOAT_COUNT];
    public enum VARIBLE_BOOL {BLOCK, DASH, JUMP, IN_AIR, BOOL_COUNT}
    private string[] m_varibleBoolToString = new string[(int)VARIBLE_BOOL.BOOL_COUNT];

    public enum INTERRUPT_BOOL {RECOIL, KNOCKBACK, DEATH, INTERRUPT_COUNT}
    private string[] m_interruptToString = new string[(int)INTERRUPT_BOOL.INTERRUPT_COUNT];

    private Animator m_animator = null;

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

        m_varibleFloatToString[(int)VARIBLE_FLOAT.CURRENT_VELOCITY] ="CurrentVelocity";
        m_varibleFloatToString[(int)VARIBLE_FLOAT.DESIRED_VELOCITY] = "DesiredVelocity";
        m_varibleFloatToString[(int)VARIBLE_FLOAT.ABSOLUTE_VELOCITY] = "AbsoluteVelocity";
        m_varibleFloatToString[(int)VARIBLE_FLOAT.VERTICAL_VELOCITY] ="VerticalVelocity";
        m_varibleFloatToString[(int)VARIBLE_FLOAT.RANDOM_IDLE] = "RandomIdle";

        m_varibleBoolToString[(int)VARIBLE_BOOL.BLOCK] = "Block";
        m_varibleBoolToString[(int)VARIBLE_BOOL.DASH] = "Dash";
        m_varibleBoolToString[(int)VARIBLE_BOOL.JUMP] = "Jump";
        m_varibleBoolToString[(int)VARIBLE_BOOL.IN_AIR] = "InAir";

        m_interruptToString[(int)INTERRUPT_BOOL.RECOIL]="Recoil";
        m_interruptToString[(int)INTERRUPT_BOOL.KNOCKBACK] = "Knockback";
        m_interruptToString[(int)INTERRUPT_BOOL.DEATH] = "Death";
    }

    /// <summary>
    /// Set the varible value 
    /// </summary>
    /// <param name="p_floatVarible">Varible to set</param>
    /// <param name="p_value">Value of varible to set to</param>
    public void SetVaribleFloat(VARIBLE_FLOAT p_floatVarible, float p_value)
    {
        if(p_floatVarible != VARIBLE_FLOAT.FLOAT_COUNT)
            m_animator.SetFloat(m_varibleFloatToString[(int)p_floatVarible], p_value);
    }

    /// <summary>
    /// Set the varible value 
    /// </summary>
    /// <param name="p_boolVarible">Varible to set</param>
    /// <param name="p_value">Value of varible to set to</param>
    public void SetVaribleBool(VARIBLE_BOOL p_boolVarible, bool p_value)
    {
        if (p_boolVarible != VARIBLE_BOOL.BOOL_COUNT)
            m_animator.SetBool(m_varibleBoolToString[(int)p_boolVarible], p_value);
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

        return m_animator.GetCurrentAnimatorStateInfo(m_layerToInt[(int)p_layer]).normalizedTime > 0.99f;
    }

    /// <summary>
    /// Play a given attack animation
    /// </summary>
    /// <param name="p_attackLeaf">Attack leaf containg all relavant animation data</param>
    public void PlayAttack(ManoeuvreLeaf p_attackLeaf)
    {
        m_animator.SetLayerWeight(m_layerToInt[(int)LAYER.BASE], 0.0f);
        m_animator.SetLayerWeight(m_layerToInt[(int)LAYER.ATTACK], 1.0f);

        m_animator.Play(p_attackLeaf.GetAnimationString(), m_layerToInt[(int)LAYER.ATTACK]);
    }

    /// <summary>
    /// End of attacking, move back to base animator layer
    /// </summary>
    public void EndAttack()
    {
        m_animator.SetLayerWeight(m_layerToInt[(int)LAYER.BASE], 1.0f);
        m_animator.SetLayerWeight(m_layerToInt[(int)LAYER.ATTACK], 0.0f);

        NullLayer(LAYER.ATTACK);
    }

    /// <summary>
    /// Play an interrupt
    /// Sets layer weight
    /// </summary>
    /// <param name="p_interrupt">Interrupt to set</param>
    public void PlayInterrupt(INTERRUPT_BOOL p_interrupt)
    {
        if (p_interrupt != INTERRUPT_BOOL.INTERRUPT_COUNT)
        {
            m_animator.Play(m_interruptToString[(int)p_interrupt], m_layerToInt[(int)LAYER.INTERRUPT]);

            m_animator.SetLayerWeight(m_layerToInt[(int)LAYER.BASE], 0.0f);
            m_animator.SetLayerWeight(m_layerToInt[(int)LAYER.ATTACK], 0.0f);
            m_animator.SetLayerWeight(m_layerToInt[(int)LAYER.INTERRUPT], 1.0f);
        }
    }

    /// <summary>
    /// End an interrupt
    /// Sets layer weight
    /// </summary>
    public void EndInterrupt()
    {
        m_animator.SetLayerWeight(m_layerToInt[(int)LAYER.BASE], 1.0f);
        m_animator.SetLayerWeight(m_layerToInt[(int)LAYER.ATTACK], 0.0f);
        m_animator.SetLayerWeight(m_layerToInt[(int)LAYER.INTERRUPT], 0.0f);

        NullLayer(LAYER.INTERRUPT);
    }

    /// <summary>
    /// Set the layer to the null state in animator
    /// </summary>
    /// <param name="p_layer"></param>
    public void NullLayer(LAYER p_layer)
    {
        m_animator.Play(NULL_STRING, m_layerToInt[(int)p_layer]);
    }
}
