using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public GameObject m_followTarget = null;

    public Vector3 m_cameraOffset = Vector3.zero;

    public float m_cameraSpeed = 1.0f;
    private Vector3 m_cameraDesiredPos = Vector3.zero;

    private void Update ()
    {
        //Get desired position
        Vector3 cameraOffset = m_followTarget.transform.forward * m_cameraOffset.z + m_followTarget.transform.right * m_cameraOffset.x + m_followTarget.transform.up * m_cameraOffset.y;
        m_cameraDesiredPos = m_followTarget.transform.position + cameraOffset;

        //Based off desired postion move towards it. Based off distance magnitude move faster/slower

        Vector3 desiredVelocity = m_cameraDesiredPos - transform.position;
        if(desiredVelocity.magnitude > 0.0f)
            transform.position += desiredVelocity * ((desiredVelocity.magnitude * m_cameraSpeed) / desiredVelocity.magnitude) * Time.deltaTime;

        transform.LookAt(m_followTarget.transform);
	}
}
