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
    public bool m_useGravity = true;
    public AnimationCurve m_translationXCurve = null;
    public AnimationCurve m_translationYCurve = null;

    [SerializeField]
    public struct ManoeuvreAction
    {
        public float m_damage;
        [Range(0.0f, 1.0f)]
        public float m_damageStart;
        [Range(0.0f, 1.0f)]
        public float m_damageEnd;
    };

    [Header("Manoeuvre Actions")]
    [SerializeField]
    public List<ManoeuvreAction> m_manoeuvreActions = new List<ManoeuvreAction>();

    /// <summary>
    /// Build string used in animator
    /// </summary>
    private ManoeuvreLeaf()
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
