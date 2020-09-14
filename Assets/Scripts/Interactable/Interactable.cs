using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Interactable : MonoBehaviour
{
    public int m_uniqueID = -1;

    public float m_activationDistance = 2.0f;
    private float m_sqrActivationDistance = 4.0f;
    protected Character_Player m_playerCharacter = null;

    /// <summary>
    /// Setup a unique id for every interactable
    /// </summary>
    protected virtual void Awake()
    {
        if (m_uniqueID == -1) //Get new uniqueID, TODO make better
        {
            m_uniqueID = 0;
            Interactable[] allInteractables = FindObjectsOfType<Interactable>();
            for (int interactableIndex = 0; interactableIndex < allInteractables.Length; interactableIndex++)
            {
                if (allInteractables[interactableIndex] != this && allInteractables[interactableIndex].m_uniqueID >= m_uniqueID)
                    m_uniqueID = allInteractables[interactableIndex].m_uniqueID + 1;
            }
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
    }

    /// <summary>
    /// Used rather than update
    /// Checks for player interaction
    /// </summary>
    public virtual void UpdateInteractable()
    {
        if(MOARMaths.SqrDistance(m_playerCharacter.transform.position, transform.position) < m_sqrActivationDistance && m_playerCharacter.m_customInput.GetKey(CustomInput.INPUT_KEY.INTERACT) == CustomInput.INPUT_STATE.DOWNED)
        {
            Debug.Log("Can interact)");
            Interact();
        }
    }

    /// <summary>
    /// Called when player tries to interact
    /// </summary>
    public virtual void Interact()
    { 
    
    }

}
