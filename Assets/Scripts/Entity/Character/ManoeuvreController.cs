using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManoeuvreController : MonoBehaviour
{
    public enum MANOEUVRE_STANCE { LIGHT, HEAVY };
    public enum MANOEUVRE_TYPE { GROUND, INAIR, SPRINT };

    [Header("Branches")]
    public GameObject m_lightBranchPrefab = null;
    public GameObject m_heavyBranchPrefab = null;

    [HideInInspector]
    public ManoeuvreController m_lightBranchController = null;
    [HideInInspector]
    public ManoeuvreController m_heavyBranchController = null;

    [Header("Animation String Details")]
    [Range(1, 100)]
    [SerializeField]
    private int m_manoeuvreIndex = 1;
    [SerializeField]
    private MANOEUVRE_TYPE m_manoeuvreType = MANOEUVRE_TYPE.GROUND;
    [SerializeField]
    private MANOEUVRE_STANCE m_manoeuvreStance = MANOEUVRE_STANCE.LIGHT;

    [HideInInspector]
    public string m_animationString = "";

    [Header("Manoeuvre Details")]
    [Tooltip("Will this attack be affected by gravity")]
    public bool m_useGravity = true;
    [Tooltip("Translation data from start of attack, positive will move character forwards")]
    public AnimationCurve m_translationXCurve = null;
    [Tooltip("Will this attack require a sheathing blend? Example most gorund attacks, whereas in air attacks never sheath")]
    public bool m_requiresSheathingBlend = false;
    [Tooltip("Manoeuvre Damage")]
    public float m_manoeuvreDamage = 0.0f;

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

    private const string ONE_SHOT_TRIGGER = "OneShot";

    private Animator[] m_animators = new Animator[0];
    private ManoeuvreHitbox[] m_hitboxes = new ManoeuvreHitbox[0];

    /// <summary>
    /// Initialise hitbox
    /// </summary>
    /// <param name="p_character">Character controlling hitbox</param>
    public void InitController(Character p_character)
    {
        string type = m_manoeuvreType == MANOEUVRE_TYPE.GROUND ? "Ground" : m_manoeuvreType == MANOEUVRE_TYPE.INAIR ? "InAir" : "Sprint";
        string stance = m_manoeuvreStance == MANOEUVRE_STANCE.LIGHT ? "Light" : "Heavy";

        m_animationString = type + "_" + stance + m_manoeuvreIndex;

        m_animators = GetComponentsInChildren<Animator>();
        m_hitboxes = GetComponentsInChildren<ManoeuvreHitbox>();

        for (int hitboxIndex = 0; hitboxIndex < m_hitboxes.Length; hitboxIndex++)
        {
            m_hitboxes[hitboxIndex].Init(p_character, m_manoeuvreDamage);
        }
    }

    private void OnEnable()
    {
        for (int animatorIndex = 0; animatorIndex < m_animators.Length; animatorIndex++)
        {
            m_animators[animatorIndex].SetTrigger(ONE_SHOT_TRIGGER);
        }

        for (int hitboxIndex = 0; hitboxIndex < m_hitboxes.Length; hitboxIndex++)
        {
            m_hitboxes[hitboxIndex].StartOfManouvre();
        }
    }

    private void OnDisable()
    {
        for (int hitboxIndex = 0; hitboxIndex < m_hitboxes.Length; hitboxIndex++)
        {
            m_hitboxes[hitboxIndex].EndOfManouvre();
        }
    }
}
