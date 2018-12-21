using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Navigation_Spline_Curve : Navigation_Spline
{
    private const float DEBUG_STEPS = 18;
    public float m_totalDegrees = 90;

    public enum ROT_DIRECTION {CLOCKWISE = 1, ANTI_CLOCKWISE = -1}
    public ROT_DIRECTION m_rotationDirection = ROT_DIRECTION.CLOCKWISE;

    private Vector3 m_startingOffset = Vector3.zero;

    public override void Start()
    {
        base.Start();

        float radius = (m_splineStart.position - transform.position).magnitude;

        m_splineLength = 2 * Mathf.PI * radius * (m_totalDegrees/360); //Total = PI*r    90 degrees = total/4
        m_startingOffset = m_splineStart.position - transform.position;
    }

    public override Vector3 GetSplinePosition(float p_splinePercent)
    {
        Quaternion rotAngle = Quaternion.AngleAxis(m_totalDegrees * p_splinePercent * (int)m_rotationDirection, Vector3.up);
        float heightStep = (m_splineEnd.position.y - m_splineStart.position.y) * p_splinePercent;

        Vector3 splinePosition = transform.position + rotAngle * m_startingOffset;
        splinePosition.y = transform.position.y + heightStep;
        return splinePosition;
    }

    public override Vector3 GetForwardsDir(Vector3 p_splinePosition)
    {
        Vector3 desiredDir = p_splinePosition - transform.position;
        desiredDir = new Vector3(desiredDir.z, 0.0f,- desiredDir.x);
        return desiredDir.normalized * (int)m_rotationDirection;
    }

    private void OnDrawGizmos()
    {
        if (m_splineStart == null || m_splineEnd == null)
            return;

        Gizmos.color = Color.blue;

        Vector3 startingVector = m_splineStart.position - transform.position;
        Vector3 nextVector = Vector3.zero;

        Quaternion rotAngle = Quaternion.AngleAxis(m_totalDegrees / DEBUG_STEPS * (int)m_rotationDirection, Vector3.up);

        float heightStep = (m_splineEnd.position.y - m_splineStart.position.y) / DEBUG_STEPS;
        //Loop through approximating circle, every (m_totalDegrees / DEBUG_STEPS) degrees
        for (int i = 0; i < DEBUG_STEPS; i++)
        {
            nextVector = rotAngle * startingVector;
            nextVector += new Vector3(0, heightStep, 0);
            Gizmos.DrawLine(transform.position + startingVector, transform.position + nextVector);
            startingVector = nextVector;
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        Vector3 startPosition = m_splineStart.transform.position - transform.position;
        Vector3 endPosition = m_splineEnd.transform.position;

        Vector3 endOffset = Quaternion.AngleAxis(m_totalDegrees * (int)m_rotationDirection, Vector3.up) * startPosition;

        endPosition.x = transform.position.x + endOffset.x;
        endPosition.z = transform.position.z + endOffset.z;
        m_splineEnd.transform.position = endPosition;
    }
#endif
}
