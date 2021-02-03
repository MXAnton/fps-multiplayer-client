using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;

    public List<AudioSource> audioSources = new List<AudioSource>();

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

    private void Start()
    {
        SetMainVolume(Settings.mainVolume);
    }

    public void SetMainVolume(float _newVolume)
    {
        foreach (AudioSource _audioSource in audioSources)
        {
            if (_audioSource == null)
            {
                audioSources.Remove(_audioSource);
                return;
            }
            _audioSource.volume = _newVolume;
        }
    }
}
