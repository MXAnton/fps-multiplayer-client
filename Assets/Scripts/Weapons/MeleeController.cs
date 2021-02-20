using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeController : MonoBehaviour
{
    public WeaponsController weaponsController;

    [Header("Fire")]
    public bool canFire = true;

    [Header("Sound")]
    AudioSource audioSource;   
    public AudioClip normalFireClip;
    public AudioClip hardFireClip;

    [Header("Other")]
    public GameObject weaponIconPrefab;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        canFire = true;
    }

    public void NormalHit()
    {
        if (canFire == false)
        {
            return;
        }

        canFire = false;

        audioSource.PlayOneShot(normalFireClip);
    }

    public void HardHit()
    {
        if (canFire == false)
        {
            return;
        }

        canFire = false;

        audioSource.PlayOneShot(hardFireClip);
    }
}
