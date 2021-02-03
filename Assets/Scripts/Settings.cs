using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings
{
    public static float sensivity = 3f;
    public static bool cursorLocked = false;

    public static float mainVolume = 0.5f;
    public static float musicVolume = 0.5f;


    public static void SaveSettings()
    {
        PlayerPrefs.SetFloat("sensivity", sensivity);
        PlayerPrefs.SetFloat("mainVolume", mainVolume);
        PlayerPrefs.SetFloat("musicVolume", musicVolume);

        PlayerPrefs.Save();
    }
    public static void LoadSettings()
    {
        sensivity = PlayerPrefs.GetFloat("sensivity", sensivity);
        mainVolume = PlayerPrefs.GetFloat("mainVolume", mainVolume);
        musicVolume = PlayerPrefs.GetFloat("musicVolume", musicVolume);
    }
}
