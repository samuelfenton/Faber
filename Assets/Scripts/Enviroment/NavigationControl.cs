using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationControl : MonoBehaviour
{
    public List<Character> m_currentCharacters = new List<Character>();

    public virtual void AddCharacter(Character p_character)
    {
        if (!m_currentCharacters.Contains(p_character))
            m_currentCharacters.Add(p_character);
    }

    public virtual void RemoveCharacter(Character p_character, NavigationTrigger p_navigationTrigger)
    {
        m_currentCharacters.Remove(p_character);
    }
}
