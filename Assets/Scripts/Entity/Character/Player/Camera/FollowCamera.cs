using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public GameObject m_followTarget = null;

    public Vector3 m_cameraOffset = Vector3.zero;
    public Vector3 m_lookAtOffset = Vector3.zero;

    public float m_cameraSpeed = 1.0f;

    private void Start()
    {
        ForceSnap();
    }

    /// <summary>
    /// Move the camera into place with no blending
    /// </summary>
    public void ForceSnap()
    {
        Vector3 cameraDesiredPos = m_followTarget.transform.position + (m_followTarget.transform.forward * m_cameraOffset.z + m_followTarget.transform.right * m_cameraOffset.x + m_followTarget.transform.up * m_cameraOffset.y);
        transform.position = cameraDesiredPos;
    }

    /// <summary>
    /// Follow object given offset
    /// </summary>
    private void Update ()
    {
        if (m_followTarget == null)//Early breakout
            return;

        //Get desired position
        Vector3 cameraOffset = m_followTarget.transform.forward * m_cameraOffset.z + m_followTarget.transform.right * m_cameraOffset.x + m_followTarget.transform.up * m_cameraOffset.y;

        Vector3 cameraDesiredPos = m_followTarget.transform.position + cameraOffset;

        //Based off desired postion move towards it. Based off distance magnitude move faster/slower

        Vector3 desiredVector = cameraDesiredPos - transform.position;
        if(desiredVector.magnitude > 0.0f)
            transform.position += desiredVector * ((desiredVector.magnitude * m_cameraSpeed) / desiredVector.magnitude) * Time.deltaTime;

        Quaternion desiredRotation = Quaternion.LookRotation(m_followTarget.transform.position + m_lookAtOffset - transform.position);

        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 0.5f);
	}
}
