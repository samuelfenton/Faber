﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Player : Character
{
    protected StateMachine_Player m_playerStateMachine = null;

    [HideInInspector]
    public CustomInput m_customInput = null;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();

        m_customInput = ((SceneController_InGame)MasterController.Instance.m_currentSceneController).m_customInput;

        //Init
        m_playerStateMachine = gameObject.AddComponent<StateMachine_Player>();

        m_playerStateMachine.InitStateMachine(this);//Run first as animation depends on states being created
    }

    protected override void Update()
    {
        base.Update();

        m_playerStateMachine.UpdateStateMachine();

        if(m_customInput.GetKey(CustomInput.INPUT_KEY.CAMERA_FLIP) == CustomInput.INPUT_STATE.DOWNED) //Flip camera
        {
            transform.rotation *= Quaternion.Euler(0.0f, 180.0f, 0.0f);
            m_characterModel.transform.rotation *= Quaternion.Euler(0.0f, 180.0f, 0.0f);
            m_localVelocity.x *= -1;
        }

    }

    /// <summary>
    /// Get turning direction for junction navigation, based off current input
    /// </summary>
    /// <param name="p_node">junction entity will pass through</param>
    /// <returns>Path entity will desire to take</returns>
    public override TURNING_DIR GetDesiredTurning(Pathing_Node p_node)
    {
        float relativeDot = Vector3.Dot(transform.forward, p_node.transform.forward);

        float verticalInput = m_customInput.GetAxis(CustomInput.INPUT_AXIS.VERTICAL);

        if (relativeDot >= 0)//Right is positive on vertical, left is negative
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

    #region WEAPON FUNCTIONS - OVERRIDE
    /// <summary>
    /// Function desired to be overridden, should this character be attempting to perform light or heavy attack
    /// Example click by player, or logic for NPC
    /// </summary>
    /// <returns>Light,heavy or none based off logic</returns>
    public override ATTACK_INPUT_STANCE DetermineAttackStance()
    {
        if(m_customInput.GetKeyBool(CustomInput.INPUT_KEY.LIGHT_ATTACK))
            return ATTACK_INPUT_STANCE.LIGHT;
        if (m_customInput.GetKeyBool(CustomInput.INPUT_KEY.HEAVY_ATTACK))
            return ATTACK_INPUT_STANCE.HEAVY;

        return ATTACK_INPUT_STANCE.NONE;
    }
    #endregion
}
