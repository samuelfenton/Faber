using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController_MainMenu : UIController
{
    public struct OptionVariables
    {
        public OptionVariables(int p_masterVolume = 100, int p_musicVolume = 100, int p_SFXVolume = 100, int p_voiceVolume = 100, bool p_subtitles = true, int p_performanceIndex = 2, int p_resolutionIndex = 3, int p_windowModeIndex = 1, int p_customResolutionX = 640, int p_customResolutionY = 640)
        {
            m_masterVolume = p_masterVolume;
            m_musicVolume = p_musicVolume;
            m_SFXVolume = p_SFXVolume;
            m_voiceVolume = p_voiceVolume;

            m_subtitles = p_subtitles;

            m_performanceIndex = p_performanceIndex;
            m_resolutionIndex = p_resolutionIndex;
            m_windowModeIndex = p_windowModeIndex;
            m_customResolutionX = p_customResolutionX;
            m_customResolutionY = p_customResolutionY;
        }

        public int m_masterVolume;
        public int m_musicVolume;
        public int m_SFXVolume;
        public int m_voiceVolume;

        public bool m_subtitles;

        public int m_performanceIndex;  
        public int m_resolutionIndex;
        public int m_windowModeIndex;
        public int m_customResolutionX;
        public int m_customResolutionY;
    }

    public OptionVariables m_optionsVariables;

    private const int CUSTOM_RESOLUTION_INDEX = 12;
    private const int MIN_RESOLUTION_VAL = 640;
    private const int MAX_RESOLUTION_VAL = 7680;

    //NOTE: Needs to match up with what the drop down has
    private int[,] m_availableResolutions = new int[,] { { 1280, 720 }, { 1920, 1080 }, { 2560, 1440 }, { 3840, 2160 }, { 800, 600 }, { 1024, 768 }, { 1440, 1080 }, { 1920, 1440 }, { 1280, 800 }, { 1440, 900 }, { 1920, 1200 }, { 2560, 1600 } };

    private static Color INTERACTIVE_UI_COLOR = Color.white;
    private static Color NON_INTERACTIVE_UI_COLOR = new Color(0.4f, 0.4f, 0.4f, 1.0f);

    [Header("Assigned Variables")]
    public GameObject m_UIObjectMainMenu = null;
    public GameObject m_UIObjectOptions = null;
    public GameObject m_UIObjectCredits = null;

    [Header("Options Variables")]
    public Slider m_masterVolumeSlider = null;
    public Slider m_musicVolumeSlider = null;
    public Slider m_SFXVolumeSlider = null;
    public Slider m_voiceVolumeSlider = null;

    public Toggle m_subtitleToggle = null;

    public TMP_Dropdown m_performanceDropdown = null;
    public TMP_Dropdown m_resolutionDropdown = null;
    public TMP_Dropdown m_windowModeDropdown = null;


    public TMP_InputField m_customResXInput = null;
    public TMP_InputField m_customResYInput = null;

    public TextMeshProUGUI m_customResXLabel = null;
    public TextMeshProUGUI m_customResYLabel = null;

    private SceneController_MainMenu m_mainMenuSceneController = null;

    /// <summary>
    /// Setup variables to be used in UI
    /// </summary>
    public override void Init()
    {
        base.Init();

        m_mainMenuSceneController = (SceneController_MainMenu)(MasterController.Instance.m_currentSceneController);

        if (m_mainMenuSceneController == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " is unable to find a SceneController_MainMenu script, maybe its missing?");
#endif
            Destroy(gameObject);
            return;
        }

        if (m_UIObjectMainMenu == null || m_UIObjectOptions == null || m_UIObjectCredits == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have all its required variables assigned");
#endif
            Destroy(gameObject);
            return;
        }

        if (m_masterVolumeSlider == null || m_musicVolumeSlider == null || m_SFXVolumeSlider == null || m_voiceVolumeSlider == null ||
            m_subtitleToggle == null || 
            m_performanceDropdown == null || m_resolutionDropdown == null || m_windowModeDropdown == null || m_customResXInput == null || m_customResYInput == null || 
            m_customResXLabel == null || m_customResYLabel == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have all its required options variables assigned");
#endif
            Destroy(gameObject);
            return;
        }

        //Fill data
        m_optionsVariables = DataController.GetOptionsData();
        RebuildOptionUIVariables();

        m_UIObjectMainMenu.SetActive(true);
        m_UIObjectOptions.SetActive(false);
        m_UIObjectCredits.SetActive(false);

        //Call in start to set defaults after loading data
        OnChange_Resolution();
        OnChange_CustomResolution();

        //Setup graphics
        UpdateGraphicsSettings();
    }

    /// <summary>
    /// Apply the struct variables to the vairbels found in the UI
    /// </summary>
    public void RebuildOptionUIVariables()
    {
        m_masterVolumeSlider.value = m_optionsVariables.m_masterVolume;
        m_musicVolumeSlider.value = m_optionsVariables.m_musicVolume;
        m_SFXVolumeSlider.value = m_optionsVariables.m_SFXVolume;
        m_voiceVolumeSlider.value = m_optionsVariables.m_voiceVolume;

        m_subtitleToggle.isOn = m_optionsVariables.m_subtitles;

        m_performanceDropdown.value = m_optionsVariables.m_performanceIndex;
        m_resolutionDropdown.value = m_optionsVariables.m_resolutionIndex;
        m_windowModeDropdown.value = m_optionsVariables.m_windowModeIndex;

        m_customResXInput.text = m_optionsVariables.m_customResolutionX.ToString();
        m_customResYInput.text = m_optionsVariables.m_customResolutionY.ToString();
    }

    /// <summary>
    /// Update the grahps to reflect the settings
    /// TODO add in actual graphics settings 
    /// </summary>
    public void UpdateGraphicsSettings()
    {
        int xResolution = m_optionsVariables.m_resolutionIndex == CUSTOM_RESOLUTION_INDEX ? m_optionsVariables.m_customResolutionX : m_availableResolutions[m_optionsVariables.m_resolutionIndex, 0];
        int yResolution = m_optionsVariables.m_resolutionIndex == CUSTOM_RESOLUTION_INDEX ? m_optionsVariables.m_customResolutionY : m_availableResolutions[m_optionsVariables.m_resolutionIndex, 1];

        //0 = Windowed
        //1 = Borderless Windown
        //2 = fullscreen
        FullScreenMode screenMode = m_optionsVariables.m_windowModeIndex == 0 ? FullScreenMode.Windowed : m_optionsVariables.m_windowModeIndex == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.ExclusiveFullScreen;

        Screen.SetResolution(xResolution, yResolution, screenMode);
    }

    #region Option on change

    /// <summary>
    /// Called when resoulition dorpdown has had a change in value
    /// </summary>
    public void OnChange_Resolution()
    {
        if(m_resolutionDropdown.value == CUSTOM_RESOLUTION_INDEX)
        {
            m_customResXInput.interactable = true;
            m_customResYInput.interactable = true;

            m_customResXLabel.color = INTERACTIVE_UI_COLOR;
            m_customResYLabel.color = INTERACTIVE_UI_COLOR;
        }
        else
        {
            m_customResXInput.interactable = false;
            m_customResYInput.interactable = false;

            m_customResXLabel.color = NON_INTERACTIVE_UI_COLOR;
            m_customResYLabel.color = NON_INTERACTIVE_UI_COLOR;
        }
    }

    /// <summary>
    /// Called when custom resolution is deslected
    /// Not on change as values are "snapped" to be valid
    /// </summary>
    public void OnChange_CustomResolution()
    {
        m_customResXInput.text = ValidateCustomResolution(m_customResXInput).ToString();
        m_customResYInput.text = ValidateCustomResolution(m_customResYInput).ToString();
    }

    /// <summary>
    /// Keep text value restricted between min and max resolution values
    /// </summary>
    /// <param name="p_textVal">Input field to check with be set to only accept int inputs</param>
    /// <returns>A valid resolution</returns>
    private int ValidateCustomResolution(TMP_InputField p_textVal)
    {
        if (int.TryParse(p_textVal.text, out int textVal))
        {
            if (textVal > MAX_RESOLUTION_VAL)
                textVal = MAX_RESOLUTION_VAL;

            else if (textVal < MIN_RESOLUTION_VAL)
                textVal = MIN_RESOLUTION_VAL;

            return textVal;
        }

        return MIN_RESOLUTION_VAL;
    }

    #endregion

    #region Main menu Buttons

    /// <summary>
    /// Button to load game
    /// </summary>
    public void Btn_LoadGame()
    {
        //TODO, actually load game
        m_mainMenuSceneController.LoadFirstLevel();
    }

    /// <summary>
    /// Button to start new game
    /// </summary>
    public void Btn_NewGame()
    {
        m_mainMenuSceneController.LoadFirstLevel();
    }

    /// <summary>
    /// Button to change settings
    /// </summary>
    public void Btn_Options()
    {
        m_UIObjectMainMenu.SetActive(false);
        m_UIObjectOptions.SetActive(true);
    }

    /// <summary>
    /// Button to view credits
    /// </summary>
    public void Btn_Credits()
    {
        m_UIObjectMainMenu.SetActive(false);
        m_UIObjectCredits.SetActive(true);
    }

    /// <summary>
    /// Button to quit to desktop
    /// </summary>
    public void Btn_Quit()
    {
        Application.Quit();
    }
    #endregion

    #region Generic buttons
    /// <summary>
    /// Swap canvas to show main menu
    /// TODO, add in animaiton effect, fade in/slide in etc
    /// </summary>
    public void Btn_ReturnToMainMenu()
    {
        m_UIObjectMainMenu.SetActive(true);
        m_UIObjectOptions.SetActive(false);
        m_UIObjectCredits.SetActive(false);
    }

    #endregion

    #region Option Buttons

    /// <summary>
    /// Discard all changes made
    /// </summary>
    public void Btn_RevertChanges()
    {
        RebuildOptionUIVariables();

        Btn_ReturnToMainMenu();
    }

    /// <summary>
    /// Apply the changes made
    /// Save new varibles to player prefs
    /// </summary>
    public void Btn_ApplyChanges()
    {
        //Apply changes to stuct
        m_optionsVariables.m_masterVolume = Mathf.FloorToInt(m_masterVolumeSlider.value);
        m_optionsVariables.m_musicVolume = Mathf.FloorToInt(m_musicVolumeSlider.value);
        m_optionsVariables.m_SFXVolume = Mathf.FloorToInt(m_SFXVolumeSlider.value);
        m_optionsVariables.m_voiceVolume = Mathf.FloorToInt(m_voiceVolumeSlider.value);
        m_optionsVariables.m_subtitles = m_subtitleToggle;

        m_optionsVariables.m_performanceIndex = m_performanceDropdown.value;
        m_optionsVariables.m_resolutionIndex = m_resolutionDropdown.value;
        m_optionsVariables.m_windowModeIndex = m_windowModeDropdown.value;
        m_optionsVariables.m_customResolutionX = ValidateCustomResolution(m_customResXInput);
        m_optionsVariables.m_customResolutionY = ValidateCustomResolution(m_customResYInput);

        //Save
        DataController.SaveOptionsData(m_optionsVariables);
        UpdateGraphicsSettings();

        Btn_ReturnToMainMenu();
    }

    #endregion
}
