using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class DataController
{
    [SerializeField]
    public struct InGameSaveData
    {
        public InGameSaveData(int p_savePointID, int p_saveSceneIndex)
        {
            m_savePointID = p_savePointID;
            m_saveSceneIndex = p_saveSceneIndex;
        }

        public int m_savePointID;
        public int m_saveSceneIndex; //Not defined by build index but the enum equivalent found in MasterController.SCENE
    }

    public enum PLAYER_PREF_VARIABLES { MASTER_VOLUME, MUSIC_VOLUME, SFX_VOLUME, VOICE_VOLUME, SUBTITLES, PERFORMANCE_INDEX, RESOLUTION_INDEX, WINDOW_MODE, CUSTOM_RESOLUTION_X, CUSTOM_RESOLUTION_Y }

    public static string[] m_prefsToStrings = new string[] { "Master_Volume", "Music_Volume", "SFX_Volume", "Voice_Volume", "Subtitles", "Performance_Index", "Resolution_Index", "FullScreen_Windowed", "Custom_Resolution_X", "Custom_Resolution_Y" };

    /// <summary>
    /// Fill struct with saved data if available
    /// </summary>
    /// <returns>Saved data, or default struct data when not previously saved</returns>
    public static UIController_MainMenu.OptionVariables GetOptionsData()
    {
        UIController_MainMenu.OptionVariables optionVariables = new UIController_MainMenu.OptionVariables();

        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.MASTER_VOLUME]))
            optionVariables.m_masterVolume = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.MASTER_VOLUME]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.MUSIC_VOLUME]))
            optionVariables.m_musicVolume = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.MUSIC_VOLUME]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.SFX_VOLUME]))
            optionVariables.m_SFXVolume = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.SFX_VOLUME]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.VOICE_VOLUME]))
            optionVariables.m_voiceVolume = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.VOICE_VOLUME]);

        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.SUBTITLES]))
            optionVariables.m_subtitles = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.SUBTITLES]) == 1;

        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.PERFORMANCE_INDEX]))
            optionVariables.m_performanceIndex = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.PERFORMANCE_INDEX]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.RESOLUTION_INDEX]))
            optionVariables.m_resolutionIndex = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.RESOLUTION_INDEX]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.WINDOW_MODE]))
            optionVariables.m_windowModeIndex = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.WINDOW_MODE]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.CUSTOM_RESOLUTION_X]))
            optionVariables.m_customResolutionX = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.CUSTOM_RESOLUTION_X]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.CUSTOM_RESOLUTION_Y]))
            optionVariables.m_customResolutionY = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.CUSTOM_RESOLUTION_Y]);

        return optionVariables;
    }

    /// <summary>
    /// Save the given structs data into player prefs 
    /// </summary>
    /// <param name="p_data">No data will be verified, this should be done previously</param>
    public static void SaveOptionsData(UIController_MainMenu.OptionVariables p_data)
    {
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.MASTER_VOLUME], p_data.m_masterVolume);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.MUSIC_VOLUME], p_data.m_musicVolume);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.SFX_VOLUME], p_data.m_SFXVolume);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.VOICE_VOLUME], p_data.m_voiceVolume);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.SUBTITLES], p_data.m_subtitles ? 1 : 0);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.PERFORMANCE_INDEX], p_data.m_performanceIndex);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.RESOLUTION_INDEX], p_data.m_resolutionIndex);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.WINDOW_MODE], p_data.m_windowModeIndex);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.CUSTOM_RESOLUTION_X], p_data.m_customResolutionX);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.CUSTOM_RESOLUTION_Y], p_data.m_customResolutionY);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Save the characters stats
    /// </summary>
    /// <param name="p_stats">Stats to be saved</param>
    public static void SaveCharacterStats(CharacterStatistics p_stats)
    {
        string savePath = Application.persistentDataPath + "/saves/";
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        //TODO add in multiple save files
        string saveFile = "PlayerData_Save" + 0;

        string filePath = savePath + saveFile + ".json";

        string result = JsonUtility.ToJson(p_stats);

        File.WriteAllText(filePath, result);
    }

    /// <summary>
    /// Load the stas where possibnle for the character
    /// </summary>
    /// <param name="p_stats">Where to save stats to, when no save data is present will use default/param>
    public static void LoadCharacterStats(CharacterStatistics p_statistics)
    {
        string loadPath = Application.persistentDataPath + "/saves/";
        if (!Directory.Exists(loadPath))
        {
            return;
        }

        //TODO add in multiple save files
        string loadFile = "PlayerData_Save" + 0;

        string filePath = loadPath + loadFile + ".json";

        if (!File.Exists(filePath))
        {
            return;
        }

        string dataAsJson = File.ReadAllText(filePath);

        JsonUtility.FromJsonOverwrite(dataAsJson, p_statistics);
    }

    /// <summary>
    /// Load up the chaarcters level data, 
    /// </summary>
    /// <returns>true when able to load</returns>
    public static bool LoadCharacterLevelData()
    {
        string loadPath = Application.persistentDataPath + "/saves/";
        if (!Directory.Exists(loadPath))
        {
            return false;
        }

        //TODO add in multiple save files
        string loadFile = "LevelData_Save" + 0;

        string filePath = loadPath + loadFile + ".json";

        if (!File.Exists(filePath))
        {
            return false;
        }

        string dataAsJson = File.ReadAllText(filePath);

        JsonUtility.FromJsonOverwrite(dataAsJson, MasterController.Instance.m_lastInGameSaveData);

        return true;
    }

    /// <summary>
    /// Save the current load point
    /// Will auto update master controller
    /// </summary>
    /// <param name="p_newSavePoint">Point to use</param>
    public static void SaveLevelData(LevelSavePoint p_newSavePoint)
    {
        MasterController.Instance.m_lastInGameSaveData = new InGameSaveData(p_newSavePoint.m_uniqueID, (int)MasterController.Instance.m_currentScene);

        string savePath = Application.persistentDataPath + "/saves/";
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        //TODO add in multiple save files
        string saveFile = "LevelData_Save" + 0;

        string filePath = savePath + saveFile + ".json";

        string result = JsonUtility.ToJson(MasterController.Instance.m_lastInGameSaveData);

        File.WriteAllText(filePath, result);
    }
}
