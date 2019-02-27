using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierDelay : Modifier
{
    public float m_delay = 1.0f;

    private bool m_continue = false;

    public override RESULT Execute(Character_NPC p_character)
    {
        if (!m_continue) //Return early while witing for timer
            return RESULT.PENDING;

        RESULT result = m_childBehaviour.Execute(p_character);

        if (result != RESULT.PENDING)//Finsihed, reset values
        {
            m_continue = false;
        }

        return result;
    }

    private IEnumerator Continue()
    {
        yield return new WaitForSeconds(m_delay);
        m_continue = true;
    }
}
