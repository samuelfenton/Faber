using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAbilities : MonoBehaviour
{
    public enum ABILITY {DOUBLE_JUMP = 0, IN_AIR_DASH, ABILITY_COUNT}

    [SerializeField]
    private bool[] m_abilityFlags = new bool[(int)ABILITY.ABILITY_COUNT];

    private void Awake()
    {
        m_abilityFlags = new bool[(int)ABILITY.ABILITY_COUNT];

        for (int abilityIndex = 0; abilityIndex < (int)ABILITY.ABILITY_COUNT; abilityIndex++)
        {
            m_abilityFlags[abilityIndex] = true;
        }
    }

    /// <summary>
    /// DHas this character had an ability flag
    /// </summary>
    /// <param name="p_ability">Ability to check</param>
    /// <returns>true when flag is set, false when invalid or not set</returns>
    public bool HasAbility(ABILITY p_ability)
    {
        if (p_ability == ABILITY.ABILITY_COUNT)
            return false;
        return m_abilityFlags[(int)p_ability];
    }
}
