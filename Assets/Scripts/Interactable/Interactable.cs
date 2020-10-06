using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueID))]
public class Interactable : MonoBehaviour
{
    private const float SQR_ACTIVATION_DISTANCE = 4.0f;
    
    [Header("Assigned Variables")]
    public GameObject m_interactText = null;

    [HideInInspector]
    public UniqueID m_uniqueID = null;

    protected Character_Player m_playerCharacter = null;
    private bool m_previouslyCloseFlag = false;

    /// <summary>
    /// Initilise the interactable
    /// </summary>
    /// <param name="p_player">Player character</param>
    public virtual void InitInteractable(Character_Player p_player)
    {
        m_uniqueID = GetComponent<UniqueID>();
        
        m_playerCharacter = p_player;

        m_previouslyCloseFlag = false;

        if (m_interactText != null)
            m_interactText.SetActive(false);
    }

    /// <summary>
    /// Used rather than update
    /// Checks for player interaction
    /// </summary>
    public void UpdateInteractable()
    {
        if(!ValidInteraction()) //Not Valid, early breakout
        {
            return;
        }

        float distance = MOARMaths.SqrDistance(transform.position, m_playerCharacter.transform.position);

        if (m_previouslyCloseFlag)
        {
            if(distance > SQR_ACTIVATION_DISTANCE) //No longer close enough
            {
                m_previouslyCloseFlag = false;
                m_playerCharacter.RemoveCurrentInteractable(this);
                return;
            }

            m_playerCharacter.UpdateCurrentInteractable(this, distance);

            if(m_playerCharacter.m_currentInteractable == this && m_playerCharacter.m_customInput.GetKey(CustomInput.INPUT_KEY.INTERACT) == CustomInput.INPUT_STATE.DOWNED)
                Interact();
        }
        else
        {
            if (distance < SQR_ACTIVATION_DISTANCE) //No close enough
            {
                m_previouslyCloseFlag = true;
            }
        }
    }

    /// <summary>
    /// Player first can interact
    /// </summary>
    public virtual void InteractStart()
    {
        if (m_interactText != null)
            m_interactText.SetActive(true);
    }

    /// <summary>
    /// Called when player tries to interact
    /// </summary>
    public virtual void Interact()
    { 
    
    }
    
    /// <summary>
    /// Player can no longer interact, moved out of distance
    /// </summary>
    public virtual void InteractEnd()
    {
        if (m_interactText != null)
            m_interactText.SetActive(false);
    }

    /// <summary>
    /// Can the player interact with this interactable?
    /// Will be contstantly checked
    /// </summary>
    /// <returns>true when able to interact</returns>
    public virtual bool ValidInteraction()
    {
        return false;
    }
}
