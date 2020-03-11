using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [Tooltip("If none assigned assumed it is its own object")]
    public GameObject m_objectToRot = null;
    public Vector3 m_rotateAxis = Vector3.up;
    [Tooltip("In degrees per second")]
    public float m_rotateSpeed = 30.0f;

    private void Start()
    {
        if (m_objectToRot == null)
            m_objectToRot = gameObject;
    }

    private void Update()
    {
        m_objectToRot.transform.Rotate(m_rotateAxis, m_rotateSpeed * Time.deltaTime, Space.Self);
    }
}
