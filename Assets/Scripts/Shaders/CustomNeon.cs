using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomNeon : MonoBehaviour
{
    [System.Serializable]
    public struct NEON_FLASH
    {
        public Color m_color;
        public float m_duration;
    }

    public List<NEON_FLASH> m_neonSequence = new List<NEON_FLASH>();

    public GameObject m_objectWithRenderer = null;

    private MeshRenderer m_meshRenderer = null;
    private Material m_material = null;

    private void Start()
    {
        if(m_neonSequence.Count > 0)
        {
            //Get varibles
            if(m_objectWithRenderer == null)
                m_meshRenderer = GetComponent<MeshRenderer>();
            else
                m_meshRenderer = m_objectWithRenderer.GetComponent<MeshRenderer>();

            if (m_meshRenderer == null) //Try children
            {
#if UNITY_EDITOR
                Debug.LogWarning(name + ": Has no renderer on base object or assigned object, will check for any on its children");
#endif
                m_meshRenderer = GetComponentInChildren<MeshRenderer>();
            }

            if (m_meshRenderer == null) //Found nothing
            {
#if UNITY_EDITOR
                Debug.LogError(name + ": No mesh renderer is found on the neon object or its children");
#endif
                return;
            }

            m_material = m_meshRenderer.material;

            if(m_material == null)
            {
#if UNITY_EDITOR
                Debug.LogError(name + ": No material is found on the neon object");
#endif
                return;
            }

            //m_meshRenderer.material = m_material;

            StartCoroutine(ChangeColor(0));
        }
    }

    private IEnumerator ChangeColor(int p_currentIndex)
    {
        m_material.SetColor("_EmissionColor", m_neonSequence[p_currentIndex].m_color);
        m_material.EnableKeyword("_EMISSION");

        yield return new WaitForSeconds(m_neonSequence[p_currentIndex].m_duration);

        p_currentIndex += 1;
        if (p_currentIndex >= m_neonSequence.Count)
        {
            p_currentIndex = 0;
        }

        StartCoroutine(ChangeColor(p_currentIndex));
    }
}
