using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation_Humanoid : CustomAnimation
{
    //Interrupt
    public enum INTERRUPT_ANIM { DEATH, KNOCKBACK, RECOIL }
    private Dictionary<INTERRUPT_ANIM, string> m_interruptAnimToString = new Dictionary<INTERRUPT_ANIM, string>();
    //Locomotion
    public enum LOCOMOTION_ANIM { NULL, IDLE, DODGE, JUMP, DOUBLE_JUMP, IN_AIR, LAND, WALL_GRAB, WALL_FLIP, ROLL, BLOCK }
    private Dictionary<LOCOMOTION_ANIM, string> m_locomotionAnimToString = new Dictionary<LOCOMOTION_ANIM, string>();

    /// <summary>
    /// Setup dicionaries used
    /// </summary>
    /// <param name="p_animator">Animator to check against</param>
    public override void Init(Animator p_animator)
    {
        base.Init(p_animator);

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
}

