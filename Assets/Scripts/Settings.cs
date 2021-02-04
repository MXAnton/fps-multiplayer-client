using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings
{
    public static float sensivity = 3f;
    public static bool cursorLocked = false;

    public static float mainVolume = 0.5f;
    public static float musicVolume = 0.5f;


    public static int screenWidth = 1920;
    public static int screenHeight = 1080;
    public static int refreshRate = 60;
    public static FullScreenMode fullscreenMode = FullScreenMode.MaximizedWindow;


    public static void SaveSettings()
    {
        PlayerPrefs.SetFloat("sensivity", sensivity);
        PlayerPrefs.SetFloat("mainVolume", mainVolume);
        PlayerPrefs.SetFloat("musicVolume", musicVolume);

        PlayerPrefs.SetInt("screenWidth", screenWidth);
        PlayerPrefs.SetInt("screenHeight", screenHeight);
        PlayerPrefs.SetInt("refreshRate", refreshRate);
        PlayerPrefs.SetInt("fullscreenMode", FullscreenModeToInt(fullscreenMode));

        PlayerPrefs.Save();
    }
    public static void LoadSettings()
    {
        sensivity = PlayerPrefs.GetFloat("sensivity", sensivity);
        mainVolume = PlayerPrefs.GetFloat("mainVolume", mainVolume);
        musicVolume = PlayerPrefs.GetFloat("musicVolume", musicVolume);

        screenWidth = PlayerPrefs.GetInt("screenWidth", screenWidth);
        screenHeight = PlayerPrefs.GetInt("screenHeight", screenHeight);
        refreshRate = PlayerPrefs.GetInt("refreshRate", refreshRate);
        fullscreenMode = IntToFullscreenMode(PlayerPrefs.GetInt("fullscreenMode", FullscreenModeToInt(fullscreenMode)));
    }

    public static int FullscreenModeToInt(FullScreenMode _fullscreenMode)
    {
        int _i = 0;
        switch (_fullscreenMode)
        {
            case FullScreenMode.ExclusiveFullScreen:
                _i = 0;
                break;
            case FullScreenMode.FullScreenWindow:
                _i = 1;
                break;
            case FullScreenMode.MaximizedWindow:
                _i = 2;
                break;
            default:
                _i = 3;
                break;
        }

        return _i;
    }
    public static FullScreenMode IntToFullscreenMode(int _i)
    {
        switch (_i)
        {
            case 0:
                return FullScreenMode.ExclusiveFullScreen;
            case 1:
                return FullScreenMode.FullScreenWindow;
            case 2:
                return FullScreenMode.MaximizedWindow;
            default:
                return FullScreenMode.Windowed;
        }
    }
}
