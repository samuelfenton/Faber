using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState_Interrupt : NPC_State
{
    protected bool m_inProgressFlag = false;

    /// <summary>
    /// When swapping to this state, this is called.
    /// </summary>
    public override void StateStart()
    {
        base.StateStart();

        m_inProgressFlag = true;
    }

    /// <summary>
    /// When state has completed, this is called
    /// </summary>
    public override void StateEnd()
    {
        m_inProgressFlag = false;

        base.StateEnd();
    }
}
