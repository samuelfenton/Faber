using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierLog : Modifier
{
    public string m_logMessage = "";

    public override RESULT Execute(Character_NPC p_character)
    {
        #if UNITY_EDITOR
        Debug.Log(m_logMessage);
        #endif

        return m_childBehaviour.Execute(p_character);
    }
}
