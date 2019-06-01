using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_StateCondition : MonoBehaviour
{
    //-------------------
    //Setup condition for future use
    //-------------------
    public virtual void Init(Character_NPC p_character)
    {

    }

    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public virtual bool Execute(Character_NPC p_character)
    {
        return false;
    }
}
