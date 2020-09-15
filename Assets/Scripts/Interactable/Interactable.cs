using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Interactable : MonoBehaviour
{
    [Header("Assigned Variables")]
    public float m_activationDistance = 2.0f;
    public GameObject m_interactText = null;
    private float m_sqrActivationDistance = 4.0f;

    [Header("Auto generated")]
    public int m_uniqueID = -1;

    protected Character_Player m_playerCharacter = null;
    private bool m_interactableFlag = false;

    /// <summary>
    /// Setup a unique id for every interactable
    /// </summary>
    protected virtual void Awake()
    {
        m_uniqueID = 0;
        Interactable[] allInteractables = FindObjectsOfType<Interactable>();
        for (int interactableIndex = 0; interactableIndex < allInteractables.Length; interactableIndex++)
        {
            if (allInteractables[interactableIndex] != this && allInteractables[interactableIndex].m_uniqueID >= m_uniqueID)
                m_uniqueID = allInteractables[interactableIndex].m_uniqueID + 1;
        }
    }

    /// <summary>
    /// Initilise the interactable
    /// </summary>
    /// <param name="p_player">Player character</param>
    public virtual void InitInteractable(Character_Player p_player)
    {
        m_playerCharacter = p_player;

        m_sqrActivationDistance = m_activationDistance * m_activationDistance;
        m_interactableFlag = false;

        if (m_interactText != null)
            m_interactText.SetActive(false);
    }

    /// <summary>
    /// Used rather than update
    /// Checks for player interaction
    /// </summary>
    public virtual void UpdateInteractable()
    {
        if(m_interactableFlag)
        {
            if(!ValidInteraction())
            {
                InteractEnd();
                m_interactableFlag = false;

            }
            else if(m_playerCharacter.m_customInput.GetKey(CustomInput.INPUT_KEY.INTERACT) == CustomInput.INPUT_STATE.DOWNED)
            {
                Interact();
            }
        }
        else
        {
            if (ValidInteraction())
            {
                InteractStart();
                m_interactableFlag = true;
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
    /// Player can no longer intereact, moved out of distance
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
        return MOARMaths.SqrDistance(m_playerCharacter.transform.position, transform.position) < m_sqrActivationDistance;
    }

}
