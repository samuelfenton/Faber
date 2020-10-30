using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomNeon : MonoBehaviour
{
    private const string BASE_COLOR = "_BaseColor";
    private const string EMISSION_ENABLE = "_EMISSION";
    private const string EMISSION_COLOR = "_EmissionColor";

    [System.Serializable]
    public struct NEON_SECTION
    {
        public float m_timeCode;
        public Color m_neonColor;
        public bool m_emissionInUse;

        public NEON_SECTION(float p_timeCode = 0.0f)
        {
            m_timeCode = p_timeCode;
            m_neonColor = Color.white;
            m_emissionInUse = false;
        }
    }

    [System.Serializable]
    public struct NEON_SEQUENCE_DATA
    {
        public GameObject m_neonObject;
        public List<NEON_SECTION> m_neonSequence;

        /// <summary>
        /// Check if struct is full of valid data
        /// </summary>
        /// <returns>true when valid and usable</returns>
        public bool IsValidData()
        {
            if (m_neonObject == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("One or more of the sections has no designated neon gameobjects");
#endif
                return false;
            }

            if (m_neonSequence.Count == 0)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning(m_neonObject.name + " has no assigned sequence, and will be ignored");
#endif
                return false;
            }

            return true;
        }
    }

    public class NeonSequence : MonoBehaviour
    {
        private GameObject m_neonObject = null;
        private List<NEON_SECTION> m_neonSequence;

        private int m_currentIndex = 0;
        private Material[] m_materials = new Material[0];

        private Coroutine m_loopCoroutine = null;

        private NEON_TYPE m_neonType;
        private float m_emissionIntensity = 1.0f;

        /// <summary>
        /// Initialise the seqeance
        /// </summary>
        /// <param name="p_validData">Data to copy over from</param>
        /// <param name="p_neonType">Type of intended neon</param>
        /// <param name="p_emissionIntensity">In the case of emission, what intensity is it?</param>
        /// <returns>True when inialisation has completed</returns>
        public bool InitSequence(NEON_SEQUENCE_DATA p_validData, NEON_TYPE p_neonType, float p_emissionIntensity = 1.0f)
        {
            //Copy data
            m_neonObject = p_validData.m_neonObject;
            m_neonSequence = p_validData.m_neonSequence;
            m_neonType = p_neonType;
            m_emissionIntensity = p_emissionIntensity;

            //Valid setup
            MeshRenderer[] sectionRenderers = m_neonObject.GetComponentsInChildren<MeshRenderer>();

            if (sectionRenderers.Length == 0)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning(m_neonObject.name + " has no attached mesh renderers");
#endif
                return false;
            }

            //Setup material
            m_materials = new Material[sectionRenderers.Length];

            //Apply to all renderers
            for (int rendererIndex = 0; rendererIndex < sectionRenderers.Length; rendererIndex++)
            {
                m_materials[rendererIndex] = Instantiate(sectionRenderers[rendererIndex].material);

                //Setup keywords
                switch (m_neonType)
                {
                    case NEON_TYPE.COLOR_ONLY:                        
                        m_materials[rendererIndex].EnableKeyword(BASE_COLOR);
                        break;
                    case NEON_TYPE.EMISSION_ONLY:

                        break;
                    case NEON_TYPE.COLOR_AND_EMISSION:                        
                        m_materials[rendererIndex].EnableKeyword(BASE_COLOR);
                        break;
                    default:
                        break;
                }

                sectionRenderers[rendererIndex].sharedMaterial = m_materials[rendererIndex];
            }

            //Only one change needed so set now and forget
            if(m_neonSequence.Count == 1)
            {
                SetColour(m_neonSequence[0].m_neonColor, m_neonSequence[0].m_emissionInUse);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Reset neon sequence to first index
        /// </summary>
        public void ResetSequence()
        {
            m_currentIndex = 0;

            if (m_loopCoroutine != null)
            {
                StopCoroutine(m_loopCoroutine);
                m_loopCoroutine = null;
            }

            m_loopCoroutine = StartCoroutine(NextSection());
        }

        /// <summary>
        /// Delay till next neon change
        /// </summary>
        private IEnumerator NextSection()
        {
            SetColour(m_neonSequence[m_currentIndex].m_neonColor, m_neonSequence[m_currentIndex].m_emissionInUse);

            if (m_currentIndex == m_neonSequence.Count - 1)
            {
                yield break;
            }

            float nextDelay = m_neonSequence[m_currentIndex + 1].m_timeCode - m_neonSequence[m_currentIndex].m_timeCode;

            yield return new WaitForSeconds(Mathf.Max(0.0f, nextDelay));

            m_currentIndex++;

            m_loopCoroutine = StartCoroutine(NextSection());
        }


        /// <summary>
        /// Set color for all materials being used
        /// </summary>
        /// <param name="p_enabled">Is the emission set for this</param>
        /// <param name="p_emisisonInUse">Is this color in use? In the case of emission, set emission to 0</param>
        private void SetColour(Color p_color, bool p_emisisonInUse)
        {
            for (int materialIndex = 0; materialIndex < m_materials.Length; materialIndex++)
            {
                switch (m_neonType)
                {
                    case NEON_TYPE.COLOR_ONLY:
                        //Apply values
                        m_materials[materialIndex].SetColor(BASE_COLOR, p_color);

                        break;
                    case NEON_TYPE.EMISSION_ONLY:
                        //Apply values
                        if (p_emisisonInUse)
                            ToggleEmission(m_materials[materialIndex], true, GetColourIntensity(p_color));
                        else
                            ToggleEmission(m_materials[materialIndex], false, Color.black);

                        break;
                    case NEON_TYPE.COLOR_AND_EMISSION:
                        //Apply values
                        m_materials[materialIndex].SetColor(BASE_COLOR, p_color);

                        if (p_emisisonInUse)
                            ToggleEmission(m_materials[materialIndex], true, GetColourIntensity(p_color));
                        else
                            ToggleEmission(m_materials[materialIndex], false, Color.black);

                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Determine a colour based off its intensity
        /// uses m_emissionIntensity for intensity 
        /// Math found here https://forum.unity.com/threads/how-to-change-hdr-colors-intensity-via-shader.531861/
        /// </summary>
        /// <param name="p_baseColour">Base colour to modify</param>
        /// <returns>Color modified by intensity</returns>
        private Color GetColourIntensity(Color p_baseColour)
        {
            float intensityMul = Mathf.Pow(2.0f, m_emissionIntensity);
            intensityMul = Mathf.Pow(intensityMul, 2.2f); //Get gamma value

            //Aplpy modifier
            p_baseColour.r *= intensityMul;
            p_baseColour.g *= intensityMul;
            p_baseColour.b *= intensityMul;

            return p_baseColour;
        }

        /// <summary>
        /// Toggle a materials emissivness
        /// </summary>
        /// <param name="p_material">Material to modify</param>
        /// <param name="p_val">Emissiveness value, true for in use</param>
        /// <param name="p_color">Desired emission color, use black for false</param>
        private void ToggleEmission(Material p_material, bool p_val, Color p_color)
        {
            if(p_val)
            {
                p_material.EnableKeyword(EMISSION_ENABLE);
                p_material.EnableKeyword(EMISSION_COLOR);

                p_material.SetColor(EMISSION_COLOR, p_color);

                p_material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                
                DynamicGI.UpdateEnvironment();
            }
            else
            {
                p_material.DisableKeyword(EMISSION_ENABLE);
                p_material.DisableKeyword(EMISSION_COLOR);

                p_material.SetColor(EMISSION_COLOR, Color.black);

                p_material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

                DynamicGI.UpdateEnvironment();
            }
        }
    }


    [Header("Settings")]
    public float m_totalTime = 0.0f;
    public enum NEON_TYPE {COLOR_ONLY, EMISSION_ONLY, COLOR_AND_EMISSION}
    [Tooltip("COLOR_ONLY: only changes color, COLOR_AND_EMISSION: Add to both colour and emission, used more for LCD screens")]
    public NEON_TYPE m_colourType = NEON_TYPE.COLOR_ONLY;

    [Header("Advanced")]
    [Tooltip("Neon intensity when using emission")]
    public float m_emissionIntensity = 1.0f;
    [Tooltip("Only change if youre not using the default varible names in the shader, otherwise leave empty")]
    public string m_materialVarible = "";

    [Header("Sequence Data")]
    public List<NEON_SEQUENCE_DATA> m_neonSequenceData = new List<NEON_SEQUENCE_DATA>();
    private List<NeonSequence> m_neonSequence = new List<NeonSequence>();

    /// <summary>
    /// Setup all sequencing based off data given
    /// </summary>
    private void Start()
    {            
        for (int sequenceIndex = 0; sequenceIndex < m_neonSequenceData.Count; sequenceIndex++)
        {
            if(m_neonSequenceData[sequenceIndex].IsValidData())
            {
                NeonSequence nextSequence = gameObject.AddComponent<NeonSequence>();

                if (nextSequence.InitSequence(m_neonSequenceData[sequenceIndex], m_colourType, m_emissionIntensity))
                    m_neonSequence.Add(nextSequence);
                else
                    Destroy(nextSequence);
            }
        }

        //Check if theres no sequence left
        if(m_neonSequence.Count == 0)
        {
            return;
        }


        StartCoroutine(TotalSequence());
    }

    /// <summary>
    /// Run first neon sequence for each, then delay till loop ends
    /// </summary>
    /// <returns></returns>
    private IEnumerator TotalSequence()
    {
        for (int sequenceIndex = 0; sequenceIndex < m_neonSequence.Count; sequenceIndex++)
        {
            m_neonSequence[sequenceIndex].ResetSequence();
        }

        yield return new WaitForSeconds(m_totalTime);

        StartCoroutine(TotalSequence());
    }
}
