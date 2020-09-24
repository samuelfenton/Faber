﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController_MainMenu : SceneController
{
    public MasterController.SCENE m_firstScene = MasterController.SCENE.LEVEL_PROTOTYPE_TUTORIAL;

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    public override void Init()
    {
        base.Init();
    }

    /// <summary>
    /// Should be called from UI button
    /// Button should only be clickable after checking that load game is valid
    /// Will attempt to load the game
    /// </summary>
    public void LoadGame()
    {
        MasterController.Instance.LoadScene((MasterController.SCENE)MasterController.Instance.m_inGameSaveData.m_saveSceneIndex, true);
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