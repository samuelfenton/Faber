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


        GameObject topObject = gameObject;
        while (topObject.transform.parent != null)
            topObject = topObject.transform.parent.gameObject;

        DontDestroyOnLoad(topObject);

        InitSceneControllers();
    }

    private void InitSceneControllers()
    {
        GameObject controllerObj = GameObject.FindGameObjectWithTag(CustomTags.GAME_CONTROLLER);
        SceneController sceneController = controllerObj.GetComponent<SceneController>();

        if (sceneController != null)
            sceneController.InitController();
    }

    public void LoadLevel()
    {
        InitSceneControllers();
    }
}
