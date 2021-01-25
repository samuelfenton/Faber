using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTools_HitboxSync : MonoBehaviour
{
    private enum SEQUENCE_STATE { INITIAL, ATTACK, END }
    private const string SECTION01_STRING = "_Section1";
    private const string SECTION02_STRING = "_Section2";

    public GameObject m_characterObj = null;
    public GameObject m_hitboxObj = null;

    private Character m_character = null;
    private Animator m_characterAnimator = null;
    private Manoeuvre m_hitboxManoeuvre = null;

    [Tooltip("Add weapon object")]
    public GameObject m_primaryWeaponObject = null;
    [Tooltip("Add weapon object")]
    public GameObject m_secondaryWeaponObject = null;

    private SkinnedMeshRenderer m_primaryWeaponSkinned = null;
    private SkinnedMeshRenderer m_secondaryWeaponSkinned = null;

    private SEQUENCE_STATE m_currentSequenceState = SEQUENCE_STATE.INITIAL;

    private void Start()
    {
        if(m_characterObj == null || m_hitboxObj == null)
        {
            Destroy(this);
            return;
        }

        m_character = m_characterObj.GetComponent<Character>();
        m_characterAnimator = m_characterObj.GetComponentInChildren<Animator>();
        m_hitboxManoeuvre = m_hitboxObj.GetComponentInChildren<Manoeuvre>();

        if (m_character == null || m_characterAnimator == null || m_hitboxManoeuvre == null)
        {
            Destroy(this);
            return;
        }

        if (m_primaryWeaponObject != null)
        {
            m_primaryWeaponSkinned = m_primaryWeaponObject.GetComponentInChildren<SkinnedMeshRenderer>();
        }
        if (m_secondaryWeaponObject != null)
        {
            m_secondaryWeaponSkinned = m_secondaryWeaponObject.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        m_hitboxManoeuvre.InitController(m_character);

        //First play

        if (m_hitboxManoeuvre.m_sequenceAttack)//Sequence does not start with hit box enabled
        {
            m_hitboxManoeuvre.DisableHitboxes();
        }
        else //Normal starts with hitboxes enabled
        {
            m_hitboxManoeuvre.EnableHitboxes();
        }

        m_characterAnimator.Play(m_hitboxManoeuvre.m_animationString);

    }

    private void Update()
    {
        float currentPercent = Mathf.Clamp(m_characterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime, 0.0f, 1.0f);

        bool endOfAnim = m_characterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f;

        //Running through initial sequence, that is the start of an attack, wait till completed
        if (m_hitboxManoeuvre.m_sequenceAttack)
        {
            //Given its a 3 manouevre sequence, velocity and blendshapes should be divided up into 3, that is, first sequence isnt using percent 0.0f->1.0f, rather 0.0f->0.33f, etc
            float sequencePercent = currentPercent / 3.0f;
            sequencePercent += m_currentSequenceState == SEQUENCE_STATE.INITIAL ? 0.0f : (m_currentSequenceState == SEQUENCE_STATE.ATTACK ? 0.33f : 0.66f); //Add on a third for each section, attack then has range 0.33f -> 0.66f, etc

            //Blendshapes
            SetBlendShapes(m_hitboxManoeuvre.m_blendshapeCurve0.Evaluate(sequencePercent), m_hitboxManoeuvre.m_blendshapeCurve1.Evaluate(sequencePercent), m_hitboxManoeuvre.m_blendshapeCurve2.Evaluate(sequencePercent));

            switch (m_currentSequenceState)
            {
                case SEQUENCE_STATE.INITIAL:
                    if (endOfAnim)
                    {
                        m_currentSequenceState = SEQUENCE_STATE.ATTACK;
                        m_characterAnimator.Play(m_hitboxManoeuvre.m_animationString + SECTION01_STRING);

                        m_hitboxManoeuvre.EnableHitboxes();
                    }
                    break;
                case SEQUENCE_STATE.ATTACK:
                    if (endOfAnim)
                    {
                        m_currentSequenceState = SEQUENCE_STATE.END;
                        m_characterAnimator.Play(m_hitboxManoeuvre.m_animationString + SECTION02_STRING);

                        m_hitboxManoeuvre.DisableHitboxes();
                    }
                    break;
                case SEQUENCE_STATE.END:
                    if (endOfAnim)
                    {
                        m_currentSequenceState = SEQUENCE_STATE.INITIAL;

                        m_characterAnimator.Play(m_hitboxManoeuvre.m_animationString, 0, 0.0f);
                    }
                    break;
                default:
                    break;
            }
        }
        else //Normal attack
        {
            SetBlendShapes(m_hitboxManoeuvre.m_blendshapeCurve0.Evaluate(currentPercent), m_hitboxManoeuvre.m_blendshapeCurve1.Evaluate(currentPercent), m_hitboxManoeuvre.m_blendshapeCurve2.Evaluate(currentPercent));

            if (endOfAnim)// just wait for attack to end
            {
                m_hitboxManoeuvre.DisableHitboxes();
                m_hitboxManoeuvre.EnableHitboxes();
                m_characterAnimator.Play(m_hitboxManoeuvre.m_animationString, 0, 0.0f);
            }
        }
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
        if (p_skinnedMesh != null)
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
