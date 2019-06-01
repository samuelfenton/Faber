using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Qualifier_AttackingDistance : BehaviourNode
{
    public float m_closeEnoughDistance = 0.1f;

    public override RESULT Execute(Character_NPC p_character)
    {
        //Kinematic equation
        // Vf^2 = Vi^2 + 2ad
        // d = -Vi^2 / 2a
        // As a is negitive -> d = Vi^2 / 2a
        float stoppingDistance = p_character.m_localVelocity.x == 0.0f ? 0 : Mathf.Pow(p_character.m_localVelocity.x, 2) / (2 * p_character.m_groundedHorizontalDeacceleration); //Close enough based off time to slow down

        if (p_character.m_targetCharacter != null && MOARMaths.SqrDistance(p_character.transform.position, p_character.m_targetCharacter.transform.position) <= Mathf.Pow(m_closeEnoughDistance + stoppingDistance,2))
            return RESULT.SUCCESS;
        return RESULT.FAILED;
    }
}
