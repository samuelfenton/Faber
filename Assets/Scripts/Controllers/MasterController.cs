using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MasterController : MonoBehaviour
{
    //Naming scheme for levels excluding main menu and loading should follow the following "LEVEL_AREA"
    public enum SCENE {MAIN_MENU, LOADING, LEVEL_TUTORIAL, LEVEL_CHILDHOOD, LEVEL_CYBERCITY, LEVEL_INDUSTRIAL, LEVEL_EDO, LEVEL_PAINTING}
    private Dictionary<SCENE, string> m_sceneStrings = new Dictionary<SCENE, string>() { {SCENE.MAIN_MENU, "MainMenu" }, { SCENE.LOADING, "Loading" }, { SCENE.LEVEL_TUTORIAL, "Level_Tutorial" }, 
        { SCENE.LEVEL_CHILDHOOD, "Level_Childhood" }, { SCENE.LEVEL_CYBERCITY, "Level_CyberCity" }, { SCENE.LEVEL_INDUSTRIAL, "Level_Industrial" }, { SCENE.LEVEL_EDO, "Level_Edo" }, { SCENE.LEVEL_PAINTING, "Level_Painting" } };
    public static MasterController Instance { get; private set; }

    //In scene varibles
    [HideInInspector]
    public int m_currentSaveSlot = -1;

    [HideInInspector]
    public SceneController m_currentSceneController = null;
    [HideInInspector]
    public UIController m_currentUIController = null;

    [HideInInspector]
    public AsyncOperation m_asyncSceneLoading = null;
    [HideInInspector]
    public DataController.PlayerPreferences m_playerPrefs;

    //In game save
    [HideInInspector]
    public DataController.InGameSaveData m_inGameSaveData;

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

    private void FixedUpdate()
    {
        if (m_currentSceneController != null)
            m_currentSceneController.FixedUpdateSceneController();
    }

    /// <summary>
    /// Convert scene name to scene enum
    /// </summary>
    /// <param name="p_sceneString">String to attempt to find</param>
    /// <returns>Scene enum, defaulted to MAINMENU, when not found</returns>
    public SCENE GetSceneEnum(string p_sceneString)
    {
        foreach (KeyValuePair<SCENE, string> scenePair in m_sceneStrings)
        {
            if (scenePair.Value == p_sceneString)
                return scenePair.Key;
        }

        return SCENE.MAIN_MENU;
    }

    /// <summary>
    /// Attempted to load scene
    /// Will automatically swap to loading screen
    /// </summary>
    /// <param name="p_scene">Scene to attempted to load</param>
    /// <param name="p_useLoadingScreen">Should we use the loading screen? In cases such as loading credits or main menu, theres no need to wait</param>
    public void LoadScene(SCENE p_scene, bool p_useLoadingScreen)
    {
        if (p_scene == SCENE.LOADING)
            return;

        if (!MOARMaths.DoesSceneExist(m_sceneStrings[p_scene]))//No scene found, or not in load list
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
        SceneManager.LoadScene(m_sceneStrings[p_scene]);
        yield return null;

        InitSceneControllers();
    }


    /// <summary>
    /// Data is validated, use coroutein to load asyncly
    /// </summary>
    /// <param name="p_scene">Scene to attempted to load, pre checked for validity</param>
    private IEnumerator LoadSceneAsync(SCENE p_scene)
    {
        AsyncOperation loadingAsync = SceneManager.LoadSceneAsync(m_sceneStrings[SCENE.LOADING], LoadSceneMode.Single);

        //Setup async loading
        m_asyncSceneLoading = SceneManager.LoadSceneAsync(m_sceneStrings[p_scene], LoadSceneMode.Additive);
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

        AsyncOperation unloading = SceneManager.UnloadSceneAsync(m_sceneStrings[SCENE.LOADING]);

        while (!unloading.isDone)
        {
            yield return null;
        }

        InitSceneControllers();
    }
}
