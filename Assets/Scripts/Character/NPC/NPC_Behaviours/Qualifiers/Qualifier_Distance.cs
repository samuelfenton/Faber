using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Qualifier_Distance : BehaviourNode
{
    public float m_closeEnoughDistance = 0.1f;
    private float m_closeEnoughDistanceSqr = 0.0f;

    public override void InitNode()
    {
        m_closeEnoughDistanceSqr = m_closeEnoughDistance * m_closeEnoughDistance;
    }

    public override RESULT Execute(Character_NPC p_character)
    {
        if (p_character.m_targetCharacter != null && MOARMaths.SqrDistance(p_character.transform.position, p_character.m_targetCharacter.transform.position) < m_closeEnoughDistanceSqr)
            return RESULT.SUCCESS;
        return RESULT.FAILED;
    }
}
