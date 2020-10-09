using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController_MainMenu : UIController
{
    public const string DELETE_OLD_GAME_PROMPT = "Are you sure you want to delete your old save?"; 

    private const int CUSTOM_RESOLUTION_INDEX = 12;
    private const int MIN_RESOLUTION_VAL = 640;
    private const int MAX_RESOLUTION_VAL = 7680;

    //NOTE: Needs to match up with what the drop down has
    private int[,] m_availableResolutions = new int[,] { { 1280, 720 }, { 1920, 1080 }, { 2560, 1440 }, { 3840, 2160 }, { 800, 600 }, { 1024, 768 }, { 1440, 1080 }, { 1920, 1440 }, { 1280, 800 }, { 1440, 900 }, { 1920, 1200 }, { 2560, 1600 } };

    private static Color INTERACTIVE_UI_COLOR = Color.white;
    private static Color NON_INTERACTIVE_UI_COLOR = new Color(0.4f, 0.4f, 0.4f, 1.0f);

    [Header("Canvas Variables")]
    public GameObject m_UIObjectSaveSlots = null;
    public GameObject m_UIObjectMainMenu = null;
    public GameObject m_UIObjectOptions = null;
    public GameObject m_UIObjectCredits = null;
    public GameObject m_UIObjectPrompt = null;

    [Header("Main Menu Variables")]
    public Button m_loadGameButton = null;

    [Header("Save Slot Variables")]
    public TextMeshProUGUI m_saveSlot1BtnText = null;
    public TextMeshProUGUI m_saveSlot2BtnText = null;
    public TextMeshProUGUI m_saveSlot3BtnText = null;

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

        if (m_UIObjectSaveSlots == null || m_UIObjectMainMenu == null || m_UIObjectOptions == null || m_UIObjectCredits == null || m_UIObjectPrompt == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have all its required variables assigned");
#endif
            Destroy(gameObject);
            return;
        }

        if (m_loadGameButton == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have all its required main menu variables assigned");
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

        if (m_saveSlot1BtnText == null || m_saveSlot2BtnText == null || m_saveSlot3BtnText == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have all its required save slot variables assigned");
#endif
            Destroy(gameObject);
            return;
        }

        if (m_promptText == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD 
            Debug.LogWarning(name + " does not have all its required prompt variables assigned");
#endif
            Destroy(gameObject);
            return;
        }

        if (MasterController.Instance.m_currentSaveSlot == -1)
        {
            //Determnine save slot
            bool anySaves = false;

            string savePath = Application.persistentDataPath + "/saves/";

            for (int saveSlotIndex = 0; saveSlotIndex < 3; saveSlotIndex++)
            {
                string savefile = "LevelData_Save" + saveSlotIndex + ".dat";

                //TODO can probably make this better.... A lot better
                if (DataController.DoesFileExist(savePath, savefile))
                {
                    anySaves = true;
                    switch (saveSlotIndex)
                    {
                        case 0:
                            m_saveSlot1BtnText.text = "Load";
                            break;
                        case 1:
                            m_saveSlot2BtnText.text = "Load";
                            break;
                        case 2:
                            m_saveSlot3BtnText.text = "Load";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (saveSlotIndex)
                    {
                        case 0:
                            m_saveSlot1BtnText.text = "New";
                            break;
                        case 1:
                            m_saveSlot2BtnText.text = "New";
                            break;
                        case 2:
                            m_saveSlot3BtnText.text = "New";
                            break;
                        default:
                            break;
                    }
                }
            }

            if (anySaves) //At least one save slot
            {
                m_UIObjectSaveSlots.SetActive(true);
                m_UIObjectMainMenu.SetActive(false);
                m_UIObjectOptions.SetActive(false);
                m_UIObjectCredits.SetActive(false);
                m_UIObjectPrompt.SetActive(false);
            }
            else //No save slots, use first
            {
                MasterController.Instance.m_currentSaveSlot = 0;
                DisplayMainMenu();
            }
        }
        else
        {
            MasterController.Instance.m_currentSaveSlot = 0;
            DisplayMainMenu();
        }
    }

    public void DisplayMainMenu()
    {
        DataController.LoadInGameSaveData();

        //Fill data
        MasterController.Instance.m_playerPrefs = DataController.LoadPlayerPrefs();
        RebuildOptionUIVariables();

        m_UIObjectSaveSlots.SetActive(false);
        m_UIObjectMainMenu.SetActive(true);
        m_UIObjectOptions.SetActive(false);
        m_UIObjectCredits.SetActive(false);
        m_UIObjectPrompt.SetActive(false);

        //Call in start to set defaults after loading data
        OnChange_Resolution();
        OnChange_CustomResolution();

        //Setup graphics
        UpdateGraphicsSettings();

        if (!MasterController.Instance.m_inGameSaveData.m_lastSavePoint.IsValid())
        {
            m_loadGameButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Apply the struct variables to the vairbels found in the UI
    /// </summary>
    public void RebuildOptionUIVariables()
    {
        DataController.PlayerPreferences playerPrefs = MasterController.Instance.m_playerPrefs;

        m_masterVolumeSlider.value = playerPrefs.m_masterVolume;
        m_musicVolumeSlider.value = playerPrefs.m_musicVolume;
        m_SFXVolumeSlider.value = playerPrefs.m_SFXVolume;
        m_voiceVolumeSlider.value = playerPrefs.m_voiceVolume;

        m_subtitleToggle.isOn = playerPrefs.m_subtitles;

        m_performanceDropdown.value = playerPrefs.m_performanceIndex;
        m_resolutionDropdown.value = playerPrefs.m_resolutionIndex;
        m_windowModeDropdown.value = playerPrefs.m_windowModeIndex;

        m_customResXInput.text = playerPrefs.m_customResolutionX.ToString();
        m_customResYInput.text = playerPrefs.m_customResolutionY.ToString();
    }

    /// <summary>
    /// Update the grahps to reflect the settings
    /// TODO add in actual graphics settings 
    /// </summary>
    public void UpdateGraphicsSettings()
    {
        DataController.PlayerPreferences playerPrefs = MasterController.Instance.m_playerPrefs;

        int xResolution = playerPrefs.m_resolutionIndex == CUSTOM_RESOLUTION_INDEX ? playerPrefs.m_customResolutionX : m_availableResolutions[playerPrefs.m_resolutionIndex, 0];
        int yResolution = playerPrefs.m_resolutionIndex == CUSTOM_RESOLUTION_INDEX ? playerPrefs.m_customResolutionY : m_availableResolutions[playerPrefs.m_resolutionIndex, 1];

        //0 = Windowed
        //1 = Borderless Windown
        //2 = fullscreen
        FullScreenMode screenMode = playerPrefs.m_windowModeIndex == 0 ? FullScreenMode.Windowed : playerPrefs.m_windowModeIndex == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.ExclusiveFullScreen;

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

    #region Save Selection

    /// <summary>
    /// New save slot is selected, in the case of already having 
    /// </summary>
    /// <param name="p_saveSlot">Slot to be selected, range should be between 0 and 2 inclusive</param>
    public void Btn_SaveSlotSelected(int p_saveSlot)
    {
        MasterController.Instance.m_currentSaveSlot = p_saveSlot;
        DisplayMainMenu();
    }

    #endregion

    #region Main menu Buttons

    /// <summary>
    /// Button to load game
    /// </summary>
    public void Btn_LoadGame()
    {
        m_mainMenuSceneController.LoadGame();
    }

    /// <summary>
    /// Button to start new game
    /// </summary>
    public void Btn_NewGame()
    {
        if(MasterController.Instance.m_inGameSaveData.m_lastSavePoint.IsValid())//Already a save file, should we overwrite?
        {
            StartCoroutine(NewGamePrompt());
        }
        else
        {
            m_mainMenuSceneController.LoadFirstLevel();
        }
    }

    private IEnumerator NewGamePrompt()
    {
        m_UIObjectPrompt.SetActive(true);
        m_UIObjectMainMenu.SetActive(false);

        m_promptText.text = DELETE_OLD_GAME_PROMPT;

        m_currentPromptState = PROMPT_STATE.AWAITING_INPUT;

        while (m_currentPromptState == PROMPT_STATE.AWAITING_INPUT)
            yield return null;

        if(m_currentPromptState == PROMPT_STATE.PROMPT_ACCECPTED) //Accept prompt
        {
            DataController.RemoveSaveFiles();
            m_mainMenuSceneController.LoadFirstLevel();
        }
        else //Declined prompt
        {
            m_UIObjectPrompt.SetActive(false);
            m_UIObjectMainMenu.SetActive(true);
        }
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
        DataController.PlayerPreferences playerPrefs = MasterController.Instance.m_playerPrefs;

        //Apply changes to stuct
        playerPrefs.m_masterVolume = Mathf.FloorToInt(m_masterVolumeSlider.value);
        playerPrefs.m_musicVolume = Mathf.FloorToInt(m_musicVolumeSlider.value);
        playerPrefs.m_SFXVolume = Mathf.FloorToInt(m_SFXVolumeSlider.value);
        playerPrefs.m_voiceVolume = Mathf.FloorToInt(m_voiceVolumeSlider.value);
        playerPrefs.m_subtitles = m_subtitleToggle;

        playerPrefs.m_performanceIndex = m_performanceDropdown.value;
        playerPrefs.m_resolutionIndex = m_resolutionDropdown.value;
        playerPrefs.m_windowModeIndex = m_windowModeDropdown.value;
        playerPrefs.m_customResolutionX = ValidateCustomResolution(m_customResXInput);
        playerPrefs.m_customResolutionY = ValidateCustomResolution(m_customResYInput);

        MasterController.Instance.m_playerPrefs = playerPrefs;

        //Save
        DataController.SavePlayerPrefs(playerPrefs);
        UpdateGraphicsSettings();

        Btn_ReturnToMainMenu();
    }

    #endregion
}
