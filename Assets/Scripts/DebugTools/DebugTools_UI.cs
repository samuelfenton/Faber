using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTools_UI : MonoBehaviour
{
    /// <summary>
    /// Add the ability to player
    /// </summary>
    /// <param name="p_abilityIndex">Index to add, range 0 -> (CharacterStatistics.ABILITY.ABILITY_COUNT - 1)</param>
    public void Btn_AddPlayerAbility(int p_abilityIndex)
    {
        if (p_abilityIndex < 0 || p_abilityIndex >= (int)CharacterStatistics.ABILITY.ABILITY_COUNT)
            return;

        Character_Player player = FindObjectOfType<Character_Player>();

        if(player!=null)
        {
            CharacterStatistics stats = player.GetComponent<CharacterStatistics>();

            if(stats != null)
            {
                stats.m_abilityFlags[p_abilityIndex] = true;
            }
        }
    }

}
