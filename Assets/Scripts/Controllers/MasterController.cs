using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MasterController : MonoBehaviour
{
    public static MasterController Instance { get; private set; }

    //In scene varibles
    [HideInInspector]
    public SceneController m_currentSceneController = null;
    [HideInInspector]
    public UIController m_currentUIController = null;
    [HideInInspector]
    public DataController m_dataController = null;

    /// <summary>
    /// Setup singleton functionality for the master controller
    /// </summary>
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

        m_dataController = new DataController();
        m_dataController.Init();

        DontDestroyOnLoad(topObject);

        //Setup loading on new scene
        SceneManager.sceneLoaded += SceneLoaded;

        InitSceneControllers();
    }

    /// <summary>
    /// Initialise all the current controllers within the scene
    /// Includes UI, Scene.
    /// </summary>
    private void InitSceneControllers()
    {
        m_currentSceneController = FindObjectOfType<SceneController>();
        m_currentUIController = FindObjectOfType<UIController>();

        if (m_currentSceneController != null)
            m_currentSceneController.Init(this);

        if (m_currentUIController != null)
            m_currentUIController.Init(this);
    }

    /// <summary>
    /// On a scene load, this is called. Setup all scene dependant varibles
    /// </summary>
    public void SceneLoaded(Scene aScene, LoadSceneMode aMode)
    {
        InitSceneControllers();
    }
}
