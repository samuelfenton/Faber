using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomNeon : MonoBehaviour
{
    private const string COLOR_STRING = "_Color";
    private const string EMISSION_STRING = "_EmissionColor";

    [System.Serializable]
    public struct NEON_SECTION
    {
        public Color m_neonColor;
        public float m_timeCode;
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
#if UNITY_EDITOR
                Debug.LogWarning("One or more of the sections has no designated neon gameobjects");
#endif
                return false;
            }

            if (m_neonSequence.Count == 0)
            {
#if UNITY_EDITOR
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

        private string[] m_targetVariables;

        /// <summary>
        /// Initialise the seqeance
        /// </summary>
        /// <param name="p_validData">Data to copy over from</param>
        /// <param name="p_targetVariables">String to use when modifying color</param>
        /// <returns>True when inialisation has completed</returns>
        public bool InitSequence(NEON_SEQUENCE_DATA p_validData, string[] p_targetVariables)
        {
            //Copy data
            m_neonObject = p_validData.m_neonObject;
            m_neonSequence = p_validData.m_neonSequence;
            m_targetVariables = p_targetVariables;

            //Valid setup
            MeshRenderer[] sectionRenderers = m_neonObject.GetComponentsInChildren<MeshRenderer>();

            if (sectionRenderers.Length == 0)
            {
#if UNITY_EDITOR
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
                m_materials[rendererIndex].EnableKeyword("_EMISSION");
                sectionRenderers[rendererIndex].sharedMaterial = m_materials[rendererIndex];
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
            SetColour(m_neonSequence[m_currentIndex].m_neonColor);

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
        /// <param name="p_color">Color to set to</param>
        private void SetColour(Color p_color)
        {
            for (int materialIndex = 0; materialIndex < m_materials.Length; materialIndex++)
            {
                //Assign to all varibles
                for (int varibleIndex = 0; varibleIndex < m_targetVariables.Length; varibleIndex++)
                {
                    m_materials[materialIndex].SetColor(m_targetVariables[varibleIndex], p_color);
                }
            }
        }
    }

    [Header("Settings")]
    public float m_totalTime = 0.0f;
    public enum NEON_TYPE {COLOR, EMISSION, BOTH}
    [Tooltip("COLOR: Changes base color, used more for posters/prelit objects, EMISSION: Adds emission to object, used more on neon tube signs, BOTH: Add to both colour and emission, used more for LCD screens")]
    public NEON_TYPE m_colourType = NEON_TYPE.COLOR;

    [Header("Advanced")]
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
        string[] variableStrings = new string[0];

        if(m_materialVarible != "") //Preassigned varible
        {
            variableStrings = new string[1];
            variableStrings[0] = m_materialVarible;
        }
        else
        {
            switch (m_colourType)
            {
                case NEON_TYPE.COLOR:
                case NEON_TYPE.EMISSION:
                    variableStrings = new string[1];
                    variableStrings[0] = m_colourType == NEON_TYPE.COLOR ? COLOR_STRING : EMISSION_STRING;
                    break;
                case NEON_TYPE.BOTH:
                    variableStrings = new string[2];
                    variableStrings[0] = COLOR_STRING;
                    variableStrings[1] = EMISSION_STRING;
                    break;
            }
        }
            
        for (int sequenceIndex = 0; sequenceIndex < m_neonSequenceData.Count; sequenceIndex++)
        {
            if(m_neonSequenceData[sequenceIndex].IsValidData())
            {
                NeonSequence nextSequence = gameObject.AddComponent<NeonSequence>();

                if (nextSequence.InitSequence(m_neonSequenceData[sequenceIndex], variableStrings))
                    m_neonSequence.Add(nextSequence);
                else
                    Destroy(nextSequence);
            }
        }

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
