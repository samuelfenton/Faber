﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState_Idle : NPC_State
{
    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public override void StateStart()
    {

    }

    //-------------------
    //State update, perform any actions for the given state
    //
    //Return bool: Has this state been completed, e.g. Attack has completed, idle would always return true 
    //-------------------
    public override bool UpdateState()
    {
        return true;
    }

    //-------------------
    //When swapping to a new state, this is called.
    //-------------------
    public override void StateEnd()
    {

    }
}