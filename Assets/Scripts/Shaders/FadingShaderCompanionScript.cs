﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadingShaderCompanionScript : MonoBehaviour
{
    public Texture m_modelTexture = null;
    private Transform m_playerTransform = null;
    private Material m_companionMaterial = null;

    private void Start()
    {
        m_playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        m_companionMaterial = GetComponent<Renderer>().materials[0];

        m_companionMaterial.SetTexture("_MainTex", m_modelTexture);
    }

    private void Update ()
    {
        m_companionMaterial.SetVector("_PlayerWorldPostion", m_playerTransform.position);

    }
}
