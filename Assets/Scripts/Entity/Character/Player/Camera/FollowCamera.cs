using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public GameObject m_followTarget = null;

    public Vector3 m_cameraOffset = Vector3.zero;
    public Vector3 m_lookAtOffset = Vector3.zero;

    [Tooltip("Linear Speed, how long itll take to move the requried distance in one second")]
    public float m_cameraLinerarSpeed = 1.0f;
    [Tooltip("Distance where camera will move the linear speed")]
    public float m_linearSpeedDistance = 1.0f;
    [Tooltip("How quickly when not near the one to one distance will it slowdown and speed up, smaller means harsher dropoff")]
    public float m_dropOffRate = 1.0f;

    public enum CAMERA_ORIENTATION { INITIAL, INVERTED }
    [HideInInspector]
    public CAMERA_ORIENTATION m_currentOrientation = CAMERA_ORIENTATION.INITIAL;

    private void Start()
    {
        if (m_followTarget == null)
        {
            m_followTarget = FindObjectOfType<Character_Player>().gameObject;

            if (m_followTarget == null)
            {
                enabled = false;
                return;
            }
        }

        ForceSnap();
    }

    /// <summary>
    /// Move the camera into place with no blending
    /// </summary>
    public void ForceSnap()
    {
        if (m_followTarget == null)//Early breakout
            return;

        Vector3 cameraDesiredPos = m_followTarget.transform.position + (m_followTarget.transform.forward * m_cameraOffset.z + m_followTarget.transform.right * m_cameraOffset.x + m_followTarget.transform.up * m_cameraOffset.y);
        transform.position = cameraDesiredPos;
    }

    /// <summary>
    /// Flips camera to other side
    /// </summary>
    public void FlipCamera()
    {
        m_currentOrientation = m_currentOrientation == CAMERA_ORIENTATION.INITIAL ? CAMERA_ORIENTATION.INVERTED : CAMERA_ORIENTATION.INITIAL;
    }

    /// <summary>
    /// Follow object at a given offset
    /// </summary>
    private void FixedUpdate ()
    {
        if (m_followTarget == null)//Early breakout
            return;

        //Get desired position
        Vector3 cameraOffset = Vector3.zero;

        if(m_currentOrientation == CAMERA_ORIENTATION.INITIAL)
        {
            cameraOffset = m_followTarget.transform.forward * m_cameraOffset.z + m_followTarget.transform.right * m_cameraOffset.x + m_followTarget.transform.up * m_cameraOffset.y;
        }  
        else
        {
            cameraOffset = -m_followTarget.transform.forward * m_cameraOffset.z - m_followTarget.transform.right * m_cameraOffset.x + m_followTarget.transform.up * m_cameraOffset.y;
        }

        Vector3 cameraDesiredPos = m_followTarget.transform.position + cameraOffset;

        //Based off desired postion move towards it. Based off distance move faster/slower
        Vector3 requiredMovement = cameraDesiredPos - transform.position;
        Vector3 smoothedMovement = requiredMovement.normalized * DetermineCameraSpeed(requiredMovement.magnitude) * Time.deltaTime;

        if (smoothedMovement.sqrMagnitude < requiredMovement.sqrMagnitude)
            transform.position += smoothedMovement;
        else
            transform.position = cameraDesiredPos;

        Quaternion desiredRotation = Quaternion.LookRotation(m_followTarget.transform.position + m_lookAtOffset - transform.position);

        transform.rotation = desiredRotation;
	}

    public float DetermineCameraSpeed(float p_distance)
    {
        //Static
        return float.PositiveInfinity;

        //Arctan
        //Equation of (2xSpeed/PI) * arctan(droppoff*(x - linearSpeedDistance)) + C
        //Where C = -y intercept

        //Calc C, set x or p_distance to 0 for y-intercept, then invert 
        //float C = -((m_cameraLinerarSpeed * 2 / Mathf.PI) * Mathf.Atan(m_dropOffRate * (-m_linearSpeedDistance)));

        //return (m_cameraLinerarSpeed * 2 / Mathf.PI) * Mathf.Atan(m_dropOffRate * (p_distance - m_linearSpeedDistance)) + C;

        //Linear
        //Equation of gradiant(x)
        //return m_cameraLinerarSpeed * p_distance;

        //Parabola
        //Equation of gradiant(x)
        //return m_cameraLinerarSpeed * p_distance*p_distance;
    }
}
