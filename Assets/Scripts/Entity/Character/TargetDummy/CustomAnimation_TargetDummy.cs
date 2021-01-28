using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimation_TargetDummy : CustomAnimation
{
    public enum VARIBLE_FLOAT { KNOCKBACK_IMPACT, COUNT }
    public enum INTERRUPT_DEFINES { KNOCKBACK, COUNT }

    /// <summary>
    /// Setup dicionaries used
    /// </summary>
    /// <param name="p_animator">Animator to check against</param>
    public override void Init(Animator p_animator)
    {
        base.Init(p_animator);

        m_floatToString = new string[(int)VARIBLE_FLOAT.COUNT];
        m_baseToString = new string[0];
        m_interruptToString = new string[(int)INTERRUPT_DEFINES.COUNT];

        m_floatToString[(int)VARIBLE_FLOAT.KNOCKBACK_IMPACT] = ContainsParam(m_animator, "KnockbackImpact") ? "KnockbackImpact" : "";

        m_interruptToString[(int)INTERRUPT_DEFINES.KNOCKBACK] = HasAnimation(m_animator, "Knockback", m_layerToInt[(int)LAYER.INTERRUPT]) ? "Knockback" : "";
    }
}
