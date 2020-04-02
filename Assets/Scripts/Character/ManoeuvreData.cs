using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ManoeuvreData", menuName = "ScriptableObject/ManoeuvreData", order = 0)]
public class ManoeuvreData : ScriptableObject
{
    [System.Serializable]
    public struct AttackDetails
    {
        [Header("Light")]
        [Range(0.0f,1.0f)]
        public float m_lightDamageStart;
        [Range(0.0f, 1.0f)]
        public float m_lightDamageEnd;
        [Range(0.0f, 1.0f)]
        public float m_lightComboStart;
        [Range(0.0f, 1.0f)]
        public float m_lightComboEnd;
        [Header("Heavy")]
        [Range(0.0f, 1.0f)]
        public float m_heavyDamageStart;
        [Range(0.0f, 1.0f)]
        public float m_heavyDamageEnd;
        [Range(0.0f, 1.0f)]
        public float m_heavyComboStart;
        [Range(0.0f, 1.0f)]
        public float m_heavyComboEnd;
    }

    public enum WEAPON_TYPE {UNARMED = 0, ONE_HANDED_SWORD, TWO_HANDED_SWORD, SWORD_AND_SHIELD, POLEARM, POLEARM_AND_SHIELD } //used for determining animations
    public WEAPON_TYPE m_currentWeapon = WEAPON_TYPE.UNARMED;

    [Header("Manouevre Details")]
    [SerializeField]
    public AttackDetails[] m_groundAttacks = new AttackDetails[1];
    [SerializeField]
    public AttackDetails[] m_inAirAttacks = new AttackDetails[1];
    [SerializeField]
    public AttackDetails[] m_sprintAttacks = new AttackDetails[1];
}
