using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_StateCondition_Distance : NPC_StateCondition
{
    public float m_closeEnoughDistance = 0.1f;
    private float m_closeEnoughDistanceSqr = 0.0f;

    //-------------------
    //Setup condition for future use
    //-------------------
    public override void Init(Character_NPC p_character)
    {
        m_closeEnoughDistanceSqr = m_closeEnoughDistance * m_closeEnoughDistance;
    }

    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public override bool Execute(Character_NPC p_character)
    {
        return (p_character.m_targetCharacter != null && MOARMaths.SqrDistance(p_character.transform.position, p_character.m_targetCharacter.transform.position) < m_closeEnoughDistanceSqr);
    }
}
