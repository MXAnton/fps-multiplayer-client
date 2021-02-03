using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceController : MonoBehaviour
{
    private void Start()
    {
        AudioController.instance.audioSources.Add(GetComponent<AudioSource>());
    }
}
