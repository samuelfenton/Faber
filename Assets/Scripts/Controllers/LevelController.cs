using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour 
{
    //-------------------
    //Check for player input, changing to next level
    //-------------------
    void Update ()
    {
		if(UnityEngine.Input.GetKeyDown(KeyCode.L))
        {
            if (SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCount)
                SceneManager.LoadScene(0);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
	}
}
