using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourNode : MonoBehaviour
{
    public enum RESULT {SUCCESS, FAILED, PENDING };

    public virtual RESULT Execute(Character_NPC p_character)
    {
        return RESULT.FAILED;
    } 
}
