using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreboardController : MonoBehaviour
{
    public static ScoreboardController instance;

    public GameObject scoreboard;
    public GameObject scoresHolder;

    public GameObject playerScoreInfoPrefab;

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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ReloadScoreboard();
            scoreboard.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            scoreboard.SetActive(false);
        }
    }

    public void ReloadScoreboard()
    {
        // Remove all old scoreboard infos
        foreach (RectTransform _playerScoreInfo in scoresHolder.transform)
        {
            if (_playerScoreInfo.gameObject != scoresHolder && _playerScoreInfo.name != "ScoreboardInfo")
            {
                Destroy(_playerScoreInfo.gameObject);
            }
        }

        // Initialize new playerscoreinfos
        foreach (int _newPlayerScoreInfoId in GameManager.instance.players.Keys)
        {
            ScoreboardPlayerInfoController _newPlayerScoreInfoController = Instantiate(playerScoreInfoPrefab, scoresHolder.transform).GetComponent<ScoreboardPlayerInfoController>();
            _newPlayerScoreInfoController.id = _newPlayerScoreInfoId;
            _newPlayerScoreInfoController.SetInfoTexts();
        }

        transform.SetAsLastSibling();
    }
}
