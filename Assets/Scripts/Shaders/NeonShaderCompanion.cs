using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeonShaderCompanion : MonoBehaviour
{
    public const int NEONSLOT_COUNT = 256;
    public const int NEONCOLOUR_COUNT = 32;
    [Range(0, 255)]
    public int m_neonSlot = 0;

    public float m_scrollSpeed = 1.0f;

    private void Start()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if(meshRenderer !=null)
        {
            float slotIncrementValue = 1.0f / NEONSLOT_COUNT;
            float colourIncrementValue = 1.0f / NEONCOLOUR_COUNT;

            //Setup values used
            meshRenderer.material.SetFloat("_XUV", slotIncrementValue * m_neonSlot + slotIncrementValue / 2.0f);
            meshRenderer.material.SetFloat("_Increment", colourIncrementValue);
            meshRenderer.material.SetFloat("_IncrementHalf", colourIncrementValue / 2.0f);
            meshRenderer.material.SetFloat("_UVScrollSpeed", m_scrollSpeed);

        }
    }
}
