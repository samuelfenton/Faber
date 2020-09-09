using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour
{
    private EventSystem m_eventSystem = null;

    private void Awake()
    {
        m_eventSystem = GetComponentInChildren<EventSystem>();

        ToggleEventSystem(false);
    }

    /// <summary>
    /// Setup varibels to be used in UI
    /// </summary>
    public virtual void Init()
    {
        if(m_eventSystem == null)
            m_eventSystem = GetComponentInChildren<EventSystem>();

        ToggleEventSystem(true);
    }

    /// <summary>
    /// Toggle if the event system is enabled or not
    /// </summary>
    /// <param name="p_val">Value to set to</param>
    public void ToggleEventSystem(bool p_val)
    {
        m_eventSystem.enabled = p_val;
    }
}
