using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState_HeavyAttackCombo : CharacterState_BaseAttack
{
    private bool m_delayTrigger = false;

    //-------------------
    //When swapping to this state, this is called.
    //-------------------
    public override void StateStart()
    {
        m_parentCharacter.m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.HEAVY_ATTACK_COMBO, true);
        m_parentCharacter.m_currentAttackType = Character.ATTACK_TYPE.HEAVY;

        m_delayTrigger = false;
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

        if (!m_delayTrigger && !(m_characterAnimationController.EndOfAnimation() || m_characterAnimationController.m_canCombo))//Wait for everything to be set to false
        {
            m_delayTrigger = true;
        }

        return m_characterAnimationController.EndOfAnimation() || m_characterAnimationController.m_canCombo;
    }

    //-------------------
    //When swapping to a new state, this is called.
    //-------------------
    public override void StateEnd()
    {
        m_characterAnimationController.SetBool(CharacterAnimationController.ANIMATIONS.HEAVY_ATTACK_COMBO, false);
    }

    //-------------------
    //Do all of this states preconditions return true
    //
    //Return bool: Is this valid, e.g. Death requires players to have no health
    //-------------------
    public override bool IsValid()
    {
        return m_characterInput.GetInputState().m_heavyCombo && m_characterAnimationController.m_canCombo;
    }
}
