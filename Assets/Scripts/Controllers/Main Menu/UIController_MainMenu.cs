using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController_MainMenu : UIController
{
    private SceneController_MainMenu m_mainMenuSceneController = null;

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    /// <param name="p_masterController">Master controller</param>
    public override void Init(MasterController p_masterController)
    {
        base.Init(p_masterController);

        m_mainMenuSceneController = (SceneController_MainMenu)p_masterController.m_currentSceneController;
        
        if (m_mainMenuSceneController == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " is unable to find a SceneController_MainMenu script, maybe its missing?");
#endif
            Destroy(gameObject);
            return;
        }

    }

    public void Btn_NewGame()
    {
        m_mainMenuSceneController.LoadFirstLevel();
    }

}
