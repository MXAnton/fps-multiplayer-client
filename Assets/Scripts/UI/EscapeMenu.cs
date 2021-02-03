using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EscapeMenu : MonoBehaviour
{
    public UIManager uiManager;

    public GameObject mainMenu;
    public GameObject settingsMenu;

    [Header("Settings")]
    public Slider sensivitySlider;
    public TMP_InputField sensivityInputField;

    public Slider mainVolumeSlider;
    public TMP_InputField mainVolumeInputField;

    public Slider musicVolumeSlider;
    public TMP_InputField musicVolumeInputField;

    private void Start()
    {
        sensivitySlider.value = Settings.sensivity;
        sensivityInputField.text = "" + Settings.sensivity;

        mainVolumeSlider.value = Settings.mainVolume;
        mainVolumeInputField.text = "" + Settings.mainVolume;

        musicVolumeSlider.value = Settings.musicVolume;
        musicVolumeInputField.text = "" + Settings.musicVolume;
    }

    #region settings
    public void SetSensivitySliderValue()
    {
        if (sensivityInputField.text == "" || sensivityInputField.text == null || sensivityInputField.text.Contains(",") && sensivityInputField.text.Length <= 1)
        {
            return;
        }

        float _newSliderValue = float.Parse(sensivityInputField.text);
        if (_newSliderValue < sensivitySlider.minValue)
        {
            _newSliderValue = sensivitySlider.minValue;
            sensivityInputField.text = "" + _newSliderValue;
        }
        else if (_newSliderValue > sensivitySlider.maxValue)
        {
            _newSliderValue = sensivitySlider.maxValue;
            sensivityInputField.text = "" + _newSliderValue;
        }

        sensivitySlider.value = _newSliderValue;

        Settings.sensivity = sensivitySlider.value;

        ApplySettings();
    }
    public void OnSensivityValueUpdate()
    {
        sensivityInputField.text = "" + Mathf.Round(sensivitySlider.value * 10) / 10;

        Settings.sensivity = sensivitySlider.value;

        ApplySettings();
    }

    public void SetMainVolumeSliderValue()
    {
        if (mainVolumeInputField.text == "" || mainVolumeInputField.text == null || mainVolumeInputField.text.Contains(",") && mainVolumeInputField.text.Length <= 1)
        {
            return;
        }

        float _newSliderValue = float.Parse(mainVolumeInputField.text);
        if (_newSliderValue < mainVolumeSlider.minValue)
        {
            _newSliderValue = mainVolumeSlider.minValue;
            mainVolumeInputField.text = "" + _newSliderValue;
        }
        else if (_newSliderValue > mainVolumeSlider.maxValue)
        {
            _newSliderValue = mainVolumeSlider.maxValue;
            mainVolumeInputField.text = "" + _newSliderValue;
        }

        mainVolumeSlider.value = _newSliderValue;

        Settings.mainVolume = mainVolumeSlider.value;

        ApplySettings();
    }
    public void OnMainVolumeValueUpdate()
    {
        mainVolumeInputField.text = "" + Mathf.Round(mainVolumeSlider.value * 100) / 100;

        Settings.mainVolume = mainVolumeSlider.value;

        ApplySettings();
    }

    public void SetMusicVolumeSliderValue()
    {
        if (musicVolumeInputField.text == "" || musicVolumeInputField.text == null || musicVolumeInputField.text.Contains(",") && musicVolumeInputField.text.Length <= 1)
        {
            return;
        }

        float _newSliderValue = float.Parse(musicVolumeInputField.text);
        if (_newSliderValue < musicVolumeSlider.minValue)
        {
            _newSliderValue = musicVolumeSlider.minValue;
            musicVolumeInputField.text = "" + _newSliderValue;
        }
        else if (_newSliderValue > musicVolumeSlider.maxValue)
        {
            _newSliderValue = musicVolumeSlider.maxValue;
            musicVolumeInputField.text = "" + _newSliderValue;
        }

        musicVolumeSlider.value = _newSliderValue;

        Settings.musicVolume = musicVolumeSlider.value;

        ApplySettings();
    }
    public void OnMusicVolumeValueUpdate()
    {
        musicVolumeInputField.text = "" + Mathf.Round(musicVolumeSlider.value * 100) / 100;

        Settings.musicVolume = musicVolumeSlider.value;

        ApplySettings();
    }

    public void ApplySettings()
    {
        Settings.SaveSettings();

        AudioController.instance.SetMainVolume(Settings.mainVolume);
    }
    #endregion

    public void Continue()
    {
        uiManager.ButtonClicked();
        uiManager.ToggleEscapeMenu();
    }
    public void OnSettingsHit()
    {
        uiManager.ButtonClicked();
        settingsMenu.SetActive(true);
    }
    public void Disconnect()
    {
        uiManager.ButtonClicked();
        uiManager.Disconnect();
    }
}
