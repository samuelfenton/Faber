using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    //Going to just leave this here for future use
    
    //-------------------
    //Function Definition
    //
    //Param float:
    //      int:
    //
    //Return bool:
    //-------------------


    public LevelController m_levelController = null;
    public InputController m_inputController = null;

    private void Update()
    {
        if (m_inputController.GetKeyInput(InputController.INPUT_KEY.CANCEL))
            Application.Quit();
    }
}
