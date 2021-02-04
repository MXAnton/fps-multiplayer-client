using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextAnimation : MonoBehaviour
{
    public TextMeshProUGUI text;

    public string[] textStates;
    private int currentState = 0;
    public float animationDelay = 0.4f;

    private void Start()
    {
        StartCoroutine(NextState());
    }

    private IEnumerator NextState()
    {
        yield return new WaitForSeconds(animationDelay);

        text.text = textStates[currentState];
        currentState++;
        if (currentState >= textStates.Length)
        {
            currentState = 0;
        }

        StartCoroutine(NextState());
    }
}
