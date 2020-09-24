using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    private enum MANOEUVRE_STATE {AWAITING_ATTACK, PERFORMING_ATTACK, COMPLETED_ATTACK}
    private enum SEQUENCE_STATE { INITIAL, ATTACK, END}

    [Header("Manoeuvre Controller Prefabs")]
    [Header("Ground")]
    public GameObject m_groundLightControllerPrefab = null;
    public GameObject m_groundHeavyControllerPrefab = null;

    [Header("In Air")]
    public GameObject m_inAirLightControllerPrefab = null;
    public GameObject m_inAirHeavyControllerPrefab = null;

    [Header("Sprinting")]
    public GameObject m_sprintingLightControllerPrefab = null;
    public GameObject m_sprintingHeavyControllerPrefab = null;

    //Stored scripts
    private ManoeuvreController m_groundLightController = null;
    private ManoeuvreController m_groundHeavyController = null;

    private ManoeuvreController m_inAirLightController = null;
    private ManoeuvreController m_inAirHeavyController = null;

    private ManoeuvreController m_sprintingLightController = null;
    private ManoeuvreController m_sprintingHeavyController = null;

    //Stored varibles
    private ManoeuvreController m_currentController = null;

    private Character m_character = null;
    private CustomAnimation m_customAnimation = null;

    //Manoeuvre Varibles
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

        m_groundLightController = BuildHitboxController(m_groundLightControllerPrefab);
        m_groundHeavyController = BuildHitboxController(m_groundHeavyControllerPrefab);
        m_inAirLightController = BuildHitboxController(m_inAirLightControllerPrefab);
        m_inAirHeavyController = BuildHitboxController(m_inAirHeavyControllerPrefab);
        m_sprintingLightController = BuildHitboxController(m_sprintingLightControllerPrefab);
        m_sprintingHeavyController = BuildHitboxController(m_sprintingHeavyControllerPrefab);
    }

    /// <summary>
    /// Recursive instantiation of prefab hitbox colliders
    /// </summary>
    /// <param name="p_hitboxPrefab"></param>
    /// <returns></returns>
    private ManoeuvreController BuildHitboxController(GameObject p_hitboxPrefab)
    {
        if(p_hitboxPrefab != null)
        {
            GameObject newHitboxController = Instantiate(p_hitboxPrefab, m_character.transform, false);

            ManoeuvreController controller = newHitboxController.GetComponent<ManoeuvreController>();

            if(controller == null)
            {
                Destroy(newHitboxController);
                return null;
            }

            controller.InitController(m_character);
            
            controller.m_lightBranchController = BuildHitboxController(controller.m_lightBranchPrefab);
            controller.m_heavyBranchController = BuildHitboxController(controller.m_heavyBranchPrefab);

            controller.gameObject.SetActive(false);

            return controller;
        }

        return null;
    }

    /// <summary>
    /// Start the tree of attacks
    /// </summary>
    /// <param name="p_initialType">What the first attack type is</param>
    /// <param name="p_intialStance">What the first attack stance is</param>
    public void StartAttack(ManoeuvreController.MANOEUVRE_TYPE p_initialType, ManoeuvreController.MANOEUVRE_STANCE p_intialStance)
    {
        //Reset details
        m_currentController = null;

        switch (p_initialType)
        {
            case ManoeuvreController.MANOEUVRE_TYPE.GROUND:
                switch (p_intialStance)
                {
                    case ManoeuvreController.MANOEUVRE_STANCE.LIGHT:
                        m_currentController = m_groundLightController;
                        break;
                    case ManoeuvreController.MANOEUVRE_STANCE.HEAVY:
                        m_currentController = m_groundHeavyController;
                        break;
                    default:
                        m_currentController = null;
                        break;
                }
                break;
            case ManoeuvreController.MANOEUVRE_TYPE.INAIR:
                switch (p_intialStance)
                {
                    case ManoeuvreController.MANOEUVRE_STANCE.LIGHT:
                        m_currentController = m_inAirLightController;
                        break;
                    case ManoeuvreController.MANOEUVRE_STANCE.HEAVY:
                        m_currentController = m_inAirHeavyController;
                        break;
                    default:
                        m_currentController = null;
                        break;
                }
                break;
            case ManoeuvreController.MANOEUVRE_TYPE.SPRINT:
                switch (p_intialStance)
                {
                    case ManoeuvreController.MANOEUVRE_STANCE.LIGHT:
                        m_currentController = m_sprintingLightController;
                        break;
                    case ManoeuvreController.MANOEUVRE_STANCE.HEAVY:
                        m_currentController = m_sprintingHeavyController;
                        break;
                    default:
                        m_currentController = null;
                        break;
                }
                break;
            default:
                break;
        }

        if (m_currentController != null)
            StartManoeuvre();
    }

    /// <summary>
    /// Update a tree of attacks
    /// </summary>
    /// <returns>True once a tree has completed</returns>
    public bool UpdateAttack()
    {
        if (m_currentController == null)
        {
            EndAttack();
            return true;
        }

        if(UpdateManoeuvre())
        {
            EndManoeuvre();

            ManoeuvreController nextManoeuvre;

            switch (m_nextAttackStance)
            {
                case Character.ATTACK_INPUT_STANCE.LIGHT:
                    nextManoeuvre = m_currentController.m_lightBranchController;
                    break;
                case Character.ATTACK_INPUT_STANCE.HEAVY:
                    nextManoeuvre = m_currentController.m_heavyBranchController;
                    break;
                case Character.ATTACK_INPUT_STANCE.NONE:
                default:
                    nextManoeuvre = null;
                    break;
            }

            if (nextManoeuvre != null)
            {
                m_currentController = nextManoeuvre;
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
        if(m_currentController != null)
            m_customAnimation.EndAttack(m_currentController.m_requiresSheathingBlend);
        
        m_currentController = null;
    }

    /// <summary>
    /// Start of a single tree attack AKA Attack Manoeuvre
    /// </summary>
    public void StartManoeuvre()
    {
        m_nextAttackStance = Character.ATTACK_INPUT_STANCE.NONE;

        m_currentSequenceState = SEQUENCE_STATE.INITIAL;

        m_attackTimer = 0.0f;

        m_currentController.gameObject.SetActive(true);

        m_character.m_gravity = m_currentController.m_useGravity;
        if(!m_currentController.m_useGravity)
        {
            m_character.m_splinePhysics.HardSetUpwardsVelocity(0.0f);
        }

        m_customAnimation.PlayAttack(m_currentController.m_animationString);

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

        m_character.SplineTranslate(m_currentController.m_translationXCurve.Evaluate(currentPercent) * Time.deltaTime);
        
            //Running through initial sequence, that is the start of an attack, wait till completed
        if(m_currentController.m_sequenceAttack)
        {
            switch (m_currentSequenceState)
            {
                case SEQUENCE_STATE.INITIAL:
                    if (m_customAnimation.IsAnimationDone(CustomAnimation.LAYER.ATTACK))
                    {
                        m_currentSequenceState = SEQUENCE_STATE.ATTACK;
                        m_customAnimation.PlayAttackSection01(m_currentController.m_animationString);
                    }
                    break;
                case SEQUENCE_STATE.ATTACK:
                    if (CompletedManoeuvreSequence())
                    {
                        m_currentSequenceState = SEQUENCE_STATE.END;
                        m_customAnimation.PlayAttackSection02(m_currentController.m_animationString);
                    }
                    break;
                case SEQUENCE_STATE.END:
                    break;
                default:
                    break;
            }
        }

        if(m_nextAttackStance == Character.ATTACK_INPUT_STANCE.NONE && currentPercent > 0.3f) //Get next input
        {
            Character.ATTACK_INPUT_STANCE nextAttackStance = m_character.DetermineAttackStance();

            if (m_currentController.m_lightBranchController != null && nextAttackStance == Character.ATTACK_INPUT_STANCE.LIGHT)
                m_nextAttackStance = Character.ATTACK_INPUT_STANCE.LIGHT;
            else if (m_currentController.m_heavyBranchController != null && nextAttackStance == Character.ATTACK_INPUT_STANCE.HEAVY)
                m_nextAttackStance = Character.ATTACK_INPUT_STANCE.HEAVY;
        }

        return currentPercent >= 0.99f && (!m_currentController.m_sequenceAttack || m_currentSequenceState == SEQUENCE_STATE.END);
    }

    /// <summary>
    /// Attack has ended ensure all colldiers are disabled etc
    /// </summary>
    public void EndManoeuvre()
    {
        m_currentController.gameObject.SetActive(false);

        m_character.m_gravity = true;
    }

    public bool CompletedManoeuvreSequence()
    {
        //Grounded
        if (m_currentController.m_groundedFlag && !m_character.m_splinePhysics.m_downCollision)
            return false;

        //Time
        if (m_currentController.m_timeTraveledFlag && !(m_attackTimer < m_currentController.m_requiredAttackTime))
            return false;

        return true;
    }
}
