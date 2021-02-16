using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation_TargetDummy : CustomAnimation
{
    public enum VARIBLE_FLOAT { KNOCKBACK_IMPACT, COUNT }
    public enum INTERRUPT_DEFINES { KNOCKBACK, KNOCKFORWARD, COUNT }

    /// <summary>
    /// Setup dicionaries used
    /// </summary>
    public override void Init()
    {
        base.Init();

        m_floatToString = new string[(int)VARIBLE_FLOAT.COUNT];
        m_baseToString = new string[0];
        m_interruptToString = new string[(int)INTERRUPT_DEFINES.COUNT];

        m_floatToString[(int)VARIBLE_FLOAT.KNOCKBACK_IMPACT] = ContainsParam("KnockbackImpact") ? "KnockbackImpact" : "";

        m_interruptToString[(int)INTERRUPT_DEFINES.KNOCKBACK] = HasAnimation("Knockback", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Knockback" : "";
        m_interruptToString[(int)INTERRUPT_DEFINES.KNOCKFORWARD] = HasAnimation("Knockforward", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Knockforward" : "";

    }
}
