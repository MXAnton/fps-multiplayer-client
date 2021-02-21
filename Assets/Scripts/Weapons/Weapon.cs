using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    public enum FireModes { Semi, Burst, Auto };


    public int id;

    public WeaponTransform weaponTransform;
    public WeaponsController weaponsController;

    [Header("FireMode")]
    public int currentFireModeState;
    public FireModes currentFireMode;
    public FireModes[] enabledFireModes;
    public float semiFireRate = 0.5f;
    public float burstFireRate = 0.1f;
    public float autoFireRate = 0.2f;

    [Header("Fire")]
    public bool canFire = false;
    public float fireSpread = 0.2f;
    public float fireDistance = 100f;
    [Space]
    public LineRenderer lineRendererPrefab;
    public Transform bulletSpawnPos;

    [Header("Hit")]
    public GameObject[] bulletHolePrefabs;

    [Header("Ammo")]
    public int currentClipAmmo;
    public int maxClipAmmo = 30;
    [Space]
    public int currentExtraAmmo;
    public int maxExtraAmmo = 120;
    [Space]
    public float reloadTime = 1;
    public bool reloading = false;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip fireClip;
    public AudioClip emptyClipClip;
    public AudioClip reloadClip;

    [Header("Other")]
    public GameObject weaponIconPrefab;

    private void Start()
    {
        currentClipAmmo = maxClipAmmo;
        currentExtraAmmo = maxExtraAmmo;

        currentFireModeState = 0;
        currentFireMode = enabledFireModes[currentFireModeState];
    }

    private void Awake()
    {
        //Debug.Log("Weaponscript activated");
        canFire = true;
        reloading = false;
    }

    public void Initialize(int _id)
    {
        id = _id;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V) && enabledFireModes.Length > 1)
        {
            ChangeFireMode();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(weaponsController.playerManager.playerMovementController.transform.position, weaponsController.playerController.camTransform.forward, currentFireModeState);
            Fire();
        }
        else if (currentFireMode == FireModes.Auto && Input.GetKey(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(weaponsController.playerManager.playerMovementController.transform.position, weaponsController.playerController.camTransform.forward, currentFireModeState);
            Fire();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    public void Fire()
    {
        if (canFire == true)
        {
            canFire = false;
            switch (currentFireMode)
            {
                case FireModes.Semi:
                    StartCoroutine(SemiShoot());
                    break;
                case FireModes.Burst:
                    StartCoroutine(BurstShoot());
                    break;
                case FireModes.Auto:
                    StartCoroutine(AutoShoot());
                    break;
            }
        }
    }

    IEnumerator SemiShoot()
    {
        canFire = false;
        FireBullet();

        yield return new WaitForSeconds(semiFireRate);

        canFire = true;
    }
    IEnumerator BurstShoot()
    {
        canFire = false;

        FireBullet();
        yield return new WaitForSeconds(burstFireRate);

        if (currentClipAmmo > 0)
        {
            FireBullet();
            yield return new WaitForSeconds(burstFireRate);
        }
        if (currentClipAmmo > 0)
        {
            FireBullet();
        }

        yield return new WaitForSeconds(semiFireRate);

        canFire = true;
    }
    IEnumerator AutoShoot()
    {
        canFire = false;
        FireBullet();

        yield return new WaitForSeconds(autoFireRate);

        canFire = true;
    }

    void FireBullet()
    {
        if (weaponsController == null)
        {
            return;
        }

        if (reloading == true)
        {
            // Can't fire while reloading
            return;
        }

        if (currentClipAmmo <= 0)
        {
            audioSource.PlayOneShot(emptyClipClip);
            return;
        }

        audioSource.PlayOneShot(fireClip);


        // Add firespread
        //Vector3 _fireDirection = Camera.main.ScreenToWorldPoint(weaponsController.crosshair.transform.position) + transform.forward;
        //_fireDirection.x += UnityEngine.Random.Range(-fireSpread, fireSpread);
        //_fireDirection.y += UnityEngine.Random.Range(-fireSpread, fireSpread);
        //_fireDirection.z += UnityEngine.Random.Range(-fireSpread, fireSpread);

        //// Define ray
        //Ray fireRay = new Ray(weaponsController.playerManager.shootOrigin.position, weaponsController.playerController.camTransform.forward);
        //Physics.Raycast(fireRay, out RaycastHit _hit, fireDistance);


        //if (Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, shootDistance))
        //{
        //    if (_hit.transform.tag != "Player" && _hit.transform.tag != "Enemy")
        //    {
        //        GameObject _newBulletHole = Instantiate(bulletHolePrefab, _hit.point + (_hit.normal * 0.025f), Quaternion.identity);
        //        //_newBulletHole.transform.parent = _hit.transform;
        //        _newBulletHole.transform.rotation = Quaternion.FromToRotation(Vector3.up, _hit.normal);
        //    }
        //}


        //if (_hit.collider != null)
        //{
        //    // Hit
        //    // Show bullet path
        //    LineRenderer newLineRenderer = Instantiate(lineRendererPrefab);
        //    newLineRenderer.SetPosition(0, bulletSpawnPos.position);
        //    newLineRenderer.SetPosition(1, _hit.point);

        //    if (_hit.collider.tag != "Player")
        //    {
        //        // Show bullet hole
        //        int randomizedBulletHole = UnityEngine.Random.Range(0, bulletHolePrefabs.Length);
        //        GameObject newHole = Instantiate(bulletHolePrefabs[randomizedBulletHole], _hit.point + _hit.normal * 0.001f,
        //                                            Quaternion.LookRotation(_hit.normal) * bulletHolePrefabs[randomizedBulletHole].transform.rotation);
        //        newHole.transform.parent = _hit.transform;
        //    }
        //}
        //else
        //{
        //    // No hit, but show bullet path
        //    LineRenderer newLineRenderer = Instantiate(lineRendererPrefab);
        //    newLineRenderer.SetPosition(0, bulletSpawnPos.position);
        //    newLineRenderer.SetPosition(1, bulletSpawnPos.position + fireDistance * transform.forward);
        //}

        currentClipAmmo--;
    }

    void ChangeFireMode()
    {
        if (currentFireModeState == 0)
        {
            currentFireModeState = 1;
        }
        else if (currentFireModeState == 1 && enabledFireModes.Length == 3)
        {
            currentFireModeState = 2;
        }
        else
        {
            currentFireModeState = 0;
        }

        ClientSend.PlayerFireMode(currentFireModeState);
        currentFireMode = enabledFireModes[currentFireModeState];
    }
    private int FireModeToInt(FireModes _fireMode)
    {
        int _fireModeInt = 0;

        switch (_fireMode)
        {
            case FireModes.Auto:
                _fireModeInt = 2;
                break;
            case FireModes.Burst:
                _fireModeInt = 1;
                break;
            default: // Semi
                _fireModeInt = 0;
                break;
        }

        return _fireModeInt;
    }


    void Reload()
    {
        if (reloading == true)
        {
            // Already reloading
            return;
        }
        if (currentClipAmmo >= maxClipAmmo)
        {
            // Already full of ammo
            return;
        }

        if (currentExtraAmmo <= 0)
        {
            if (currentClipAmmo <= 0)
            {
                // Completely out of ammo
                audioSource.PlayOneShot(emptyClipClip);
            }
            // Out of extra ammo

            return;
        }


        // Reload
        reloading = true;
        audioSource.PlayOneShot(reloadClip);

        // Reload weapon on server
        ClientSend.PlayerReload(weaponsController.weaponUsed);
    }
    public void CompleteReload(int _ammoInClip, int _extraAmmo)
    {
        // Reload done
        currentClipAmmo = _ammoInClip;
        currentExtraAmmo = _extraAmmo;

        reloading = false;
    }

    public void Dropped()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        reloading = false;
        weaponsController = null;
        transform.parent = null;

        weaponTransform.enabled = true;

        gameObject.SetActive(true);

        this.enabled = false;
    }
}
