using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ManoeuvreData", menuName = "ScriptableObject/ManoeuvreData", order = 0)]
public class ManoeuvreData : ScriptableObject
{
    [System.Serializable]
    public struct ManoeuvreDetails
    {
        [Range(0.0f,1.0f)]
        public float m_primaryDamageStart;
        [Range(0.0f, 1.0f)]
        public float m_primaryDamageEnd;
        [Range(0.0f, 1.0f)]
        public float m_secondaryDamageStart;
        [Range(0.0f, 1.0f)]
        public float m_secondaryDamageEnd;

        public AnimationCurve m_translationXCurve; 
        public AnimationCurve m_translationYCurve;
    }

    public enum WEAPON_TYPE {UNARMED = 0, ONE_HANDED_SWORD, TWO_HANDED_SWORD, SWORD_AND_SHIELD, POLEARM, POLEARM_AND_SHIELD } //used for determining animations
    public WEAPON_TYPE m_currentWeapon = WEAPON_TYPE.UNARMED;

    [Header("Manouevre Details")]
    [SerializeField]
    public ManoeuvreDetails[] m_groundLight = new ManoeuvreDetails[1];
    [SerializeField]
    public ManoeuvreDetails[] m_groundHeavy = new ManoeuvreDetails[1];
    [SerializeField]
    public ManoeuvreDetails[] m_inAirLight = new ManoeuvreDetails[1];
    [SerializeField]
    public ManoeuvreDetails[] m_inAirHeavy = new ManoeuvreDetails[1];
    [SerializeField]
    public ManoeuvreDetails[] m_sprintLight = new ManoeuvreDetails[1];
    [SerializeField]
    public ManoeuvreDetails[] m_sprintHeavy = new ManoeuvreDetails[1];
}
