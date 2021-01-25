﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Player : Character
{
    protected StateMachine_Player m_playerStateMachine = null;

    [Header("Saving")]
    public Interactable_SavePoint m_lastSavePoint = null;
    [HideInInspector]
    public CustomInput m_customInput = null;

    [Header("Auto Assigned Variables")]
    public bool m_atShrineFlag = false;

    [HideInInspector]
    public Interactable m_currentInteractable = null;

    /// <summary>
    /// Initiliase the entity
    /// setup varible/physics
    /// </summary>
    public override void InitEntity()
    {
        base.InitEntity();

        DataController.LoadCharacterStatistics(m_characterStatistics);

        m_customInput = ((SceneController_InGame)MasterController.Instance.m_currentSceneController).m_customInput;

        //Init
        m_playerStateMachine = gameObject.AddComponent<StateMachine_Player>();

        m_playerStateMachine.InitStateMachine(this);//Run first as animation depends on states being created
    }

    /// <summary>
    /// Update an entity, this should be called from scene controller
    /// Used to handle different scene state, pause vs in game etc
    /// </summary>
    public override void UpdateEntity()
    {
        if(m_customInput.GetKey(CustomInput.INPUT_KEY.CAMERA_FLIP) == CustomInput.INPUT_STATE.DOWNED) //Flip camera
        {
            m_followCamera.FlipCamera();
        }

        //Get logic
        m_playerStateMachine.UpdateStateMachine();
        
        base.UpdateEntity();
    }

    /// <summary>
    /// Apply the desired velocity
    /// </summary>
    /// <param name="p_allowSprinting">Should this allow a player to spring during this action</param>
    public void ApplyHorizontalMovement(bool p_allowSprinting)
    { 
        float horizontalInput = m_customInput.GetAxis(CustomInput.INPUT_AXIS.HORIZONTAL) * (m_followCamera.m_currentOrientation == FollowCamera.CAMERA_ORIENTATION.FORWARD ? 1.0f : -1.0f);

        //Allow player to jump and move
        if (p_allowSprinting && m_customInput.GetKeyBool(CustomInput.INPUT_KEY.SPRINT))
        {
            SetDesiredHorizontalVelocity(horizontalInput * m_groundRunVel * SPRINT_MODIFIER);
        }
        else
        {
            SetDesiredHorizontalVelocity(horizontalInput * m_groundRunVel);
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

    /// <summary>
    /// Given interactable is the current one, set interactable to null
    /// </summary>
    /// <param name="p_interactable">Interactable assumed to be current</param>
    public void RemoveCurrentInteractable(Interactable p_interactable)
    {
        if(m_currentInteractable == p_interactable)
        {
            m_currentInteractable.InteractEnd();
            m_currentInteractable = null;
        }
    }

    /// <summary>
    /// Update the possible current interactable
    /// </summary>
    /// <param name="p_interactable">New possible interactable</param>
    /// <param name="p_interactableDistance">Distance to new interactable</param>
    public void UpdateCurrentInteractable(Interactable p_interactable, float p_interactableDistance)
    {
        if (p_interactable == null) //Invalid variables
            return;

        if (m_currentInteractable == p_interactable) //Already current
            return;

        if(m_currentInteractable == null) //No assigned current, so go for this one
        {
            m_currentInteractable = p_interactable;
            p_interactable.InteractStart();
            return;
        }

        float currentDistance = MOARMaths.SqrDistance(m_currentInteractable.transform.position, transform.position);

        if(p_interactableDistance < currentDistance)//New interactable is closer, swap to this
        {
            m_currentInteractable.InteractEnd();
            m_currentInteractable = p_interactable;
            p_interactable.InteractStart();
        }
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
