using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Composite : BehaviourNode
{
    public List<BehaviourNode> m_childBehaviours = new List<BehaviourNode>();

    protected int? m_pendingIndex = null;
}
