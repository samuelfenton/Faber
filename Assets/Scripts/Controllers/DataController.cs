﻿using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataController
{
    [System.Serializable]
    public struct InGameSaveData
    {
        public static InGameSaveData Invalid() { return new InGameSaveData(-1, (int)MasterController.SCENE.SCENE_COUNT); }

        public InGameSaveData(int p_savePointID, int p_saveSceneIndex)
        {
            m_savePointID = p_savePointID;
            m_saveSceneIndex = p_saveSceneIndex;
        }

        /// <summary>
        /// Is this a valid save data?
        /// Values should fall within defined constraints
        /// </summary>
        /// <returns>True when all constraints are met</returns>
        public bool IsValid()
        {
            return m_savePointID > -1 && m_saveSceneIndex > 0 && m_saveSceneIndex < (int)MasterController.SCENE.SCENE_COUNT;
        }

        public int m_savePointID;
        public int m_saveSceneIndex; //Not defined by build index but the enum equivalent found in MasterController.SCENE
    }

    [System.Serializable]
    public struct PlayerPreferences
    {
        public PlayerPreferences(int p_masterVolume = 100, int p_musicVolume = 100, int p_SFXVolume = 100, int p_voiceVolume = 100, bool p_subtitles = true, int p_performanceIndex = 2, int p_resolutionIndex = 3, int p_windowModeIndex = 1, int p_customResolutionX = 640, int p_customResolutionY = 640)
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

    public enum PLAYER_PREFERENCE { MASTER_VOLUME, MUSIC_VOLUME, SFX_VOLUME, VOICE_VOLUME, SUBTITLES, PERFORMANCE_INDEX, RESOLUTION_INDEX, WINDOW_MODE, CUSTOM_RESOLUTION_X, CUSTOM_RESOLUTION_Y }

    public static string[] m_prefsToStrings = new string[] { "Master_Volume", "Music_Volume", "SFX_Volume", "Voice_Volume", "Subtitles", "Performance_Index", "Resolution_Index", "FullScreen_Windowed", "Custom_Resolution_X", "Custom_Resolution_Y" };

    /// <summary>
    /// Fill struct with saved data if available
    /// </summary>
    /// <returns>Saved data, or default struct data when not previously saved</returns>
    public static PlayerPreferences LoadPlayerPrefs()
    {
        PlayerPreferences optionVariables = new PlayerPreferences();

        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREFERENCE.MASTER_VOLUME]))
            optionVariables.m_masterVolume = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.MASTER_VOLUME]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREFERENCE.MUSIC_VOLUME]))
            optionVariables.m_musicVolume = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.MUSIC_VOLUME]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREFERENCE.SFX_VOLUME]))
            optionVariables.m_SFXVolume = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.SFX_VOLUME]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREFERENCE.VOICE_VOLUME]))
            optionVariables.m_voiceVolume = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.VOICE_VOLUME]);

        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREFERENCE.SUBTITLES]))
            optionVariables.m_subtitles = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.SUBTITLES]) == 1;

        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREFERENCE.PERFORMANCE_INDEX]))
            optionVariables.m_performanceIndex = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.PERFORMANCE_INDEX]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREFERENCE.RESOLUTION_INDEX]))
            optionVariables.m_resolutionIndex = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.RESOLUTION_INDEX]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREFERENCE.WINDOW_MODE]))
            optionVariables.m_windowModeIndex = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.WINDOW_MODE]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREFERENCE.CUSTOM_RESOLUTION_X]))
            optionVariables.m_customResolutionX = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.CUSTOM_RESOLUTION_X]);
        if (PlayerPrefs.HasKey(m_prefsToStrings[(int)PLAYER_PREFERENCE.CUSTOM_RESOLUTION_Y]))
            optionVariables.m_customResolutionY = PlayerPrefs.GetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.CUSTOM_RESOLUTION_Y]);

        return optionVariables;
    }

    /// <summary>
    /// Save the given structs data into player prefs 
    /// </summary>
    /// <param name="p_data">No data will be verified, this should be done previously</param>
    public static void SavePlayerPrefs(PlayerPreferences p_data)
    {
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.MASTER_VOLUME], p_data.m_masterVolume);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.MUSIC_VOLUME], p_data.m_musicVolume);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.SFX_VOLUME], p_data.m_SFXVolume);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.VOICE_VOLUME], p_data.m_voiceVolume);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.SUBTITLES], p_data.m_subtitles ? 1 : 0);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.PERFORMANCE_INDEX], p_data.m_performanceIndex);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.RESOLUTION_INDEX], p_data.m_resolutionIndex);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.WINDOW_MODE], p_data.m_windowModeIndex);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.CUSTOM_RESOLUTION_X], p_data.m_customResolutionX);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREFERENCE.CUSTOM_RESOLUTION_Y], p_data.m_customResolutionY);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Save the characters stats
    /// </summary>
    /// <param name="p_stats">Stats to be saved</param>
    public static void SaveCharacterStatistics(CharacterStatistics p_stats)
    {
        string savePath = Application.persistentDataPath + "/saves/";
        string savefile = "PlayerData_Save" + MasterController.Instance.m_currentSaveSlot + ".json";

        string result = JsonUtility.ToJson(p_stats);

        File.WriteAllText(savePath + savefile, result);

    }

    /// <summary>
    /// Load the stas where possibnle for the character
    /// </summary>
    /// <param name="p_stats">Where to save stats to, when no save data is present will use default/param>
    public static void LoadCharacterStatistics(CharacterStatistics p_statistics)
    {
        string savePath = Application.persistentDataPath + "/saves/";
        string savefile = "PlayerData_Save" + MasterController.Instance.m_currentSaveSlot + ".json";

        if (!DoesFileExist(savePath, savefile))
        {
            return;
        }

        string dataAsJson = File.ReadAllText(savePath + savefile);

        JsonUtility.FromJsonOverwrite(dataAsJson, p_statistics);
    }

    /// <summary>
    /// Save the current load point
    /// Will auto update master controller
    /// </summary>
    /// <param name="p_newSavePoint">Point to use</param>
    public static void SaveSavingPoint(Interactable_SavePoint p_newSavePoint)
    {
        MasterController.Instance.m_inGameSaveData = new InGameSaveData(p_newSavePoint.m_uniqueID, (int)MasterController.Instance.m_currentSceneController.m_sceneDefine);

        string savePath = Application.persistentDataPath + "/saves/";
        string savefile = "LevelData_Save" + MasterController.Instance.m_currentSaveSlot + ".dat";

        BinaryFormatter bf = new BinaryFormatter();
        if (DoesFileExist(savePath, savefile))
        {
            FileStream existingFile = File.Open(savePath + savefile, FileMode.Open);
            bf.Serialize(existingFile, MasterController.Instance.m_inGameSaveData);
            existingFile.Close();
        }
        else
        {
            FileStream newFile = File.Create(savePath + savefile);
            bf.Serialize(newFile, MasterController.Instance.m_inGameSaveData);
            newFile.Close();
        }
    }

    /// <summary>
    /// Load up the chaarcters level data, 
    /// </summary>
    /// <returns>true when able to load</returns>
    public static bool LoadGameSavingPoint()
    {
        string savePath = Application.persistentDataPath + "/saves/";
        string savefile = "LevelData_Save" + MasterController.Instance.m_currentSaveSlot + ".dat";

        if (DoesFileExist(savePath, savefile))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(savePath + savefile, FileMode.Open);
            InGameSaveData results = (InGameSaveData)bf.Deserialize(file);
            file.Close();

            MasterController.Instance.m_inGameSaveData = results;
        }
        else
        {
            MasterController.Instance.m_inGameSaveData = InGameSaveData.Invalid();

            return false;
        }

        return true;
    }

    /// <summary>
    /// Does a given file exist on the system?
    /// </summary>
    /// <param name="p_path">Path to file</param>
    /// <param name="p_fileName">File name including extension</param>
    /// <returns>true when file is found</returns>
    public static bool DoesFileExist(string p_path, string p_fileName)
    {
        if (!Directory.Exists(p_path))
        {
            Directory.CreateDirectory(p_path);
        }

        string filePath = p_path + p_fileName;

        return File.Exists(filePath);
    }

    /// <summary>
    /// Delete saves files as needed
    /// </summary>
    public static void RemoveSaveFiles()
    {
        string savePath = Application.persistentDataPath + "/saves/";
        string levelDataSavefile = "LevelData_Save" + MasterController.Instance.m_currentSaveSlot + ".dat";
        string characterDataSavefile = "PlayerData_Save" + MasterController.Instance.m_currentSaveSlot + ".json";

        if (DoesFileExist(savePath, levelDataSavefile))
        {
            File.Delete(savePath + levelDataSavefile);
        }
        if (DoesFileExist(savePath, characterDataSavefile))
        {
            File.Delete(savePath + characterDataSavefile);
        }

        MasterController.Instance.m_inGameSaveData = InGameSaveData.Invalid();
    }
}
