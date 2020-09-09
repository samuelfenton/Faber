using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MasterController : MonoBehaviour
{
    //Naming scheme for levels excluding main menu and loading should follow the following "LEVEL_THEME_AREA"
    //THEME - Where it is, cycber city, industrial, shogun, etc
    //AREA - Where within the theme is it? opening, waterfront etc
    public enum SCENE {MAIN_MENU, LOADING, LEVEL_TUTORIAL, LEVEL_CYBERCITY_PLAYERHUB, LEVEL_INDUSTRIAL_OPENING, SCENE_COUNT}
    private string[] m_sceneStrings = new string[(int)SCENE.SCENE_COUNT];
    public static MasterController Instance { get; private set; }

    //In scene varibles
    [HideInInspector]
    public SceneController m_currentSceneController = null;
    [HideInInspector]
    public UIController m_currentUIController = null;
    [HideInInspector]
    public DataController m_dataController = null;

    public AsyncOperation m_asyncSceneLoading = null;

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

        BuildSceneStrings();
        
        //Setup loading on new scene
        SceneManager.sceneLoaded += OnSceneLoaded;

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
    public void OnSceneLoaded(Scene aScene, LoadSceneMode aMode)
    {
        InitSceneControllers();
    }

    /// <summary>
    /// Assign the strings as needed to be used with enums
    /// </summary>
    public void BuildSceneStrings()
    {
        m_sceneStrings[(int)SCENE.MAIN_MENU] = "MainMenu";
        m_sceneStrings[(int)SCENE.LOADING] = "Loading";
        m_sceneStrings[(int)SCENE.LEVEL_TUTORIAL] = "Level_Tutorial";
        m_sceneStrings[(int)SCENE.LEVEL_INDUSTRIAL_OPENING] = "Level_Industrial_Opening";
        m_sceneStrings[(int)SCENE.LEVEL_CYBERCITY_PLAYERHUB] = "Level_Cybercity_PlayerHub";
    }

    /// <summary>
    /// Attempted to load scene
    /// Will automatically swap to loading screen
    /// </summary>
    /// <param name="p_scene">Scene to attempted to load</param>
    public void LoadScene(SCENE p_scene)
    {
        if (p_scene == SCENE.SCENE_COUNT || p_scene == SCENE.LOADING)
            return;

        if (!MOARMaths.DoesSceneExist(m_sceneStrings[(int)p_scene]))//No scene found, or not in load list
            return;

        StartCoroutine(LoadSceneAsync(p_scene));
    }


    /// <summary>
    /// Datat is validated, use coroutein to load asyncly
    /// </summary>
    /// <param name="p_scene">Scene to attempted to load, pre checked for validity</param>
    private IEnumerator LoadSceneAsync(SCENE p_scene)
    {
        SceneManager.LoadSceneAsync(m_sceneStrings[(int)SCENE.LOADING]);

        yield return null;
        
        //Setup async loading
        m_asyncSceneLoading = SceneManager.LoadSceneAsync(m_sceneStrings[(int)p_scene]);
        m_asyncSceneLoading.allowSceneActivation = false;
    }

    /// <summary>
    /// Scene has been loaded, ensure loading scene is removed and scene to be loaded is activated
    /// </summary>
    public void SceneLoaded()
    {
        SceneManager.UnloadSceneAsync(m_sceneStrings[(int)SCENE.LOADING]);

        m_asyncSceneLoading.allowSceneActivation = true;
    }
}
