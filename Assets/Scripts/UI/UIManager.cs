using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    AudioSource audioSource;
    public AudioClip buttonClickedSound;

    public GameObject escapeMenuPrefab;
    private GameObject escapeMenu;
    public bool escapeMenuUp = false;

    [Header("Kills Info")]
    public GameObject killsInfosHolder;
    public GameObject killInfoPrefab;

    [Header("Others")]
    public GameObject hitDamageTextPrefab;
    public GameObject hitMarkPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleEscapeMenu();
        }
    }

    public void ToggleEscapeMenu()
    {
        if (escapeMenuUp)
        {
            escapeMenuUp = false;
            Destroy(escapeMenu);
        }
        else
        {
            escapeMenuUp = true;
            escapeMenu = Instantiate(escapeMenuPrefab, transform);
            escapeMenu.GetComponent<EscapeMenu>().uiManager = this;
        }
        ToggleCursorMode(!escapeMenuUp);

        //escapeMenu.SetActive(!escapeMenu.activeSelf);
        //ToggleCursorMode(!escapeMenu.activeSelf);
    }
    public void ToggleCursorMode(bool _lock)
    {
        Settings.cursorLocked = _lock;

        Cursor.visible = !_lock;
        if (_lock)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
        //Cursor.visible = !Cursor.visible;

        //if (Cursor.lockState == CursorLockMode.None)
        //{
        //    Cursor.lockState = CursorLockMode.Locked;
        //}
        //else
        //{
        //    Cursor.lockState = CursorLockMode.None;
        //}
    }

    public void ButtonClicked()
    {
        audioSource.PlayOneShot(buttonClickedSound);
    }

    public void ShowHitDamage(Vector3 _hitPoint, float _hitDamage)
    {
        TextMesh _newHitDamageText = Instantiate(hitDamageTextPrefab, _hitPoint, Quaternion.identity).GetComponent<TextMesh>();
        _newHitDamageText.text = "" + _hitDamage;

        GameObject newHitMark = Instantiate(hitMarkPrefab, transform);
        newHitMark.transform.localEulerAngles = new Vector3(0, 0, Random.Range(-40, 40));
    }

    public void PlayerKilled(string _killerName, string _killedName)
    {
        KillInfo _newKillInfo = Instantiate(killInfoPrefab, killsInfosHolder.transform).GetComponent<KillInfo>();

        _newKillInfo.SetInfoTexts(_killerName, _killedName);
    }

    public void Disconnect()
    {
        Client.instance.Disconnect();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
