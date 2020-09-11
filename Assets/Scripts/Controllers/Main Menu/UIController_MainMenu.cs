using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController_MainMenu : UIController
{
    [Header("Assigned Variables")]
    public GameObject m_UIObjectMainMenu = null;
    public GameObject m_UIObjectOptions = null;
    public GameObject m_UIObjectCredits = null;

    private SceneController_MainMenu m_mainMenuSceneController = null;
    private enum MAINMENU_STATE {MAINMENU, OPTIONS, CREDITS}
    private MAINMENU_STATE m_currentState = MAINMENU_STATE.MAINMENU;
    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    public override void Init()
    {
        base.Init();

        m_mainMenuSceneController = (SceneController_MainMenu)(MasterController.Instance.m_currentSceneController);
        
        if (m_mainMenuSceneController == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " is unable to find a SceneController_MainMenu script, maybe its missing?");
#endif
            Destroy(gameObject);
            return;
        }

        if (m_UIObjectMainMenu == null || m_UIObjectOptions == null || m_UIObjectCredits == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have all its required variables assigned");
#endif
            Destroy(gameObject);
            return;
        }

        m_currentState = MAINMENU_STATE.MAINMENU;

        m_UIObjectMainMenu.SetActive(true);
        m_UIObjectOptions.SetActive(false);
        m_UIObjectCredits.SetActive(false);
    }

    /// <summary>
    /// Button to load game
    /// </summary>
    public void Btn_LoadGame()
    {
        //TODO, actually load game
        m_mainMenuSceneController.LoadFirstLevel();
    }

    /// <summary>
    /// Button to start new game
    /// </summary>
    public void Btn_NewGame()
    {
        m_mainMenuSceneController.LoadFirstLevel();
    }

    /// <summary>
    /// Button to change settings
    /// </summary>
    public void Btn_Options()
    {
        m_UIObjectMainMenu.SetActive(false);
        m_UIObjectOptions.SetActive(true);
    }

    /// <summary>
    /// Button to view credits
    /// </summary>
    public void Btn_Credits()
    {
        m_UIObjectMainMenu.SetActive(false);
        m_UIObjectCredits.SetActive(true);
    }

    /// <summary>
    /// Button to quit to desktop
    /// </summary>
    public void Btn_Quit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Swap canvas to show main menu
    /// TODO, add in animaiton effect, fade in/slide in etc
    /// </summary>
    public void Btn_ReturnToMainMenu()
    {
        m_UIObjectMainMenu.SetActive(true);
        m_UIObjectOptions.SetActive(false);
        m_UIObjectCredits.SetActive(false);
    }
}
