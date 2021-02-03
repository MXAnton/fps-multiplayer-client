using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreboardPlayerInfoController : MonoBehaviour
{
    public int id;

    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI deathsText;

    public void SetInfoTexts()
    {
        usernameText.text = GameManager.instance.players[id].username;
        killsText.text = "" + GameManager.instance.players[id].kills;
        deathsText.text = "" + GameManager.instance.players[id].deaths;
    }
}
