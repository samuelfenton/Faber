using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerMaster : MonoBehaviour
{
    public static ControllerMaster Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitSceneControllers();
    }

    private void InitSceneControllers()
    {
        GameObject controllerObj = GameObject.FindGameObjectWithTag("GameController");
        SceneController sceneController = controllerObj.GetComponent<SceneController>();

        if (sceneController != null)
            sceneController.InitController();
    }

    public void LoadLevel()
    {
        InitSceneControllers();
    }
}
