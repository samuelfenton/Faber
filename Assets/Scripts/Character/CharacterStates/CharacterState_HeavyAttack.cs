using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState_HeavyAttack : CharacterState_BaseAttack
{
    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public override void StateStart()
    {
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.HEAVY_ATTACK, true);
        m_parentCharacter.m_currentAttackType = Character.ATTACK_TYPE.HEAVY;
    }

    //-------------------
    //State update, perform any actions for the given state
    //
    //Return bool: Has this state been completed, e.g. Attack has completed, idle would always return true 
    //-------------------
    public override bool UpdateState()
    {
        //Slowdown movement
        base.UpdateState();

        return m_characterAnimationController.EndOfAnimation() || m_characterAnimationController.m_canCombo;
    }

    //-------------------
    //When swapping to a new state, this is called.
    //-------------------
    public override void StateEnd()
    {
        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.HEAVY_ATTACK, false);
    }

    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public override bool IsValid()
    {
        return m_characterInput.GetInputState().m_heavyAttack;
    }
}
