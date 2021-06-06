using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manoeuvre : MonoBehaviour
{
    public enum MANOEUVRE_STANCE {NONE, LIGHT, HEAVY };
    public enum MANOEUVRE_TYPE { GROUND, INAIR, SPRINT };

    public enum DAMAGE_DIRECTION {NONE, FORWARDS, HORIZONTAL_RIGHT, HORIZONTAL_LEFT, VERTICAL_UPWARDS, VERTICAL_DOWNWARDS};
    public enum DAMAGE_IMPACT {LOW, MEDIUM, HIGH};

    [Header("Branches")]
    public GameObject m_lightBranchPrefab = null;
    public GameObject m_heavyBranchPrefab = null;

    [Header("Animation String Details")]
    [Range(1, 100)]
    [SerializeField]
    private int m_manoeuvreIndex = 1;
    [SerializeField]
    private MANOEUVRE_TYPE m_manoeuvreType = MANOEUVRE_TYPE.GROUND;
    [SerializeField]
    private MANOEUVRE_STANCE m_manoeuvreStance = MANOEUVRE_STANCE.LIGHT;

    [Header("Manoeuvre Details")]
    [Header("Desired Velocity Curves")]
    public bool m_usingXVelocity = false;
    [Tooltip("Desired velocity curve for horizontal")]
    public AnimationCurve m_velocityXCurve = null;
    public bool m_usingYVelocity = false;
    [Tooltip("Desired velocity curve for vertical")]
    public AnimationCurve m_velocityYCurve = null;
    [Tooltip("Will this manoeuvre require a sheathing blend? Example most gorund attacks, whereas in air attacks never sheath")]
    public bool m_requiresSheathingBlend = false;
    [Tooltip("Manoeuvre Damage")]
    public float m_manoeuvreDamage = 0.0f;
    [SerializeField]
    [Tooltip("Direction of the manoeuvre, for determineing what effect to use")]
    public DAMAGE_DIRECTION m_damageDirection = DAMAGE_DIRECTION.FORWARDS;
    [SerializeField]
    [Tooltip("Force this manoeuvre will do, should modify the knockback effect, as well as stamina usage")]
    public DAMAGE_IMPACT m_damageImpact = DAMAGE_IMPACT.MEDIUM;

    [Header("Blendshapes")]
    [SerializeField]
    [Tooltip("Values for the given blend shape, channel 0")]
    public AnimationCurve m_blendshapeCurve0 = null;
    [SerializeField]
    [Tooltip("Values for the given blend shape, channel 1")]
    public AnimationCurve m_blendshapeCurve1 = null;
    [SerializeField]
    [Tooltip("Values for the given blend shape, channel 2")]
    public AnimationCurve m_blendshapeCurve2 = null;

    [Header("Sequence Data")]
    [Tooltip("Does this attack come in three parts? Start, Loop and End")]
    public bool m_sequenceAttack = false;
    [Header("Sequence completion flags")]
    [Tooltip("Completed once hitting the ground")]
    public bool m_groundedFlag = false;
    [Tooltip("Completed once a peridor of time has passed")]
    public bool m_timeTraveledFlag = false;
    [Tooltip("Only used when previous is checked, how long till it ends")]
    public float m_requiredAttackTime = 0.0f;

    [HideInInspector]
    public string m_animationString = "";
    private ManoeuvreHitbox[] m_hitboxes = new ManoeuvreHitbox[0];

    [HideInInspector]
    public Manoeuvre m_lightBranch = null;
    [HideInInspector]
    public Manoeuvre m_heavyBranch = null;

    /// <summary>
    /// Initialise hitbox
    /// </summary>
    /// <param name="p_character">Character controlling hitbox</param>
    public void InitController(Character p_character)
    {
        m_animationString = CustomAnimation.BuildManoeuvreString(m_manoeuvreType, m_manoeuvreStance, m_manoeuvreIndex);

        m_hitboxes = GetComponentsInChildren<ManoeuvreHitbox>();

        for (int hitboxIndex = 0; hitboxIndex < m_hitboxes.Length; hitboxIndex++)
        {
            m_hitboxes[hitboxIndex].Init(p_character, this);
        }
    }

    /// <summary>
    /// Enable the colliders for the given manoeuvre
    /// </summary>
    public void EnableHitboxes()
    {
        for (int hitboxIndex = 0; hitboxIndex < m_hitboxes.Length; hitboxIndex++)
        {
            m_hitboxes[hitboxIndex].EnableHitbox();
        }
    }

    /// <summary>
    /// Disable the colliders for the given manoeuvre
    /// </summary>
    public void DisableHitboxes()
    {
        for (int hitboxIndex = 0; hitboxIndex < m_hitboxes.Length; hitboxIndex++)
        {
            m_hitboxes[hitboxIndex].DisableHitbox();
        }
    }
}
