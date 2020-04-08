using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ManoeuvreData", menuName = "ScriptableObject/ManoeuvreData", order = 0)]
public class ManoeuvreData : ScriptableObject
{
    [System.Serializable]
    public struct AttackDetails
    {
        [Range(0.0f,1.0f)]
        public float m_damageStart;
        [Range(0.0f, 1.0f)]
        public float m_damageEnd;
        [Range(0.0f, 1.0f)]
        public float m_comboStart;
        [Range(0.0f, 1.0f)]
        public float m_comboEnd;
        public AnimationCurve m_translationCurve;
    }

    public enum WEAPON_TYPE {UNARMED = 0, ONE_HANDED_SWORD, TWO_HANDED_SWORD, SWORD_AND_SHIELD, POLEARM, POLEARM_AND_SHIELD } //used for determining animations
    public WEAPON_TYPE m_currentWeapon = WEAPON_TYPE.UNARMED;

    [Header("Manouevre Details")]
    [SerializeField]
    public AttackDetails[] m_groundLight = new AttackDetails[1];
    [SerializeField]
    public AttackDetails[] m_groundHeavy = new AttackDetails[1];
    [SerializeField]
    public AttackDetails[] m_inAirLight = new AttackDetails[1];
    [SerializeField]
    public AttackDetails[] m_inAirHeavy = new AttackDetails[1];
    [SerializeField]
    public AttackDetails[] m_sprintLight = new AttackDetails[1];
    [SerializeField]
    public AttackDetails[] m_sprintHeavy = new AttackDetails[1];
}
