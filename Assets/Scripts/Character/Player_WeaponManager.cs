using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_WeaponManager : WeaponManager
{
    //Stored varibles
    private Player_Character m_playerCharacter = null;

    /// <summary>
    /// Init manager
    /// </summary>
    /// <param name="p_character">Character that uses this manager</param>
    public override void Init(Character p_character)
    {
        base.Init(p_character);
        m_playerCharacter = p_character.GetComponent<Player_Character>();
    }

    /// <summary>
    /// Update the manoeuvre
    /// </summary>
    /// <returns>True when completed</returns>
    protected override bool UpdateAttackManoeuvre()
    {

        switch (m_currentState)
        {
            case ATTACK_MANOEUVRE_STATE.WIND_UP:
                if (AnimController.GetAnimationPercent(m_animator) > m_startDamage)
                    m_currentState = ATTACK_MANOEUVRE_STATE.DAMAGE;
                break;
            case ATTACK_MANOEUVRE_STATE.DAMAGE:
                UpdateWeaponDamage();
                if (AnimController.GetAnimationPercent(m_animator) > m_endDamage)
                    m_currentState = ATTACK_MANOEUVRE_STATE.AWAITING_COMBO;
                break;
            case ATTACK_MANOEUVRE_STATE.AWAITING_COMBO:
                if (AnimController.GetAnimationPercent(m_animator) > m_startCombo) 
                    m_currentState = ATTACK_MANOEUVRE_STATE.COMBO_CHECK;
                break;
            case ATTACK_MANOEUVRE_STATE.COMBO_CHECK:
                if (AnimController.GetAnimationPercent(m_animator) > m_endCombo) 
                    m_currentState = ATTACK_MANOEUVRE_STATE.END_ATTACK;
                if (m_playerCharacter.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK) == CustomInput.INPUT_STATE.DOWNED)
                {
                    m_comboFlag = true;
                    m_attackStance = AnimController.ATTACK_STANCE.LIGHT;
                    m_currentState = ATTACK_MANOEUVRE_STATE.END_ATTACK;
                }
                if (m_playerCharacter.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK_SECONDARY) == CustomInput.INPUT_STATE.DOWNED)
                {
                    m_comboFlag = true;
                    m_attackStance = AnimController.ATTACK_STANCE.HEAVY;
                    m_currentState = ATTACK_MANOEUVRE_STATE.END_ATTACK;
                }
                break;
            case ATTACK_MANOEUVRE_STATE.END_ATTACK:
                return (m_comboFlag || AnimController.IsAnimationDone(m_animator));
        }

        return false;
    }


    #region HELPER FUNCTIONS
    /// <summary>
    /// Determine what attack type should be performed?
    /// grounded and sprinting = sprinting
    /// just grounded = grounded
    /// in the air = in_air
    /// </summary>
    /// <returns>Correct type, defualt to grounded</returns>
    public override AnimController.ATTACK_TYPE DetermineAttackType()
    {
        if (!m_character.m_splinePhysics.m_downCollision) //In the air
        {
            return AnimController.ATTACK_TYPE.IN_AIR;
        }
        if (m_playerCharacter.m_input.GetKeyBool(CustomInput.INPUT_KEY.SPRINT))//Is it sprinting or just grounded
            return AnimController.ATTACK_TYPE.SPRINTING;

        return AnimController.ATTACK_TYPE.GROUND;
    }

    /// <summary>
    /// Determine attacking stance, light or heavy
    /// </summary>
    /// <returns>Based off input</returns>
    public override AnimController.ATTACK_STANCE DetermineStance()
    {
        return m_playerCharacter.m_input.GetKey(CustomInput.INPUT_KEY.ATTACK) == CustomInput.INPUT_STATE.DOWNED ? AnimController.ATTACK_STANCE.LIGHT : AnimController.ATTACK_STANCE.HEAVY;
    }
    #endregion
}
