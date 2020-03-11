using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController_Level_Master : UIController
{
    public UIController_Level_InGame m_inGame = null;
    public UIController_Level_Menu m_menu = null;

    private void Start()
    {
        if(m_inGame == null || m_menu == null)
        {
            Destroy(gameObject);

#if UNITY_EDITOR
            Debug.Log(name + " does not have its assigned in game and meny UI controllers");
#endif
            return;
        }

        m_inGame.InitController(this);
        m_menu.InitController(this);

        ShowInGame();
    }

    public void ShowInGame()
    {
        m_inGame.gameObject.SetActive(true);
        m_menu.gameObject.SetActive(false);

        Time.timeScale = 1.0f; 
    }

    public void ShowMenu()
    {
        m_inGame.gameObject.SetActive(false);
        m_menu.gameObject.SetActive(true);

        Time.timeScale = 0.0f;
    }
}
