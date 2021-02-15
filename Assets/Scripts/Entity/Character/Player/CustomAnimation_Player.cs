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
    public override void Init()
    {
        base.Init();

        m_floatToString = new string[(int)VARIBLE_FLOAT.COUNT];
        m_baseToString = new string[(int)BASE_DEFINES.COUNT];
        m_interruptToString = new string[(int)INTERRUPT_DEFINES.COUNT];

        //Assign strings, in the case a string/aniamtion is not found in the animator default to empty string ""
        m_floatToString[(int)VARIBLE_FLOAT.CURRENT_VELOCITY] = ContainsParam("CurrentVelocity") ? "CurrentVelocity" : "";
        m_floatToString[(int)VARIBLE_FLOAT.ABSOLUTE_VELOCITY] = ContainsParam("AbsoluteVelocity") ? "AbsoluteVelocity" : "";
        m_floatToString[(int)VARIBLE_FLOAT.VERTICAL_VELOCITY] = ContainsParam("VerticalVelocity") ? "VerticalVelocity" : "";
        m_floatToString[(int)VARIBLE_FLOAT.RANDOM_IDLE] = ContainsParam("RandomIdle") ? "RandomIdle" : "";
        m_floatToString[(int)VARIBLE_FLOAT.KNOCKBACK_IMPACT] = ContainsParam("KnockbackImpact") ? "KnockbackImpact" : "";

        m_baseToString[(int)BASE_DEFINES.LOCOMOTION] = HasAnimation("Locomotion", m_layerToInt[(int)LAYER.BASE]) ? "Locomotion" : "";
        m_baseToString[(int)BASE_DEFINES.SPRINT] = HasAnimation("Sprint", m_layerToInt[(int)LAYER.BASE]) ? "Sprint" : "";
        m_baseToString[(int)BASE_DEFINES.RUN_TO_SPRINT] = HasAnimation("RunToSprint", m_layerToInt[(int)LAYER.BASE]) ? "RunToSprint" : "";
        m_baseToString[(int)BASE_DEFINES.DASH] = HasAnimation("Dash", m_layerToInt[(int)LAYER.BASE]) ? "Dash" : "";
        m_baseToString[(int)BASE_DEFINES.JUMP] = HasAnimation("Jump", m_layerToInt[(int)LAYER.BASE]) ? "Jump" : "";
        m_baseToString[(int)BASE_DEFINES.INAIR] = HasAnimation("InAir", m_layerToInt[(int)LAYER.BASE]) ? "InAir" : "";
        m_baseToString[(int)BASE_DEFINES.DOUBLE_JUMP] = HasAnimation("DoubleJump", m_layerToInt[(int)LAYER.BASE]) ? "DoubleJump" : "";
        m_baseToString[(int)BASE_DEFINES.INAIR_DASH] = HasAnimation("InAirDash", m_layerToInt[(int)LAYER.BASE]) ? "InAirDash" : "";
        m_baseToString[(int)BASE_DEFINES.LANDING_TO_IDLE] = HasAnimation("LandingToIdle", m_layerToInt[(int)LAYER.BASE]) ? "LandingToIdle" : "";
        m_baseToString[(int)BASE_DEFINES.LANDING_TO_RUN] = HasAnimation("LandingToRun", m_layerToInt[(int)LAYER.BASE]) ? "LandingToRun" : "";
        m_baseToString[(int)BASE_DEFINES.WALL_JUMP] = HasAnimation("WallJump", m_layerToInt[(int)LAYER.BASE]) ? "WallJump" : "";
        m_baseToString[(int)BASE_DEFINES.BLOCK] = HasAnimation("Block", m_layerToInt[(int)LAYER.BASE]) ? "Block" : "";
        m_baseToString[(int)BASE_DEFINES.BLOCK_FROM_IDLE] = HasAnimation("BlockFromIdle", m_layerToInt[(int)LAYER.BASE]) ? "BlockFromIdle" : "";
        m_baseToString[(int)BASE_DEFINES.BLOCK_TO_IDLE] = HasAnimation("BlockToIdle", m_layerToInt[(int)LAYER.BASE]) ? "BlockToIdle" : "";
        m_baseToString[(int)BASE_DEFINES.PRAY_START] = HasAnimation("PrayStart", m_layerToInt[(int)LAYER.BASE]) ? "PrayStart" : "";
        m_baseToString[(int)BASE_DEFINES.PRAY_LOOP] = HasAnimation("PrayLoop", m_layerToInt[(int)LAYER.BASE]) ? "PrayLoop" : "";
        m_baseToString[(int)BASE_DEFINES.PRAY_END] = HasAnimation("PrayEnd", m_layerToInt[(int)LAYER.BASE]) ? "PrayEnd" : "";

        m_interruptToString[(int)INTERRUPT_DEFINES.RECOIL] = HasAnimation("Recoil", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Recoil" : "";
        m_interruptToString[(int)INTERRUPT_DEFINES.KNOCKBACK] = HasAnimation("Knockback", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Knockback" : "";
        m_interruptToString[(int)INTERRUPT_DEFINES.KNOCKFORWARD] = HasAnimation("Knockforward", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Knockforward" : "";
        m_interruptToString[(int)INTERRUPT_DEFINES.DEATH] = HasAnimation("Death", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Death" : "";
        m_interruptToString[(int)INTERRUPT_DEFINES.IDLE_EMOTE] = HasAnimation("IdleEmote", m_layerToInt[(int)LAYER.INTERRUPT]) ? "IdleEmote" : "";
    }
}
