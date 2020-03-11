using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController_Level_Menu : UIController
{
    private UIController_Level_Master m_masterController = null;

    /// <summary>
    /// Setup varibels to be used in UI
    /// </summary>
    /// <param name="p_masterController">Master controller for this UI</param>
    public void InitController(UIController_Level_Master p_masterController)
    {
        m_masterController = p_masterController;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            m_masterController.ShowInGame();
        }
    }

    public void BtnReturnToGame()
    {
        m_masterController.ShowInGame();
    }

    public void BtnQuit()
    {
        Application.Quit();
    }
}
