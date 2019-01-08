using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation_Spline : MonoBehaviour
{
    public Transform m_splineStart = null;
    public Transform m_splineEnd = null;

    [HideInInspector]
    public float m_splineLength = 1.0f;

    [HideInInspector]
    public List<Character> m_activeCharacters = new List<Character>();

    public virtual void Start()
    {

    }

    public virtual Vector3 GetSplinePosition(float p_splinePercent)
    {
        return Vector3.zero;
    }

    public virtual Vector3 GetForwardsDir(Vector3 p_splinePosition)
    {
        return Vector3.zero;
    }

    public float GetSplinePercent(float p_movement)
    {
        return p_movement / m_splineLength;
    }

    public void AddCharacter(Character p_character)
    {
        if (!m_activeCharacters.Contains(p_character))
            m_activeCharacters.Add(p_character);
    }

    public void RemoveCharacter(Character p_character)
    {
        if (m_activeCharacters.Contains(p_character))
            m_activeCharacters.Remove(p_character);
    }

    public float GetPositionOfSplineTransform(Transform p_splineTransform)
    {
        if (p_splineTransform == m_splineStart)
            return 0;
        return 1;
    }
}
