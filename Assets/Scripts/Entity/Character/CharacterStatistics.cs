using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStatistics : MonoBehaviour
{
    public enum STATS{HEALTH = 0, STAMINA, STRENGTH, DEXTERITY, ENDURANCE, LUCK, STAT_COUNT}
    public enum ABILITY {DOUBLE_JUMP = 0, IN_AIR_DASH, RANGED_ABILITY, ABILITY_COUNT}

    public int m_currentExperience = 0;

    public int[] m_statLevel = new int[(int)STATS.STAT_COUNT];
    public bool[] m_abilityFlags = new bool[(int)ABILITY.ABILITY_COUNT];

    private void Awake()
    {
        DataController.LoadCharacterStats(this);
    }

    /// <summary>
    /// Get the players current level
    /// </summary>
    /// <returns>Sum of all increased stats</returns>
    public int GetLevel()
    {
        int level = 0;
        for (int statIndex = 0; statIndex < (int)STATS.STAT_COUNT; statIndex++)
        {
            level += m_statLevel[statIndex];
        }

        return level;
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
