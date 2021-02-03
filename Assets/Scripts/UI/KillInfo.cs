using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KillInfo : MonoBehaviour
{
    public TextMeshProUGUI killerUsernameText;
    public TextMeshProUGUI killedUsernameText;

    public void SetInfoTexts(string _killerName, string _killedName)
    {
        killerUsernameText.text = _killerName;
        killedUsernameText.text = _killedName;
    }
}
