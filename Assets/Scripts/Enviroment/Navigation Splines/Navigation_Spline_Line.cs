using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation_Spline_Line : Navigation_Spline
{
    private Vector3 m_lineEquation = Vector3.zero;
    private Vector3 m_forwardsDir = Vector3.zero;

    public override void Start()
    {
        base.Start();

        m_lineEquation = m_splineEnd.transform.position - m_splineStart.transform.position;
        m_splineLength = m_lineEquation.magnitude;

        //Setup forwards direction
        m_forwardsDir = m_lineEquation;
        m_forwardsDir.y = 0;
        m_forwardsDir = m_forwardsDir.normalized;
    }

    public override Vector3 GetSplinePosition(float p_splinePercent)
    {
        return m_splineStart.transform.position + p_splinePercent * m_lineEquation;
    }

    public override Vector3 GetForwardsDir(Vector3 p_splinePosition)
    {
        return m_forwardsDir;
    }

    private void OnDrawGizmos()
    {
        //Draw line
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(m_splineStart.transform.position, m_splineEnd.transform.position);

        //Draw start side
        Vector3 startDisplayPos = m_splineStart.transform.position + (m_splineEnd.transform.position - m_splineStart.transform.position) * 0.1f;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(startDisplayPos, 0.2f);
    }

}
