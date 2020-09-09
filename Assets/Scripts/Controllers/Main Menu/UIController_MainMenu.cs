using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController_MainMenu : UIController
{
    private SceneController_MainMenu m_mainMenuSceneController = null;

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
    }

    /// <summary>
    /// Button to view credits
    /// </summary>
    public void Btn_Credits()
    {
    }

    /// <summary>
    /// Button to quit to desktop
    /// </summary>
    public void Btn_Quit()
    {
        Application.Quit();
    }
}
