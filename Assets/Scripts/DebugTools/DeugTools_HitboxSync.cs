using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeugTools_HitboxSync : MonoBehaviour
{
    public GameObject m_characterObj = null;
    public GameObject m_hitboxObj = null;

    private Character m_character = null;
    private Animator m_characterAnimator = null;
    private ManoeuvreController m_hitboxManoeuvreController = null;

    private void Start()
    {
        if(m_characterObj == null || m_hitboxObj == null)
        {
            Destroy(this);
            return;
        }

        m_character = m_characterObj.GetComponent<Character>();
        m_characterAnimator = m_characterObj.GetComponentInChildren<Animator>();
        m_hitboxManoeuvreController = m_hitboxObj.GetComponentInChildren<ManoeuvreController>();

        if (m_character == null || m_characterAnimator == null || m_hitboxManoeuvreController == null)
        {
            Destroy(this);
            return;
        }

        m_hitboxManoeuvreController.InitController(m_character);

        m_hitboxManoeuvreController.gameObject.SetActive(false);

        //First play
        m_hitboxManoeuvreController.gameObject.SetActive(true);
        m_characterAnimator.Play(m_hitboxManoeuvreController.m_animationString);
    }

    private void Update()
    {
        if(IsAnimationDone())
        {
            m_hitboxManoeuvreController.gameObject.SetActive(false);

            m_hitboxManoeuvreController.gameObject.SetActive(true);
            m_characterAnimator.Play(m_hitboxManoeuvreController.m_animationString, 0, 0.0f);
        }
    }

    /// <summary>
    /// Determine if animation is done
    /// </summary>
    /// <returns>True when normalized time is great than END_ANIMATION_TIME, defaults to false</returns>
    public bool IsAnimationDone()
    {
        if (m_characterAnimator == null)
            return false;

        return m_characterAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f;
    }
}
