using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation_Trigger_One2One : Navigation_Trigger
{
    public Navigation_Spline m_forwardSpline = null;
    public Navigation_Spline m_backSpline = null;

    protected override void Start()
    {
#if UNITY_EDITOR
        if(m_forwardSpline == null || m_backSpline == null)
        {
            Debug.Log("One to One spline trigger has no attached splines");

        }
#endif
        base.Start();

    }

    protected override void HandleTrigger(Character p_character, TRIGGER_DIRECTION p_direction)
    {
        if (p_direction == TRIGGER_DIRECTION.ENTERING)
        {
            if(!m_forwardSpline.m_activeCharacters.Contains(p_character))
            {
                m_backSpline.RemoveCharacter(p_character);
                m_forwardSpline.AddCharacter(p_character);

                p_character.m_characterCustomPhysics.m_currentSpline = m_forwardSpline;
                p_character.m_characterCustomPhysics.m_currentSplinePercent = 0;
            }
        }
        else
        {
            if (!m_backSpline.m_activeCharacters.Contains(p_character))
            {
                m_forwardSpline.RemoveCharacter(p_character);
                m_backSpline.AddCharacter(p_character);

                p_character.m_characterCustomPhysics.m_currentSpline = m_backSpline;
                p_character.m_characterCustomPhysics.m_currentSplinePercent = 1;
            }
        }
    }
}
