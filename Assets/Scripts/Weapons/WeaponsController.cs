using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponsController : MonoBehaviour
{
    WeaponUI weaponUI;
    AudioSource audioSource;
    public PlayerController playerController;
    public PlayerManager playerManager;

    public GameObject crosshair;

    public GameObject weaponsHolder;

    public GameObject[] weaponsEquiped = new GameObject[3]; // 0 = primary, 1 = secondary, 2 = melee
    public int weaponUsed = 0; // 0 = primary, 1 = secondary, 2 = melee

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        crosshair = GameObject.FindWithTag("Crosshair");

        weaponUI = WeaponUI.instance;
        weaponUI.weaponsController = this;

        weaponUsed = 0;
        if (weaponsEquiped[0] != null)
        {
            weaponsEquiped[0].SetActive(true);

            weaponUI.currentWeaponScript = weaponsEquiped[0].GetComponent<Weapon>();
            weaponUI.SetWeaponIcon(weaponsEquiped[0].GetComponent<Weapon>().weaponIconPrefab);

            if (weaponsEquiped[1] != null)
            {
                weaponsEquiped[1].SetActive(false);
            }
        }
        else if (weaponsEquiped[1] != null)
        {
            weaponsEquiped[1].SetActive(true);
        }
        if (weaponsEquiped[2] != null)
        {
            weaponsEquiped[2].SetActive(false);
        }

        ChangeWeaponUsed();
    }

    void Update()
    {
        CheckChangeWeaponInput();

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Try pickup item
            ClientSend.PlayerTryPickUpWeapon(playerController.camTransform.forward);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            // Try drop item
            if (weaponsEquiped[weaponUsed] != null)
            {
                ClientSend.PlayerTryDropWeapon(weaponsEquiped[weaponUsed].GetComponent<Weapon>().id, weaponUsed, weaponsEquiped[weaponUsed].transform.position, weaponsEquiped[weaponUsed].transform.eulerAngles, playerController.camTransform.forward);
            }
        }
    }

    void CheckChangeWeaponInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Select primary
            weaponUsed = 0;
            ChangeWeaponUsed();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Select secondary
            weaponUsed = 1;
            ChangeWeaponUsed();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // Select melee
            weaponUsed = 2;
            ChangeWeaponUsed();
        }


        // If scrolling
        float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheelInput < 0) // If scrolling up
        {
            if (weaponUsed == 2)
            {
                weaponUsed = 0;
            }
            else
            {
                weaponUsed++;
            }

            ChangeWeaponUsed();
        }
        else if (scrollWheelInput > 0) // If scrolling down
        {
            if (weaponUsed == 0)
            {
                weaponUsed = 2;
            }
            else
            {
                weaponUsed--;
            }

            ChangeWeaponUsed();
        }
    }
    void ChangeWeaponUsed()
    {
        // Send usedweapon data to server
        ClientSend.PlayerWeaponUsed(weaponUsed);

        switch (weaponUsed)
        {
            case 0:
                if (weaponsEquiped[0] != null)
                {
                    weaponsEquiped[0].SetActive(true);

                    weaponUI.currentWeaponScript = weaponsEquiped[0].GetComponent<Weapon>();
                    weaponUI.SetWeaponIcon(weaponsEquiped[0].GetComponent<Weapon>().weaponIconPrefab);
                }
                else
                {
                    weaponUI.currentWeaponScript = null;
                    weaponUI.SetWeaponIcon(null);
                }
                if (weaponsEquiped[1] != null)
                {
                    weaponsEquiped[1].SetActive(false);
                }
                if (weaponsEquiped[2] != null)
                {
                    weaponsEquiped[2].SetActive(false);
                }
                break;
            case 1:
                if (weaponsEquiped[0] != null)
                {
                    weaponsEquiped[0].SetActive(false);
                }
                if (weaponsEquiped[1] != null)
                {
                    weaponsEquiped[1].SetActive(true);

                    weaponUI.currentWeaponScript = weaponsEquiped[1].GetComponent<Weapon>();
                    weaponUI.SetWeaponIcon(weaponsEquiped[1].GetComponent<Weapon>().weaponIconPrefab);
                }
                else
                {
                    weaponUI.currentWeaponScript = null;
                    weaponUI.SetWeaponIcon(null);
                }
                if (weaponsEquiped[2] != null)
                {
                    weaponsEquiped[2].SetActive(false);
                }
                break;
            default:
                if (weaponsEquiped[0] != null)
                {
                    weaponsEquiped[0].SetActive(false);
                }
                if (weaponsEquiped[1] != null)
                {
                    weaponsEquiped[1].SetActive(false);
                }
                if (weaponsEquiped[2] != null)
                {
                    weaponsEquiped[2].SetActive(true);

                    weaponUI.currentWeaponScript = null;
                    weaponUI.SetWeaponIcon(weaponsEquiped[2].GetComponent<MeleeController>().weaponIconPrefab);
                }
                else
                {
                    weaponUI.currentWeaponScript = null;
                    weaponUI.SetWeaponIcon(null);
                }
                break;
        }
    }

    public void PickedUpWeapon(int _weaponId, int _weaponType, int _clipAmmo, int _extraAmmo)
    {
        GameObject _pickedWeapon = GameManager.instance.weapons[_weaponId].gameObject;

        _pickedWeapon.GetComponent<WeaponTransform>().enabled = false;

        _pickedWeapon.transform.parent = weaponsHolder.transform;
        _pickedWeapon.transform.localPosition = Vector3.zero;
        _pickedWeapon.transform.localEulerAngles = Vector3.zero;

        Debug.Log("Server ammo: " + _clipAmmo);
        Debug.Log("Client ammo: " + _pickedWeapon.GetComponent<Weapon>().currentClipAmmo);

        weaponsEquiped[_weaponType] = _pickedWeapon;

        _pickedWeapon.GetComponent<Weapon>().weaponsController = this;
        _pickedWeapon.GetComponent<Weapon>().enabled = true;

        _pickedWeapon.GetComponent<Weapon>().currentClipAmmo = _clipAmmo;
        _pickedWeapon.GetComponent<Weapon>().currentExtraAmmo = _extraAmmo;

        _pickedWeapon.GetComponent<Weapon>().canFire = true;
        _pickedWeapon.GetComponent<Weapon>().reloading = false;

        ChangeWeaponUsed();

    }

    public void DroppedWeapon(int _weaponId, int _weaponType)
    {
        GameObject _droppedWeapon = GameManager.instance.weapons[_weaponId].gameObject;

        _droppedWeapon.GetComponent<Weapon>().Dropped();

        weaponsEquiped[_weaponType] = null;

        ChangeWeaponUsed();
    }
}
