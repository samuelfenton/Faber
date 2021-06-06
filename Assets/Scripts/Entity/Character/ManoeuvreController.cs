using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class ManoeuvreController : MonoBehaviour
{
    private enum SEQUENCE_STATE { INITIAL, ATTACK, END}
    [Header("Weapon Prefabs")]
    [Tooltip("Add weapon object into Prefab and assign here, This object will follow the primary anchor point")]
    public GameObject m_primaryWeaponObject = null;
    [Tooltip("Add weapon object into Prefab and assign here, This object will follow the secondary anchor point")]
    public GameObject m_secondaryWeaponObject = null;

    private SkinnedMeshRenderer m_primaryWeaponSkinned = null;
    private SkinnedMeshRenderer m_secondaryWeaponSkinned = null;

    [Header("Assigned Manoeuvre GameObjects")]
    [Header("Ground")]
    public GameObject m_groundLightPrefab = null;
    public GameObject m_groundHeavyPrefab = null;

    [Header("In Air")]
    public GameObject m_inAirLightPrefab = null;
    public GameObject m_inAirHeavyPrefab = null;

    [Header("Sprinting")]
    public GameObject m_sprintingLightPrefab = null;
    public GameObject m_sprintingHeavyPrefab = null;

    //Stored scripts
    private Manoeuvre m_groundLight = null;
    private Manoeuvre m_groundHeavy = null;

    private Manoeuvre m_inAirLight = null;
    private Manoeuvre m_inAirHeavy = null;

    private Manoeuvre m_sprintingLight = null;
    private Manoeuvre m_sprintingHeavy = null;

    //Stored varibles
    [HideInInspector]
    public Manoeuvre m_currentManoeuvre = null;

    private Character m_character = null;
    private CustomAnimation m_customAnimation = null;

    //Manoeuvre Varibles
    private Manoeuvre.MANOEUVRE_STANCE m_nextManoeuvreStance = Manoeuvre.MANOEUVRE_STANCE.LIGHT;

    //Sequence Attack Variables
    private SEQUENCE_STATE m_currentSequenceState = SEQUENCE_STATE.INITIAL;
    private float m_attackTimer = 0.0f;

    /// <summary>
    /// Init manager
    /// </summary>
    /// <param name="p_character">Character that uses this manager</param>
    public virtual void Init(Character p_character, CustomAnimation p_customAnimation)
    {
        m_character = p_character;
        m_customAnimation = p_customAnimation;

        m_groundLight = BuildHitbox(m_groundLightPrefab);
        m_groundHeavy = BuildHitbox(m_groundHeavyPrefab);
        m_inAirLight = BuildHitbox(m_inAirLightPrefab);
        m_inAirHeavy = BuildHitbox(m_inAirHeavyPrefab);
        m_sprintingLight = BuildHitbox(m_sprintingLightPrefab);
        m_sprintingHeavy = BuildHitbox(m_sprintingHeavyPrefab);

        //Setup weapons
        if (m_primaryWeaponObject != null)
        {
            m_primaryWeaponObject.transform.SetParent(p_character.m_primaryAnchor.transform, false);
            m_primaryWeaponSkinned = m_primaryWeaponObject.GetComponentInChildren<SkinnedMeshRenderer>();
        }
        if (m_secondaryWeaponObject != null)
        {
            m_secondaryWeaponObject.transform.SetParent(p_character.m_secondaryAnchor.transform, false);
            m_secondaryWeaponSkinned = m_secondaryWeaponObject.GetComponentInChildren<SkinnedMeshRenderer>();
        }
    }

    /// <summary>
    /// Recursive instantiation of prefab hitbox colliders
    /// </summary>
    /// <param name="p_hitboxPrefab"></param>
    /// <returns></returns>
    private Manoeuvre BuildHitbox(GameObject p_hitboxPrefab)
    {
        if(p_hitboxPrefab != null)
        {
            GameObject newHitbox = Instantiate(p_hitboxPrefab, m_character.transform, false);

            Manoeuvre manoeuvre = newHitbox.GetComponent<Manoeuvre>();

            if(manoeuvre == null)
            {
                Destroy(newHitbox);
                return null;
            }

            manoeuvre.InitController(m_character);
            
            manoeuvre.m_lightBranch = BuildHitbox(manoeuvre.m_lightBranchPrefab);
            manoeuvre.m_heavyBranch = BuildHitbox(manoeuvre.m_heavyBranchPrefab);

            return manoeuvre;
        }

        return null;
    }

    /// <summary>
    /// Start of a single tree attack AKA Attack Manoeuvre
    /// </summary>
    public void StartManoeuvre(Manoeuvre p_manoeuvre)
    {
        if(m_character.m_splinePhysics.m_splineLocalVelocity.x < -0.05f)
        {
            m_character.SwapFacingDirection();
        }

        m_currentManoeuvre = p_manoeuvre;

        //Reset variables
        m_nextManoeuvreStance = Manoeuvre.MANOEUVRE_STANCE.NONE;
        m_currentSequenceState = SEQUENCE_STATE.INITIAL;

        m_attackTimer = 0.0f;

        if(!m_currentManoeuvre.m_sequenceAttack)
        {
            m_currentManoeuvre.EnableHitboxes();
        }
        
        SetBlendShapes(0.0f, 0.0f, 0.0f);

        if (m_currentManoeuvre.m_usingYVelocity)
            m_character.m_splinePhysics.m_gravity = false;

        m_customAnimation.PlayAnimation(m_currentManoeuvre.m_animationString, CustomAnimation.LAYER.ATTACK);
    }

    /// <summary>
    /// Updates the manoeuvre
    /// Should toggle colliders, update weapons as needed
    /// </summary>
    /// <returns>True when a manoeuvre is completed</returns>
    public void UpdateManoeuvre()
    {
        m_attackTimer += Time.deltaTime;

        float currentPercent = Mathf.Clamp(m_customAnimation.GetAnimationPercent(CustomAnimation.LAYER.ATTACK), 0.0f, 1.0f);
        float sequencePercent = 0.0f;

        //Running through initial sequence, that is the start of an attack, wait till completed
        if (m_currentManoeuvre.m_sequenceAttack)
        {
            //Given its a 3 manouevre sequence, velocity and blendshapes should be divided up into 3, that is, first sequence isnt using percent 0.0f->1.0f, rather 0.0f->0.33f, etc
            sequencePercent = currentPercent / 3.0f;
            sequencePercent += m_currentSequenceState == SEQUENCE_STATE.INITIAL ? 0.0f : (m_currentSequenceState == SEQUENCE_STATE.ATTACK ? 0.33f : 0.66f); //Add on a third for each section, attack then has range 0.33f -> 0.66f, etc

            //Blendshapes
            SetBlendShapes(m_currentManoeuvre.m_blendshapeCurve0.Evaluate(sequencePercent), m_currentManoeuvre.m_blendshapeCurve1.Evaluate(sequencePercent), m_currentManoeuvre.m_blendshapeCurve2.Evaluate(sequencePercent));

            switch (m_currentSequenceState)
            {
                case SEQUENCE_STATE.INITIAL:
                    if (m_customAnimation.IsAnimationDone(CustomAnimation.LAYER.ATTACK))
                    {
                        m_currentSequenceState = SEQUENCE_STATE.ATTACK;
                        m_customAnimation.PlayAnimation(m_currentManoeuvre.m_animationString + CustomAnimation.SECTION01_STRING, CustomAnimation.LAYER.ATTACK);

                        m_currentManoeuvre.EnableHitboxes();
                    }
                    break;
                case SEQUENCE_STATE.ATTACK:
                    if (CompletedManoeuvreSequence())
                    {
                        m_currentSequenceState = SEQUENCE_STATE.END;
                        m_customAnimation.PlayAnimation(m_currentManoeuvre.m_animationString + CustomAnimation.SECTION02_STRING, CustomAnimation.LAYER.ATTACK);

                        m_currentManoeuvre.DisableHitboxes();
                    }
                    else if(currentPercent >= 0.99f) //End of animation in sequence attack, loop it manually 
                    {
                        m_customAnimation.PlayAnimation(m_currentManoeuvre.m_animationString + CustomAnimation.SECTION01_STRING, CustomAnimation.LAYER.ATTACK, CustomAnimation.BLEND_TIME.INSTANT);
                    }
                    break;
                case SEQUENCE_STATE.END:
                    break;
                default:
                    break;
            }
        }
        else //normal attack
        {
            sequencePercent = currentPercent;

            //Blendshapes
            SetBlendShapes(m_currentManoeuvre.m_blendshapeCurve0.Evaluate(currentPercent), m_currentManoeuvre.m_blendshapeCurve1.Evaluate(currentPercent), m_currentManoeuvre.m_blendshapeCurve2.Evaluate(currentPercent));
        }

        //Apply velocity
        Vector2 desiredVelocity = m_character.GetDesiredVelocity();
        if (m_currentManoeuvre.m_usingXVelocity)
        {
            desiredVelocity.x = m_currentManoeuvre.m_velocityXCurve.Evaluate(sequencePercent);
        }
        if (m_currentManoeuvre.m_usingYVelocity)
        {
            desiredVelocity.y = m_currentManoeuvre.m_velocityYCurve.Evaluate(sequencePercent);
        }
        m_character.SetDesiredVelocity(desiredVelocity);

        //Getting next manouvre
        if (m_nextManoeuvreStance == Manoeuvre.MANOEUVRE_STANCE.NONE && !m_customAnimation.IsAnimatorBlending() && currentPercent > 0.3f) //Get next input
        {
            Character.ATTACK_INPUT_STANCE nextAttackStance = m_character.DetermineAttackStance();

            if (m_currentManoeuvre.m_lightBranch != null && nextAttackStance == Character.ATTACK_INPUT_STANCE.LIGHT)
                m_nextManoeuvreStance = Manoeuvre.MANOEUVRE_STANCE.LIGHT;
            else if (m_currentManoeuvre.m_heavyBranch != null && nextAttackStance == Character.ATTACK_INPUT_STANCE.HEAVY)
                m_nextManoeuvreStance = Manoeuvre.MANOEUVRE_STANCE.HEAVY;
        }
    }

    /// <summary>
    /// Attack has ended ensure all colldiers are disabled etc
    /// </summary>
    public void EndManoeuvre()
    {
        if (m_currentManoeuvre!=null)
        {
            SetBlendShapes(0.0f, 0.0f, 0.0f);
            m_currentManoeuvre.DisableHitboxes();
        }

        m_character.m_splinePhysics.m_gravity = true;

    }

    /// <summary>
    /// Determine if the manoeuvre has completed
    /// </summary>
    /// <returns>true when animation has ended, or if sequence, its ended and has performed all sequences needed</returns>
    public bool HasManoeuvreCompleted()
    {
        if (m_currentManoeuvre.m_sequenceAttack && m_currentSequenceState != SEQUENCE_STATE.END)
        {
            return false;
        }
        return m_customAnimation.IsAnimationDone(CustomAnimation.LAYER.ATTACK);
    }

    /// <summary>
    /// Has the sequence manoeuvre completed?
    /// This can be from several causes, time, on ground, etc
    /// </summary>
    /// <returns>true based off the requirements</returns>
    private bool CompletedManoeuvreSequence()
    {
        //Grounded
        if (m_currentManoeuvre.m_groundedFlag && !m_character.m_splinePhysics.m_downCollision)
            return false;

        //Time
        if (m_currentManoeuvre.m_timeTraveledFlag && m_attackTimer < m_currentManoeuvre.m_requiredAttackTime)
            return false;

        return true;
    }

    /// <summary>
    /// Get the initial manoeuvre where possible
    /// </summary>
    /// <param name="p_stance">Stance character has taken</param>
    /// <param name="p_type">Type of attack character has taken</param>
    /// <returns>Manoeuvre where possible, otherwise null</returns>
    public Manoeuvre GetInitialManoeuvre(Manoeuvre.MANOEUVRE_STANCE p_stance, Manoeuvre.MANOEUVRE_TYPE p_type)
    {
        switch (p_type)
        {
            case Manoeuvre.MANOEUVRE_TYPE.GROUND:
                switch (p_stance)
                {
                    case Manoeuvre.MANOEUVRE_STANCE.LIGHT:
                        return m_groundLight;
                    case Manoeuvre.MANOEUVRE_STANCE.HEAVY:
                        return m_groundHeavy;
                }
                break;
            case Manoeuvre.MANOEUVRE_TYPE.INAIR:
                switch (p_stance)
                {
                    case Manoeuvre.MANOEUVRE_STANCE.LIGHT:
                        return m_inAirLight;
                    case Manoeuvre.MANOEUVRE_STANCE.HEAVY:
                        return m_inAirHeavy;
                }
                break;
            case Manoeuvre.MANOEUVRE_TYPE.SPRINT:
                switch (p_stance)
                {
                    case Manoeuvre.MANOEUVRE_STANCE.LIGHT:
                        return m_sprintingLight;
                    case Manoeuvre.MANOEUVRE_STANCE.HEAVY:
                        return m_sprintingHeavy;
                }
                break;
        }

        return null;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Manoeuvre where possible, otherwise null</returns>
    public Manoeuvre GetNextManoeuvre()
    {
        if (m_currentManoeuvre != null) //Just used a manoeuvre, check just for light/heavy
        {
            switch (m_nextManoeuvreStance)
            {
                case Manoeuvre.MANOEUVRE_STANCE.LIGHT:
                    return m_currentManoeuvre.m_lightBranch;
                case Manoeuvre.MANOEUVRE_STANCE.HEAVY:
                    return m_currentManoeuvre.m_heavyBranch;
            }
        }
        return null;
    }


    /// <summary>
    /// Set the blend shapes for each weapon
    /// There are a possibilty of 3 channels to modify
    /// For example, on katana, there are 3 types of cuts: CUT, SLICE, STAB
    /// In skinned mesh, the assumed order is same as above
    /// </summary>
    /// <param name="p_channel0">Value to give, range of 0.0f-1.0f</param>
    /// <param name="p_channel1">Value to give, range of 0.0f-1.0f</param>
    /// <param name="p_channel2">Value to give, range of 0.0f-1.0f</param>
    private void SetBlendShapes(float p_channel0, float p_channel1, float p_channel2)
    {
        p_channel0 *= 100.0f; //Blends shapes use values from 0-100
        p_channel1 *= 100.0f;
        p_channel2 *= 100.0f; 

        SetSkinnedBlendShape(m_primaryWeaponSkinned, p_channel0, p_channel1, p_channel2);
        SetSkinnedBlendShape(m_secondaryWeaponSkinned, p_channel0, p_channel1, p_channel2);
    }

    /// <summary>
    /// Setup SkinnedMesh blend shapes
    /// </summary>
    /// <param name="p_skinnedMesh">skinned mesh, can be null</param>
    /// <param name="p_channel0">First blend shape parameter</param>
    /// <param name="p_channel1">Second blend shape parameter</param>
    /// <param name="p_channel2">Third blend shape parameter</param>
    private void SetSkinnedBlendShape(SkinnedMeshRenderer p_skinnedMesh, float p_channel0, float p_channel1, float p_channel2)
    {
        if(p_skinnedMesh != null)
        {
            if (p_skinnedMesh.sharedMesh.blendShapeCount < 1)//Break out early is unable to get blend shape
                return;
            p_skinnedMesh.SetBlendShapeWeight(0, p_channel0);

            if (p_skinnedMesh.sharedMesh.blendShapeCount < 2)//Break out early is unable to get blend shape
                return;
            p_skinnedMesh.SetBlendShapeWeight(1, p_channel1);

            if (p_skinnedMesh.sharedMesh.blendShapeCount < 3)//Break out early is unable to get blend shape
                return;
            p_skinnedMesh.SetBlendShapeWeight(2, p_channel2);
        }
    }
}
