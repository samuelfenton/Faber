﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Player : Character
{
    //-------------------
    //Character setup
    //  Ensure all need componets are attached, and get initilised if needed
    //-------------------
    protected override void Start()
    {
        base.Start();
    }

    //-------------------
    //Character update
    //  Get input, apply physics, update character state machine
    //  Update voxel trails
    //-------------------
    protected override void Update()
    {
        base.Update();
    }

    //-------------------
    //Get turning direction for junction navigation, based off current input
    //
    //Param p_trigger: junction character will pass through
    //
    //Return NavigationController.TURNING: Path character will desire to take
    //-------------------
    public override TURNING_DIR GetDesiredTurning(Navigation_Trigger_Junction p_trigger)
    {
        float relativeDot = Vector3.Dot(transform.forward, p_trigger.transform.forward);

        float verticalInput = m_currentCharacterInput.m_vertical;

        if(relativeDot >= 0)//Right is positive on vertical, left is negative
        {
            if (verticalInput < 0)
                return TURNING_DIR.RIGHT;
            if (verticalInput > 0)
                return TURNING_DIR.LEFT;
        }
        else//Right is negative on vertical, left is positive
        {
            if (verticalInput < 0)
                return TURNING_DIR.LEFT;
            if (verticalInput > 0)
                return TURNING_DIR.RIGHT;
        }
        return TURNING_DIR.CENTER;
    }
}
