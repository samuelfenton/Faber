using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierNOT : Modifier
{
    public override RESULT Execute(Character_NPC p_character)
    {
        RESULT result = m_childBehaviour.Execute(p_character);

        return result == RESULT.PENDING ? RESULT.PENDING : result == RESULT.FAILED ? RESULT.SUCCESS : RESULT.FAILED; // Success = Failed, Failed = Success, Pending = Pending
    }
}
