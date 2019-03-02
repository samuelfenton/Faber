using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : MonoBehaviour
{
    public BehaviourNode m_topNode = null;

    public void RunBehaviourTree(Character_NPC p_character)
    {
        m_topNode.Execute(p_character);
    }

}
