using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Incline : MonoBehaviour
{
    public float m_heightExtent = 1.0f;

    private void Start()
    {
        m_heightExtent = GetComponent<BoxCollider>().size.y/2.0f;
    }
}
