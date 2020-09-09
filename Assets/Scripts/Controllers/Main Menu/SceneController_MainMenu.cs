using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController_MainMenu : SceneController
{
    public MasterController.SCENE m_firstScene = MasterController.SCENE.LEVEL_TUTORIAL;

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    public override void Init()
    {
        base.Init();
    }

    /// <summary>
    /// Update of main menu
    /// Camera should just slowly pan around
    /// </summary>
    private void Update()
    {

    }

    /// <summary>
    /// Should be called from UI button
    /// Will attempt to load hte first level
    /// </summary>
    public void LoadFirstLevel()
    {
        MasterController.Instance.LoadScene(m_firstScene, true);
    }
}
