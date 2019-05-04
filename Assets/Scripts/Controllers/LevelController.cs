using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKeyDown(KeyCode.L))
        {
            if (SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCount)
                SceneManager.LoadScene(0);
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
	}
}
