using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public GameObject m_followTarget = null;

    public Vector3 m_cameraOffset = Vector3.zero;

    public float m_cameraSpeed = 1.0f;

    private void Update ()
    {
        if (m_followTarget == null)//Early breakout
            return;

        //Get desired position
        Vector3 cameraOffset = m_followTarget.transform.forward * m_cameraOffset.z + m_followTarget.transform.right * m_cameraOffset.x + m_followTarget.transform.up * m_cameraOffset.y;
        Vector3 cameraDesiredPos = m_followTarget.transform.position + cameraOffset;

        //Based off desired postion move towards it. Based off distance magnitude move faster/slower

        Vector3 desiredVelocity = cameraDesiredPos - transform.position;
        if(desiredVelocity.magnitude > 0.0f)
            transform.position += desiredVelocity * ((desiredVelocity.magnitude * m_cameraSpeed) / desiredVelocity.magnitude) * Time.deltaTime;

        Quaternion desiredRotation = Quaternion.LookRotation(m_followTarget.transform.position - transform.position);

        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 0.5f);
	}
}
