using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ManoeuvreLeaf", menuName = "ScriptableObject/ManoeuvreLeaf", order = 0)]
public class ManoeuvreLeaf : ScriptableObject
{
    public enum MANOEUVRE_STANCE {LIGHT, HEAVY};
    public enum MANOEUVRE_TYPE { GROUND, INAIR, SPRINT};

    [Header("Branches")]
    public ManoeuvreLeaf m_lightBranch = null;
    public ManoeuvreLeaf m_heavyBranch = null;

    [Header("Animation String Details")]
    [Range(1,100)]
    [SerializeField]
    private int m_manoeuvreIndex = 0;
    [SerializeField]
    private MANOEUVRE_TYPE m_manoeuvreType = MANOEUVRE_TYPE.GROUND;
    [SerializeField]
    private MANOEUVRE_STANCE m_manoeuvreStance = MANOEUVRE_STANCE.LIGHT;

    private string m_animationString = "";

    [Header("Manoeuvre Details")]
    [Tooltip("Will this attack be affected by gravity")]
    public bool m_useGravity = true;
    [Tooltip("Translation data from start of attack, positive will move character forwards")]
    public AnimationCurve m_translationXCurve = null;
    [Tooltip("Will this attack require a sheathing blend? Example most gorund attacks, whereas in air attacks never sheath")]
    public bool m_requiresSheathingBlend = false;

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

    [System.Serializable]
    public struct ManoeuvreAction
    {
        [Tooltip("Given a weapons damage how much its modified, example light attack first in combo, 0.8 = 80% base damage, then to 1.0 for heavy, next combo 1.2 etc")]
        public float m_damageModifier;
        [Tooltip("What percent through animation will the damage be enabled")]
        [Range(0.0f, 1.0f)]
        public float m_damageStart;
        [Tooltip("What percent through animation will the damage be disable")]
        [Range(0.0f, 1.0f)]
        public float m_damageEnd;
        [Tooltip("During this section of damage, will the primary weapons collider be enabled")]
        public bool m_primaryUsage;
        [Tooltip("During this section of damage, will the secondary weapons collider be enabled")]
        public bool m_secondaryUsage;
    };

    [Header("Manoeuvre Actions")]
    [SerializeField]
    public List<ManoeuvreAction> m_manoeuvreActions = new List<ManoeuvreAction>();

    /// <summary>
    /// Build string used in animator
    /// </summary>
    private void OnEnable()
    {
        string type = m_manoeuvreType == MANOEUVRE_TYPE.GROUND ? "Ground" : m_manoeuvreType == MANOEUVRE_TYPE.INAIR ? "InAir" : "Sprint"; 
        string stance = m_manoeuvreStance == MANOEUVRE_STANCE.LIGHT ? "Light" : "Heavy";

        m_animationString = type + "_" + stance +  m_manoeuvreIndex;
    }

    /// <summary>
    /// Get the animaiton string to call in animator
    /// </summary>
    /// <returns>Pre-generated string</returns>
    public string GetAnimationString()
    {
        return m_animationString;
    }
}
