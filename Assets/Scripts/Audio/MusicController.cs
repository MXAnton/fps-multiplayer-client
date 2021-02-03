using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void SetVolume(float _newVolume)
    {
        if (_newVolume < 0.1f)
        {
            audioSource.Stop();
        }
        else if (audioSource.volume < 0.1f)
        {
            audioSource.Play();
        }
        audioSource.volume = _newVolume;
    }
}
