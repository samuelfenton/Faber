using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourNode : MonoBehaviour
{
    public enum RESULT {SUCCESS, FAILED, PENDING };

    protected CharacterInput_NPC m_characterInput_NPC = null;

    protected virtual void Start()
    {
        m_characterInput_NPC = GetComponent<CharacterInput_NPC>();
    }

    public virtual RESULT Execute(Character_NPC p_character)
    {
        return RESULT.FAILED;
    } 
}
