using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation_Player : CustomAnimation
{
    public enum VARIBLE_FLOAT { CURRENT_VELOCITY, ABSOLUTE_VELOCITY, VERTICAL_VELOCITY, RANDOM_IDLE, KNOCKBACK_IMPACT, COUNT }
    public enum BASE_DEFINES { LOCOMOTION, SPRINT, RUN_TO_SPRINT, DASH, JUMP, INAIR, DOUBLE_JUMP, INAIR_DASH, LANDING_TO_IDLE, LANDING_TO_RUN, WALL_JUMP, BLOCK, BLOCK_FROM_IDLE, BLOCK_TO_IDLE, PRAY_START, PRAY_LOOP, PRAY_END, COUNT }
    public enum INTERRUPT_DEFINES { RECOIL, KNOCKBACK, KNOCKFORWARD, DEATH, IDLE_EMOTE, COUNT }

    /// <summary>
    /// Setup dicionaries used
    /// </summary>
    /// <param name="p_animator">Animator to check against</param>
    public override void Init(Animator p_animator)
    {
        base.Init(p_animator);

        m_floatToString = new string[(int)VARIBLE_FLOAT.COUNT];
        m_baseToString = new string[(int)BASE_DEFINES.COUNT];
        m_interruptToString = new string[(int)INTERRUPT_DEFINES.COUNT];

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
}
