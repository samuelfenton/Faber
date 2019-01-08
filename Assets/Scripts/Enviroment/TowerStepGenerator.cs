using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TowerStepGenerator : MonoBehaviour
{
    public GameObject m_stepPrefab = null;
    public GameObject m_stepParent = null;

    public int m_stepCount = 9;

    public float m_degrees = 90.0f;
    
    [Tooltip("Starts at forward")]
    public float m_degreesStartOffset = 0.0f;

    public float m_stepHeight = 1.0f;

    public float m_distanceFromCenter = 1.0f;

    public enum ROT_DIRECTION { CLOCKWISE = 1, ANTI_CLOCKWISE = -1 }
    public ROT_DIRECTION m_rotationDirection = ROT_DIRECTION.CLOCKWISE;

    public bool m_setupStairs = false;

#if UNITY_EDITOR
    private void Update()
    {
        if (m_setupStairs)
        {
            if (m_stepPrefab != null && m_stepParent != null)
            {
                //Remove all current steps
                while(m_stepParent.transform.childCount > 0)
                {
                    DestroyImmediate(m_stepParent.transform.GetChild(0).gameObject);
                }

                Vector3 stepPos = transform.forward * m_distanceFromCenter; //Starting forward
                stepPos = Quaternion.AngleAxis(m_degreesStartOffset * (int)m_rotationDirection, Vector3.up) * stepPos; //Offset

                //Each step offset
                Quaternion rotAngle = Quaternion.AngleAxis(m_degrees / m_stepCount * (int)m_rotationDirection, Vector3.up);
                float heightStep = m_stepHeight / m_stepCount;

                for (int i = 0; i < m_stepCount; i++)
                {
                    GameObject newStep = Instantiate(m_stepPrefab, m_stepParent.transform);

                    //Pos
                    Vector3 newStepPos = stepPos;
                    newStepPos.y = heightStep * i;
                    newStep.transform.position = m_stepParent.transform.position + newStepPos;

                    //Rotation
                    //Look at perpendicular angle
                    Vector3 perp = new Vector3(stepPos.z, 0.0f, -stepPos.x);
                    newStep.transform.LookAt(newStep.transform.position + (int)m_rotationDirection * perp); // rotation dir changes the facing of steps, looking away from starting pos
                    //Update nextPostion
                    stepPos = rotAngle * stepPos;
                }
            }
            m_setupStairs = false;
        }
    }
#endif
}
