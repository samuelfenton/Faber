using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataController
{
    public enum PLAYER_PREF_VARIABLES {MASTER_VOLUME, MUSIC_VOLUME, SFX_VOLUME, VOICE_VOLUME, SUBTITLES, PERFORMANCE_INDEX, RESOLUTION_INDEX, CUSTOM_RESOLUTION_X, CUSTOM_RESOLUTION_Y}

    public static string[] m_prefsToStrings = new string[] { "Master_Volume", "Music_Volume", "SFX_Volume", "Voice_Volume", "Subtitles", "Performance_Index", "Resolution_Index", "Custom_Resolution_X", "Custom_Resolution_Y" };

    /// <summary>
    /// Initialise the data used in game. 
    /// </summary>
    public void Init()
    {

    }

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
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.CUSTOM_RESOLUTION_X], p_data.m_customResolutionX);
        PlayerPrefs.SetInt(m_prefsToStrings[(int)PLAYER_PREF_VARIABLES.CUSTOM_RESOLUTION_Y], p_data.m_customResolutionY);

        PlayerPrefs.Save();
    }

}
