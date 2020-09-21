using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController_InGame : UIController
{
    public const string RETURN_TO_SHIRINE_PROMPT = "Are you sure you want to return to your last shrine?";

    [Header("Canvas Variables")]
    public GameObject m_UIObjectInGame = null;
    public GameObject m_UIObjectPause = null;
    public GameObject m_UIObjectPrompt = null;

    [Header("Pause Menu Variables")]
    public Button m_returnToShrineButton = null;


    private enum CURRENT_MENU_STATE {IN_GAME, PAUSE_MENU }
    private CURRENT_MENU_STATE m_currentMenuState = CURRENT_MENU_STATE.IN_GAME;

    private CustomInput m_customInput = null;
    private SceneController_InGame m_sceneController = null;

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    public override void Init()
    {
        base.Init();
        
        if (m_UIObjectInGame == null || m_UIObjectPause == null || m_UIObjectPrompt == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have its assigned objects");
#endif
            Destroy(gameObject);
            return;
        }

        if (m_promptText == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have all its required prompt variables assigned");
#endif
            Destroy(gameObject);
            return;
        }

        if (m_returnToShrineButton == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have all its required pause menu variables assigned");
#endif
            Destroy(gameObject);
            return;
        }

        m_sceneController = (SceneController_InGame)MasterController.Instance.m_currentSceneController;
        m_customInput = m_sceneController.m_customInput;

        ShowInGame();
    }

    /// <summary>
    /// Update the UI controller as needed
    /// should be called form master controller
    /// </summary>
    public override void UpdateUIController()
    {
        if(m_customInput!= null && m_customInput.GetKey(CustomInput.INPUT_KEY.MENU) == CustomInput.INPUT_STATE.DOWNED)
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        if (m_currentMenuState == CURRENT_MENU_STATE.IN_GAME)
        {
            ShowPauseMenu();
        }
        else
        {
            ShowInGame();
        }
    }

    #region Menu Management
    /// <summary>
    /// Show the in game UI
    /// Revert time scale to 1
    /// </summary>
    private void ShowInGame()
    {
        m_currentMenuState = CURRENT_MENU_STATE.IN_GAME;

        m_UIObjectInGame.SetActive(true);
        m_UIObjectPause.SetActive(false);
        m_UIObjectPrompt.SetActive(false);

        Time.timeScale = 1.0f;
        m_sceneController.InGameState = SceneController_InGame.INGAME_STATE.IN_GAME;
    }

    /// <summary>
    /// Show the pause menu
    /// reverts time scale to 0
    /// </summary>
    private void ShowPauseMenu()
    {
        m_currentMenuState = CURRENT_MENU_STATE.PAUSE_MENU;

        m_UIObjectInGame.SetActive(false);
        m_UIObjectPause.SetActive(true);

        m_returnToShrineButton.interactable = MasterController.Instance.m_inGameSaveData.IsValid();

        Time.timeScale = 0.0f;
        m_sceneController.InGameState = SceneController_InGame.INGAME_STATE.PAUSED;
    }
    #endregion

    #region In Game UI

    #endregion

    #region Pause Menu Management
    /// <summary>
    /// Button to return back to game
    /// </summary>
    public void Btn_ReturnToGame()
    {
        ShowInGame();
    }

    /// <summary>
    /// display the skill tree
    /// </summary>
    public void Btn_Skills()
    {

    }

    /// <summary>
    /// Respawn at last shrine
    /// </summary>
    public void Btn_ReturnToLastShrine()
    {
        StartCoroutine(ReturnToShrinePrompt());
    }

    /// <summary>
    /// Use prompt to ensure player wants to return to shrine
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReturnToShrinePrompt()
    {
        m_promptText.text = RETURN_TO_SHIRINE_PROMPT;

        m_UIObjectPrompt.SetActive(true);
        m_UIObjectPause.SetActive(false);

        m_currentPromptState = PROMPT_STATE.AWAITING_INPUT;

        while (m_currentPromptState == PROMPT_STATE.AWAITING_INPUT)
            yield return null;

        if (m_currentPromptState == PROMPT_STATE.PROMPT_ACCECPTED) //Accept prompt
        {
            ShowInGame();
            m_sceneController.RespawnPlayer(false);
        }
        else //Declined prompt
        {
            ShowInGame();
        }
    }
    /// Button to change basic options

    /// <summary>
    /// </summary>
    public void Btn_Options()
    { 
    
    }

    /// <summary>
    /// Button to return to main menu
    /// </summary>
    public void Btn_Quit()
    {
        Time.timeScale = 1.0f;
        MasterController.Instance.LoadScene(MasterController.SCENE.MAIN_MENU, false);
    }
    #endregion
}
