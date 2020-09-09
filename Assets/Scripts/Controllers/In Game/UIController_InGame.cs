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

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    /// <param name="p_masterController">Master controller</param>
    public override void Init(MasterController p_masterController)
    {
        base.Init(p_masterController);
        
        if (m_inGameUI == null || m_menuUI == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have its assigned InGame and Menu UI objects");
#endif
            Destroy(gameObject);
            return;
        }

        m_customInput = ((SceneController_InGame)MasterController.Instance.m_currentSceneController).m_customInput;

        ShowInGame();
    }

    private void Update()
    {
        if(m_customInput.GetKey(CustomInput.INPUT_KEY.MENU) == CustomInput.INPUT_STATE.DOWNED)
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
    private void ShowInGame()
    {
        m_currentMenuState = CURRENT_MENU_STATE.IN_GAME;

        m_inGameUI.SetActive(true);
        m_menuUI.SetActive(false);

        Time.timeScale = 1.0f;
    }

    private void ShowPauseMenu()
    {
        m_currentMenuState = CURRENT_MENU_STATE.PAUSE_MENU;

        m_inGameUI.SetActive(false);
        m_menuUI.SetActive(true);

        Time.timeScale = 0.0f;
    }
    #endregion

    #region In Game UI

    #endregion

    #region Pause Menu Management
    public void BtnReturnToGame()
    {
        ShowInGame();
    }

    public void BtnQuit()
    {
        Application.Quit();
    }
    #endregion
}
