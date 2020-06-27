using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation : MonoBehaviour
{
    private const string m_baseLayerString = "BaseLayer";
    private const string m_attackLayerString = "AttackLayer";
    private const string m_interruptLayerString = "InterruptLayer";

    //Used Varibles
    public enum VARIBLE_FLOAT { CURRENT_VELOCITY, DESIRED_VELOCITY, ABSOLUTE_VELOCITY, VERTICAL_VELOCITY, RANDOM_IDLE}
    protected Dictionary<VARIBLE_FLOAT, string> m_varibleFloatToString = new Dictionary<VARIBLE_FLOAT, string>();
    public enum VARIBLE_BOOL {BLOCK, DASH, JUMP, IN_AIR}
    protected Dictionary<VARIBLE_BOOL, string> m_varibleBoolToString = new Dictionary<VARIBLE_BOOL, string>();

    public enum INTERRUPT_BOOL {RECOIL, KNOCKBACK, DEATH }
    protected Dictionary<INTERRUPT_BOOL, string> m_interruptToString = new Dictionary<INTERRUPT_BOOL, string>();

    protected Animator m_animator = null;

    protected int m_baseLayer = 0;
    protected int m_interruptLayer = 0;
    protected int m_attackLayer = 0;

    /// <summary>
    /// Setup dicionaries used
    /// </summary>
    /// <param name="p_animator">Animator to check against</param>
    public virtual void Init(Animator p_animator)
    {
        m_animator = p_animator;

        m_varibleFloatToString.Add(VARIBLE_FLOAT.CURRENT_VELOCITY, "CurrentVelocity");
        m_varibleFloatToString.Add(VARIBLE_FLOAT.DESIRED_VELOCITY, "DesiredVelocity");
        m_varibleFloatToString.Add(VARIBLE_FLOAT.ABSOLUTE_VELOCITY, "AbsoluteVelocity");
        m_varibleFloatToString.Add(VARIBLE_FLOAT.VERTICAL_VELOCITY, "VerticalVelocity");
        m_varibleFloatToString.Add(VARIBLE_FLOAT.RANDOM_IDLE, "RandomIdle");

        m_varibleBoolToString.Add(VARIBLE_BOOL.BLOCK, "Block");
        m_varibleBoolToString.Add(VARIBLE_BOOL.DASH, "Dash");
        m_varibleBoolToString.Add(VARIBLE_BOOL.JUMP, "Jump");
        m_varibleBoolToString.Add(VARIBLE_BOOL.IN_AIR, "InAir");

        m_interruptToString.Add(INTERRUPT_BOOL.RECOIL, "Recoil");
        m_interruptToString.Add(INTERRUPT_BOOL.KNOCKBACK, "Knockback");
        m_interruptToString.Add(INTERRUPT_BOOL.DEATH, "Death");

        m_baseLayer = m_animator.GetLayerIndex(m_baseLayerString);
        m_attackLayer = m_animator.GetLayerIndex(m_attackLayerString);
        m_interruptLayer = m_animator.GetLayerIndex(m_interruptLayerString);
    }

    /// <summary>
    /// Set the varible value 
    /// </summary>
    /// <param name="p_floatVarible">Varible to set</param>
    /// <param name="p_value">Value of varible to set to</param>
    public void SetVaribleFloat(VARIBLE_FLOAT p_floatVarible, float p_value)
    {
        m_animator.SetFloat(m_varibleFloatToString[p_floatVarible], p_value);
    }

    /// <summary>
    /// Set the varible value 
    /// </summary>
    /// <param name="p_boolVarible">Varible to set</param>
    /// <param name="p_value">Value of varible to set to</param>
    public void SetVaribleBool(VARIBLE_BOOL p_boolVarible, bool p_value)
    {
        m_animator.SetBool(m_varibleBoolToString[p_boolVarible], p_value);
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

        return m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f;
    }

    /// <summary>
    /// Play a given attack animation
    /// </summary>
    /// <param name="p_attackLeaf">Attack leaf containg all relavant animation data</param>
    public void PlayAttack(ManoeuvreLeaf p_attackLeaf)
    {
        m_animator.SetLayerWeight(m_baseLayer, 0.0f);
        m_animator.SetLayerWeight(m_attackLayer, 1.0f);

        m_animator.Play(p_attackLeaf.GetAnimationString());
    }

    /// <summary>
    /// End of attacking, move back to base animator layer
    /// </summary>
    public void EndAttack()
    {
        m_animator.SetLayerWeight(m_baseLayer, 1.0f);
        m_animator.SetLayerWeight(m_attackLayer, 0.0f);
    }

    /// <summary>
    /// Play an interrupt
    /// Sets layer weight
    /// </summary>
    /// <param name="p_interrupt">Interrupt to set</param>
    public void PlayInterrupt(INTERRUPT_BOOL p_interrupt)
    {
        m_animator.Play(m_interruptToString[p_interrupt]);

        m_animator.SetLayerWeight(m_baseLayer, 0.0f);
        m_animator.SetLayerWeight(m_attackLayer, 0.0f);
        m_animator.SetLayerWeight(m_interruptLayer, 1.0f);
    }

    /// <summary>
    /// End an interrupt
    /// Sets layer weight
    /// </summary>
    public void EndInterrupt()
    {
        m_animator.SetLayerWeight(m_baseLayer, 1.0f);
        m_animator.SetLayerWeight(m_attackLayer, 0.0f);
        m_animator.SetLayerWeight(m_interruptLayer, 0.0f);
    }
}
