using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject clientManagerPrefab;
    public GameObject clientManager;

    public TMP_InputField usernameField;
    public TMP_InputField ipField;

    AudioSource audioSource;
    public AudioClip buttonClickedSound;

    public MusicController musicController;

    [Header("Menus")]
    public GameObject main;
    public GameObject connect;
    public GameObject settings;
    
    [Header("Settings")]
    public Slider sensivitySlider;
    public TMP_InputField sensivityInputField;

    public Slider mainVolumeSlider;
    public TMP_InputField mainVolumeInputField;

    public Slider musicVolumeSlider;
    public TMP_InputField musicVolumeInputField;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (GameObject.FindWithTag("ClientManager"))
        {
            foreach (GameObject _clientManager in GameObject.FindGameObjectsWithTag("ClientManager"))
            {
                Destroy(_clientManager);
            }
            Client.instance = null;
        }
    }
    private void Start()
    {
        Settings.LoadSettings();

        sensivitySlider.value = Settings.sensivity;
        sensivityInputField.text = "" + Settings.sensivity;

        mainVolumeSlider.value = Settings.mainVolume;
        mainVolumeInputField.text = "" + Settings.mainVolume;

        musicVolumeSlider.value = Settings.musicVolume;
        musicVolumeInputField.text = "" + Settings.musicVolume;
    }

    #region main
    public void OnPlayClicked()
    {
        ButtonClicked();
        //main.SetActive(false);
        if (settings.GetComponent<ChildGameObject>().child.activeSelf)
        {
            settings.GetComponent<Animator>().SetTrigger("SlideOut");
        }
        if (!connect.GetComponent<ChildGameObject>().child.activeSelf)
        {
            connect.GetComponent<Animator>().SetTrigger("SlideIn");
        }
    }
    public void OnSettingsClicked()
    {
        ButtonClicked();
        //main.SetActive(false);
        if (connect.GetComponent<ChildGameObject>().child.activeSelf)
        {
            connect.GetComponent<Animator>().SetTrigger("SlideOut");
        }

        if (!settings.GetComponent<ChildGameObject>().child.activeSelf)
        {
            settings.GetComponent<Animator>().SetTrigger("SlideIn");
        }
    }
    public void OnQuitClicked()
    {
        ButtonClicked();
        Application.Quit();
    }
    #endregion

    #region connect
    public void OnConnectClicked()
    {
        ButtonClicked();
        ConnectToServer();
    }
    public void OnBackClicked()
    {
        ButtonClicked();
        connect.SetActive(false);
        main.SetActive(true);
    }
    #endregion

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

        musicController.SetVolume(Settings.musicVolume);
        audioSource.volume = Settings.mainVolume;
    }
    #endregion

    private void ButtonClicked()
    {
        audioSource.PlayOneShot(buttonClickedSound);
    }

    private void ConnectToServer()
    {
        //usernameField.interactable = false;
        //ipField.interactable = false;
        if (ipField.text.Length <= 2)
        {
            Debug.Log("Enter valid ip-address!");
            ipField.GetComponent<Animator>().SetTrigger("ScaleUpAndDown");
            return;
        }
        else if (usernameField.text.Length < 1)
        {
            Debug.Log("Enter username!");
            usernameField.GetComponent<Animator>().SetTrigger("ScaleUpAndDown");
            return;
        }


        clientManager = Instantiate(clientManagerPrefab);
        DontDestroyOnLoad(Client.instance.gameObject);

        Client.instance.ip = ipField.text;
        Client.instance.username = usernameField.text;
        Client.instance.ConnectToServer();

        SceneManager.LoadScene("Main");
    }
}
