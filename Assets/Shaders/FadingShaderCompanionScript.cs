using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadingShaderCompanionScript : MonoBehaviour
{
    private GameObject m_playerTransform = null;
    private Material m_companionMaterial = null;


    [ExecuteInEditMode]
    private void Start()
    {

        m_playerTransform = GameObject.FindGameObjectWithTag("Player");
        m_companionMaterial = GetComponent<Renderer>().materials[0];
    }

    private void Update ()
    {
        if(m_playerTransform != null)
            m_companionMaterial.SetVector("_PlayerWorldPostion", m_playerTransform.transform.position);
    }
}
