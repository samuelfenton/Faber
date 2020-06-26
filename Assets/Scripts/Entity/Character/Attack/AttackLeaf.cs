using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackLeaf", menuName = "ScriptableObject/AttackLeaf", order = 0)]
public class AttackLeaf : ScriptableObject
{
    public enum ATTACK_TYPE {LIGHT, HEAVY};

    [Header("Branches")]
    public AttackLeaf m_lightBranch = null;
    public AttackLeaf m_heavyBranch = null;

    [Header("Animation String Details")]
    [Range(0,100)]
    [SerializeField]
    private int m_attackIndex = 0;
    [SerializeField]
    private ATTACK_TYPE m_attackType = ATTACK_TYPE.LIGHT;

    private string m_animationString = "";

    [Header("Attack Details")]
    public AnimationCurve m_translationXCurve;
    public AnimationCurve m_translationYCurve;

    /// <summary>
    /// Build string used in animator
    /// </summary>
    private AttackLeaf()
    {
        m_animationString = (m_attackType == ATTACK_TYPE.LIGHT ? "Light" : "Heavy") + "_" + m_attackIndex;
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
