using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIController : MonoBehaviour
{

    /// <summary>
    /// Setup varibels to be used in UI
    /// </summary>
    public virtual void Init()
    {

    }

    /// <summary>
    /// Update the UI controller as needed
    /// should be called form master controller
    /// </summary>
    public virtual void UpdateUIController()
    {

    }

    #region Prompt Functions
    //Example prompt setup
    //private IEnumerator NewGamePrompt()
    //{
    //    m_UIObjectPrompt.SetActive(true);
    //    m_UIObjectMainMenu.SetActive(false);

    //    m_currentPromptState = PROMPT_STATE.AWAITING_INPUT;

    //    while (m_currentPromptState == PROMPT_STATE.AWAITING_INPUT)
    //        yield return null;

    //    if (m_currentPromptState == PROMPT_STATE.PROMPT_ACCECPTED) //Accept prompt
    //    {
    //        DataController.RemoveSaveFiles();
    //        m_mainMenuSceneController.LoadFirstLevel();
    //    }
    //    else //Declined prompt
    //    {
    //        m_UIObjectPrompt.SetActive(false);
    //        m_UIObjectMainMenu.SetActive(true);
    //    }
    //}
    protected enum PROMPT_STATE { AWAITING_INPUT, PROMPT_ACCECPTED, PROMPT_DECLINED }
    protected PROMPT_STATE m_currentPromptState = PROMPT_STATE.AWAITING_INPUT;

    [Header("Prompt Variables")]
    public TextMeshProUGUI m_promptText = null;

    public void Btn_PromptAccept()
    {
        m_currentPromptState = PROMPT_STATE.PROMPT_ACCECPTED;
    }

    public void Btn_PromptDecline()
    {
        m_currentPromptState = PROMPT_STATE.PROMPT_DECLINED;
    }
    #endregion
}
