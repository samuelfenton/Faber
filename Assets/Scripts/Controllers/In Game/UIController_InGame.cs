using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController_InGame : UIController
{
    public GameObject m_inGameUI = null;
    public GameObject m_menuUI = null;

    private enum CURRENT_MENU_STATE {IN_GAME, PAUSE_MENU }
    private CURRENT_MENU_STATE m_currentMenuState = CURRENT_MENU_STATE.IN_GAME;

    private InputAction_Gameplay m_intput = null;

    /// <summary>
    /// Setup varibels to be used in UI
    /// </summary>
    /// <param name="p_masterController">Master controller</param>
    public override void Init(MasterController p_masterController)
    {
        base.Init(p_masterController);
        
        if (m_inGameUI == null || m_menuUI == null)
        {
#if UNITY_EDITOR
            Debug.Log(name + " does not have its assigned InGame and Menu UI objects");
#endif
            Destroy(gameObject);
            return;
        }

        m_intput = new InputAction_Gameplay();

        m_intput.UI.Menu.Enable();
        m_intput.UI.Menu.started += ctx => ToggleMenu();

        ShowInGame();
    }

    private void OnEnable()
    {
        if(m_intput!= null)
            m_intput.UI.Menu.Enable();
    }

    private void OnDisable()
    {
        if (m_intput != null)
            m_intput.UI.Menu.Disable();
    }

    private void ToggleMenu()
    {
        if (m_currentMenuState == CURRENT_MENU_STATE.IN_GAME)
        {
            m_currentMenuState = CURRENT_MENU_STATE.PAUSE_MENU;
            ShowPauseMenu();
        }
        else
        {
            m_currentMenuState = CURRENT_MENU_STATE.IN_GAME;
            ShowInGame();
        }
    }

    #region Menu Management
    private void ShowInGame()
    {
        m_inGameUI.SetActive(true);
        m_menuUI.SetActive(false);

        Time.timeScale = 1.0f;
    }

    private void ShowPauseMenu()
    {
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
