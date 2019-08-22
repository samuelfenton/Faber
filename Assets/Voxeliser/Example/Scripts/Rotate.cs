using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float m_rotateSpeed = 60.0f;

    /// <summary>
    /// Rotate object around y-axis
    /// </summary>
    private void Update()
    {
        transform.Rotate(Vector3.up, m_rotateSpeed * Time.deltaTime, Space.World);
    }
}
