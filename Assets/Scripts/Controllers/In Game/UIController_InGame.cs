using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController_InGame : UIController
{
    public GameObject m_inGameUI = null;
    public GameObject m_menuUI = null;

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
        
        if (m_inGameUI == null || m_menuUI == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have its assigned InGame and Menu UI objects");
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

        m_inGameUI.SetActive(true);
        m_menuUI.SetActive(false);

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

        m_inGameUI.SetActive(false);
        m_menuUI.SetActive(true);

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
    /// Button to change basic options
    /// </summary>
    public void Btn_Options()
    { 
    
    }

    /// <summary>
    /// Button to return to main menu
    /// </summary>
    public void Btn_Quit()
    {
        MasterController.Instance.LoadScene(MasterController.SCENE.MAIN_MENU, false);
    }
    #endregion
}
