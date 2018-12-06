using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationLogic : MonoBehaviour
{
    public List<Character> m_currentCharacters = new List<Character>();

    public virtual void AddCharacter(Character p_character)
    {
        if (!m_currentCharacters.Contains(p_character))
            m_currentCharacters.Add(p_character);
    }

    public virtual void RemoveCharacter(Character p_character, NavigationTrigger p_navigationTrigger)
    {
    }

    protected void ClampRotationOnExit(Character p_character)
    {
        Vector3 rotation = p_character.transform.rotation.eulerAngles; //Between 0 - 360

        if (rotation.y < 45 || rotation.y > 315)
            rotation.y = 0;
        else if (rotation.y > 45 && rotation.y < 135)
            rotation.y = 90;
        else if (rotation.y > 135 && rotation.y < 225)
            rotation.y = 180;
        else
            rotation.y = 270;

        p_character.transform.rotation = Quaternion.Euler(rotation);
    }
}
