using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private enum MANOEUVRE_STATE {AWAITING_ATTACK, PERFORMING_ATTACK, COMPLETED_ATTACK}
    private enum SEQUENCE_STATE { INITIAL, ATTACK, END}

    public GameObject m_primaryWeaponPrefab = null;
    public GameObject m_secondaryWeaponPrefab = null;

    [Header("Intial manoeuvre leaves")]
    [Header("Ground")]
    public ManoeuvreLeaf m_groundLightManoeuvre = null;
    public ManoeuvreLeaf m_groundHeavyManoeuvre = null;

    [Header("In Air")]
    public ManoeuvreLeaf m_inAirLightManoeuvre = null;
    public ManoeuvreLeaf m_inAirHeavyManoeuvre = null;

    [Header("Sprinting")]
    public ManoeuvreLeaf m_sprintingLightManoeuvre = null;
    public ManoeuvreLeaf m_sprintingHeavyManoeuvre = null;

    //Stored varibles
    private ManoeuvreLeaf m_currentManoeuvreLeaf = null;

    private GameObject m_primaryWeaponObject = null;
    private GameObject m_secondaryWeaponObject = null;

    private Weapon m_primaryWeaponScript = null;
    private Weapon m_secondaryWeaponScript = null;

    private Character m_character = null;
    private CustomAnimation m_customAnimation = null;

    //Manoeuvre Varibles
    private MANOEUVRE_STATE m_currentManoeuvreState = MANOEUVRE_STATE.AWAITING_ATTACK;
    private int m_manoeuvreActionIndex = 0;
    private Character.ATTACK_INPUT_STANCE m_nextAttackStance = Character.ATTACK_INPUT_STANCE.NONE;

    //Sequence Attack Variables
    private SEQUENCE_STATE m_currentSequenceState = SEQUENCE_STATE.INITIAL;
    private float m_attackTimer = 0.0f;

    /// <summary>
    /// Init manager
    /// </summary>
    /// <param name="p_character">Character that uses this manager</param>
    public virtual void Init(Character p_character)
    {
        m_character = p_character;
        m_customAnimation = p_character.GetComponentInChildren<CustomAnimation>();

        if (m_primaryWeaponPrefab != null)
        {
            m_primaryWeaponObject = Instantiate(m_primaryWeaponPrefab);

            m_primaryWeaponScript = m_primaryWeaponObject.GetComponent<Weapon>();
            m_primaryWeaponObject.transform.SetParent(m_character.m_primaryAnchor.transform, false);
        }
        if (m_secondaryWeaponPrefab != null)
        {
            m_secondaryWeaponObject = Instantiate(m_secondaryWeaponPrefab);

            m_secondaryWeaponScript = m_secondaryWeaponObject.GetComponent<Weapon>();
            m_secondaryWeaponObject.transform.SetParent(m_character.m_secondaryAnchor.transform, false);
        }

        if (m_primaryWeaponScript != null)
        {
            m_primaryWeaponScript.Init(p_character);
        }
        if (m_secondaryWeaponScript != null)
        {
            m_secondaryWeaponScript.Init(p_character);
        }
    }

    /// <summary>
    /// Start the tree of attacks
    /// </summary>
    /// <param name="p_initialType">What the first attack type is</param>
    /// <param name="p_intialStance">What the first attack stance is</param>
    public void StartAttack(ManoeuvreLeaf.MANOEUVRE_TYPE p_initialType, ManoeuvreLeaf.MANOEUVRE_STANCE p_intialStance)
    {
        //Reset details
        m_currentManoeuvreLeaf = null;

        switch (p_initialType)
        {
            case ManoeuvreLeaf.MANOEUVRE_TYPE.GROUND:
                switch (p_intialStance)
                {
                    case ManoeuvreLeaf.MANOEUVRE_STANCE.LIGHT:
                        m_currentManoeuvreLeaf = m_groundLightManoeuvre;
                        break;
                    case ManoeuvreLeaf.MANOEUVRE_STANCE.HEAVY:
                        m_currentManoeuvreLeaf = m_groundHeavyManoeuvre;
                        break;
                    default:
                        m_currentManoeuvreLeaf = null;
                        break;
                }
                break;
            case ManoeuvreLeaf.MANOEUVRE_TYPE.INAIR:
                switch (p_intialStance)
                {
                    case ManoeuvreLeaf.MANOEUVRE_STANCE.LIGHT:
                        m_currentManoeuvreLeaf = m_inAirLightManoeuvre;
                        break;
                    case ManoeuvreLeaf.MANOEUVRE_STANCE.HEAVY:
                        m_currentManoeuvreLeaf = m_inAirHeavyManoeuvre;
                        break;
                    default:
                        m_currentManoeuvreLeaf = null;
                        break;
                }
                break;
            case ManoeuvreLeaf.MANOEUVRE_TYPE.SPRINT:
                switch (p_intialStance)
                {
                    case ManoeuvreLeaf.MANOEUVRE_STANCE.LIGHT:
                        m_currentManoeuvreLeaf = m_sprintingLightManoeuvre;
                        break;
                    case ManoeuvreLeaf.MANOEUVRE_STANCE.HEAVY:
                        m_currentManoeuvreLeaf = m_sprintingHeavyManoeuvre;
                        break;
                    default:
                        m_currentManoeuvreLeaf = null;
                        break;
                }
                break;
            default:
                break;
        }

        if (m_currentManoeuvreLeaf != null)
            StartManoeuvre();
    }

    /// <summary>
    /// Update a tree of attacks
    /// </summary>
    /// <returns>True once a tree has completed</returns>
    public bool UpdateAttack()
    {
        if (m_currentManoeuvreLeaf == null)
        {
            EndAttack();
            return true;
        }

        if(UpdateManoeuvre())
        {
            EndManoeuvre();

            ManoeuvreLeaf nextManoeuvreLeaf;

            switch (m_nextAttackStance)
            {
                case Character.ATTACK_INPUT_STANCE.LIGHT:
                    nextManoeuvreLeaf = m_currentManoeuvreLeaf.m_lightBranch;
                    break;
                case Character.ATTACK_INPUT_STANCE.HEAVY:
                    nextManoeuvreLeaf = m_currentManoeuvreLeaf.m_heavyBranch;
                    break;
                case Character.ATTACK_INPUT_STANCE.NONE:
                default:
                    nextManoeuvreLeaf = null;
                    break;
            }

            if (nextManoeuvreLeaf != null)
            {
                m_currentManoeuvreLeaf = nextManoeuvreLeaf;
                StartManoeuvre();
            }
            else
            {
                EndAttack();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Attack has ended ensure all colldiers are disabled etc
    /// </summary>
    public void EndAttack()
    {
        if(m_currentManoeuvreLeaf != null)
            m_customAnimation.EndAttack(m_currentManoeuvreLeaf.m_requiresSheathingBlend);
        
        m_currentManoeuvreLeaf = null;
    }

    /// <summary>
    /// Start of a single tree attack AKA Attack Manoeuvre
    /// </summary>
    public void StartManoeuvre()
    {
        m_nextAttackStance = Character.ATTACK_INPUT_STANCE.NONE;
        m_manoeuvreActionIndex = 0;

        m_currentManoeuvreState = MANOEUVRE_STATE.AWAITING_ATTACK;
        m_currentSequenceState = SEQUENCE_STATE.INITIAL;

        m_attackTimer = 0.0f;

        m_customAnimation.PlayAttack(m_currentManoeuvreLeaf);

        if (m_currentManoeuvreLeaf.m_manoeuvreActions.Count == 0) //Early stop 
            m_currentManoeuvreState = MANOEUVRE_STATE.COMPLETED_ATTACK;
    }

    /// <summary>
    /// Updates the manoeuvre
    /// Should toggle colliders, update weapons as needed
    /// </summary>
    /// <returns>True when a manoeuvre is completed</returns>
    public bool UpdateManoeuvre()
    {
        m_attackTimer += Time.deltaTime; 

        float currentPercent = m_customAnimation.GetAnimationPercent(CustomAnimation.LAYER.ATTACK);

        //Running through initial sequence, that is the start of an attack, wait till completed
        if(m_currentManoeuvreLeaf.m_sequenceAttack)
        {
            switch (m_currentSequenceState)
            {
                case SEQUENCE_STATE.INITIAL:
                    if (m_customAnimation.IsAnimationDone(CustomAnimation.LAYER.ATTACK))
                    {
                        m_currentSequenceState = SEQUENCE_STATE.ATTACK;
                        m_customAnimation.PlayAttackSection01(m_currentManoeuvreLeaf);
                    }
                    break;
                case SEQUENCE_STATE.ATTACK:
                    if (CompletedManoeuvre())
                    {
                        m_currentSequenceState = SEQUENCE_STATE.END;
                        m_customAnimation.PlayAttackSection02(m_currentManoeuvreLeaf);
                    }
                    break;
                case SEQUENCE_STATE.END:
                    break;
                default:
                    break;
            }
        }

        if (m_currentManoeuvreState != MANOEUVRE_STATE.COMPLETED_ATTACK)
        {
            ManoeuvreLeaf.ManoeuvreAction nextAction = m_currentManoeuvreLeaf.m_manoeuvreActions[m_manoeuvreActionIndex];

            //Update manoeurvre "statemachine"
            switch (m_currentManoeuvreState)
            {
                case MANOEUVRE_STATE.AWAITING_ATTACK:

                    if (nextAction.m_damageStart <= currentPercent) //Can start damaging
                    {
                        //Setup weapons
                        if (nextAction.m_primaryUsage && m_primaryWeaponScript != null)
                            m_primaryWeaponScript.EnableWeaponDamage(nextAction.m_damageModifier);
                        if (nextAction.m_secondaryUsage && m_secondaryWeaponScript != null)
                            m_secondaryWeaponScript.EnableWeaponDamage(nextAction.m_damageModifier);

                        //Update "statemachine"
                        m_currentManoeuvreState = MANOEUVRE_STATE.PERFORMING_ATTACK;
                    }
                    break;
                case MANOEUVRE_STATE.PERFORMING_ATTACK:

                    if (nextAction.m_damageEnd <= currentPercent) //Can start damaging
                    {
                        //Setup weapons
                        if (nextAction.m_primaryUsage && m_primaryWeaponScript != null)
                            m_primaryWeaponScript.DisableWeaponDamage();
                        if (nextAction.m_secondaryUsage && m_secondaryWeaponScript != null)
                            m_secondaryWeaponScript.DisableWeaponDamage();

                        m_manoeuvreActionIndex++;

                        //Update "statemachine"
                        if (CompletedManoeuvre())
                        {
                            m_currentManoeuvreState = MANOEUVRE_STATE.COMPLETED_ATTACK;
                        }
                        else
                            m_currentManoeuvreState = MANOEUVRE_STATE.AWAITING_ATTACK;
                    }
                    break;
            }
        }

        if(m_nextAttackStance == Character.ATTACK_INPUT_STANCE.NONE && currentPercent > 0.1f) //Get next input
        {
            Character.ATTACK_INPUT_STANCE nextAttackStance = m_character.DetermineAttackStance();

            if (m_currentManoeuvreLeaf.m_lightBranch != null && nextAttackStance == Character.ATTACK_INPUT_STANCE.LIGHT)
                m_nextAttackStance = Character.ATTACK_INPUT_STANCE.LIGHT;
            else if (m_currentManoeuvreLeaf.m_heavyBranch != null && nextAttackStance == Character.ATTACK_INPUT_STANCE.HEAVY)
                m_nextAttackStance = Character.ATTACK_INPUT_STANCE.HEAVY;
        }

        return currentPercent >= 0.99f;
    }

    /// <summary>
    /// Attack has ended ensure all colldiers are disabled etc
    /// </summary>
    public void EndManoeuvre()
    {
        if (m_primaryWeaponScript != null)
            m_primaryWeaponScript.DisableWeaponDamage();
        if (m_secondaryWeaponScript != null)
            m_secondaryWeaponScript.DisableWeaponDamage();
    }

    public bool CompletedManoeuvre()
    {
        if(!m_currentManoeuvreLeaf.m_sequenceAttack) //Not a sequence attack, just return when at end
        {
            return m_manoeuvreActionIndex >= m_currentManoeuvreLeaf.m_manoeuvreActions.Count;
        }

        //Grounded
        if (m_currentManoeuvreLeaf.m_groundedFlag && !m_character.m_splinePhysics.m_downCollision)
            return false;

        //Time
        if (m_currentManoeuvreLeaf.m_timeTraveledFlag && !(m_attackTimer < m_currentManoeuvreLeaf.m_requiredAttackTime))
            return false;

        return true;
    }
}
