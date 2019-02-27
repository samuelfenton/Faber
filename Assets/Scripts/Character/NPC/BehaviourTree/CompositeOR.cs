using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeOR : Composite
{
    public override RESULT Execute(Character_NPC p_character)
    {
        if (m_pendingIndex != null) //Attempt to run pending behaviour
        {
            RESULT pendingResult = m_childBehaviours[(int)m_pendingIndex].Execute(p_character);

            if (pendingResult != RESULT.FAILED)//Behavior have concluded this frame
                return pendingResult;
        }
        else
        {
            m_pendingIndex = 0;//Default to 0 if no pending for rest of loop
        }

        for (; m_pendingIndex < m_childBehaviours.Count; m_pendingIndex++)
        {
            RESULT pendingResult = m_childBehaviours[(int)m_pendingIndex].Execute(p_character);
            if (pendingResult != RESULT.FAILED)//Behavior have concluded this frame
                return pendingResult;
        }

        m_pendingIndex = null; //Reset count
        return RESULT.FAILED;
    }
}
