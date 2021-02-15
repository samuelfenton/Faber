using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation_Drone01 : CustomAnimation
{
    public enum VARIBLE_FLOAT { CURRENT_VELOCITY, KNOCKBACK_IMPACT, COUNT }
    public enum BASE_DEFINES { LOCOMOTION, COUNT }
    public enum INTERRUPT_DEFINES {KNOCKBACK, KNOCKFORWARD, DEATH, COUNT }

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
        m_floatToString[(int)VARIBLE_FLOAT.KNOCKBACK_IMPACT] = ContainsParam("KnockbackImpact") ? "KnockbackImpact" : "";

        m_baseToString[(int)BASE_DEFINES.LOCOMOTION] = HasAnimation("Locomotion", m_layerToInt[(int)LAYER.BASE]) ? "Locomotion" : "";

        m_interruptToString[(int)INTERRUPT_DEFINES.KNOCKBACK] = HasAnimation("Knockback", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Knockback" : "";
        m_interruptToString[(int)INTERRUPT_DEFINES.KNOCKFORWARD] = HasAnimation("Knockforward", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Knockforward" : "";
        m_interruptToString[(int)INTERRUPT_DEFINES.DEATH] = HasAnimation("Death", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Death" : "";
    }
}
