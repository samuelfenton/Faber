using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCube_PivotBob : MonoBehaviour
{
    [Header("Bobbing Variables")]
    [Tooltip("How far to bob form original postion")]
    public float m_bobDistance = 1.0f;
    [Tooltip("Period of each bob, that is time for a single up and down bob")]
    public float m_bobRate = 1.0f;
    [Tooltip("Axis to bob along")]
    public Vector3 m_bobVector = Vector3.up;

    private float m_frequency = 0.1f;
    private Vector3 m_intialPosition = Vector3.zero;

    private void Start()
    {
        m_intialPosition = transform.position;
        m_frequency = 1 / m_bobRate;
    }

    private void Update()
    {
        transform.position = m_intialPosition + m_bobVector * m_bobDistance * Mathf.Sin(m_frequency * Time.time);
    }
}
