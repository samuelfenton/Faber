using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MasterController : MonoBehaviour
{
    //Naming scheme for levels excluding main menu and loading should follow the following "LEVEL_THEME_AREA"
    //THEME - Where it is, PROTOTYPE, CHILDHOOD, SLUMS, CYBERCITY, INDUSTRIAL, EDO
    //AREA - Where within the theme is it? opening, waterfront etc
    public enum SCENE {MAIN_MENU, LOADING, LEVEL_PROTOTYPE_TUTORIAL, LEVEL_CYBERCITY_PLAYERHUB, LEVEL_INDUSTRIAL_OPENING, SCENE_COUNT}
    private string[] m_sceneStrings = new string[(int)SCENE.SCENE_COUNT];
    public static MasterController Instance { get; private set; }

    //In scene varibles
    [HideInInspector]
    public SceneController m_currentSceneController = null;
    [HideInInspector]
    public UIController m_currentUIController = null;

    [HideInInspector]
    public AsyncOperation m_asyncSceneLoading = null;
    [HideInInspector]
    public DataController.InGameSaveData m_inGameSaveData;
    [HideInInspector]
    public DataController.PlayerPreferences m_playerPrefs;

    public int m_currentSaveSlot = -1;

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

        DontDestroyOnLoad(topObject);

        BuildSceneStrings();

        m_currentSaveSlot = -1;

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
            m_currentSceneController.Init();

        if (m_currentUIController != null)
            m_currentUIController.Init();
    }

    private void Update()
    {
        if (m_currentSceneController != null)
            m_currentSceneController.UpdateSceneController();

        if (m_currentUIController != null)
            m_currentUIController.UpdateUIController();
    }

    /// <summary>
    /// Assign the strings as needed to be used with enums
    /// </summary>
    public void BuildSceneStrings()
    {
        m_sceneStrings[(int)SCENE.MAIN_MENU] = "MainMenu";
        m_sceneStrings[(int)SCENE.LOADING] = "Loading";
        m_sceneStrings[(int)SCENE.LEVEL_PROTOTYPE_TUTORIAL] = "Level_Tutorial";
        m_sceneStrings[(int)SCENE.LEVEL_INDUSTRIAL_OPENING] = "Level_Industrial_Opening";
        m_sceneStrings[(int)SCENE.LEVEL_CYBERCITY_PLAYERHUB] = "Level_Cybercity_PlayerHub";
    }

    /// <summary>
    /// Convert scene name to scene enum
    /// </summary>
    /// <param name="p_sceneString">String to attempt to find</param>
    /// <returns>Scene enum, defaulted to SCENE_COUNT, when not found</returns>
    public SCENE GetSceneEnum(string p_sceneString)
    {
        for (int sceneIndex = 0; sceneIndex < (int)SCENE.SCENE_COUNT; sceneIndex++)
        {
            if (m_sceneStrings[sceneIndex] == p_sceneString)
                return (SCENE)sceneIndex;
        }

        return SCENE.SCENE_COUNT;
    }

    /// <summary>
    /// Attempted to load scene
    /// Will automatically swap to loading screen
    /// </summary>
    /// <param name="p_scene">Scene to attempted to load</param>
    /// <param name="p_useLoadingScreen">Should we use the loading screen? In cases such as loading credits or main menu, theres no need to wait</param>
    public void LoadScene(SCENE p_scene, bool p_useLoadingScreen)
    {
        if (p_scene == SCENE.SCENE_COUNT || p_scene == SCENE.LOADING)
            return;

        if (!MOARMaths.DoesSceneExist(m_sceneStrings[(int)p_scene]))//No scene found, or not in load list
            return;

        if(p_useLoadingScreen)
        {
            StartCoroutine(LoadSceneAsync(p_scene));
        }
        else
        {
            StartCoroutine(LoadSceneImmediatly(p_scene));
        }
    }

    /// <summary>
    /// Load a scene immediatly
    /// </summary>
    /// <param name="p_scene">Scene to attempt to load</param>
    private IEnumerator LoadSceneImmediatly(SCENE p_scene)
    {
        SceneManager.LoadScene(m_sceneStrings[(int)p_scene]);
        yield return null;

        InitSceneControllers();
    }


    /// <summary>
    /// Data is validated, use coroutein to load asyncly
    /// </summary>
    /// <param name="p_scene">Scene to attempted to load, pre checked for validity</param>
    private IEnumerator LoadSceneAsync(SCENE p_scene)
    {
        AsyncOperation loadingAsync = SceneManager.LoadSceneAsync(m_sceneStrings[(int)SCENE.LOADING], LoadSceneMode.Single);

        //Setup async loading
        m_asyncSceneLoading = SceneManager.LoadSceneAsync(m_sceneStrings[(int)p_scene], LoadSceneMode.Additive);
        m_asyncSceneLoading.allowSceneActivation = false;

        while(!loadingAsync.isDone)
        {
            yield return null;
        }

        InitSceneControllers();//initialising the loading scene
    }

    /// <summary>
    /// Scene has been loaded, ensure loading scene is removed and scene to be loaded is activated
    /// </summary>
    public void SceneHasLoaded()
    {
        m_asyncSceneLoading.allowSceneActivation = true;

        StartCoroutine(SceneLoadedAsync());
    }

    /// <summary>
    /// Used to asyc unload the loading screen
    /// </summary>
    public IEnumerator SceneLoadedAsync()
    {
        while(!m_asyncSceneLoading.isDone) //Go form 0.9 to 1.0 due to holding activation
        {
            yield return null;
        }

        AsyncOperation unloading = SceneManager.UnloadSceneAsync(m_sceneStrings[(int)SCENE.LOADING]);

        while (!unloading.isDone)
        {
            yield return null;
        }

        InitSceneControllers();
    }
}
