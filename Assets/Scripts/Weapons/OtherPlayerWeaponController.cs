using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayerWeaponController : MonoBehaviour
{
    public PlayerManager playerManager;

    public GameObject weaponsHolder;

    public GameObject[] weaponsEquiped = new GameObject[3]; // 0 = primary, 1 = secondary, 2 = melee
    public int weaponUsed = 0; // 0 = primary, 1 = secondary, 2 = melee

    [Header("Grenade Vars")]
    public int grenadeCount;
    public int maxGrenadeCount = 3;
    //public Transform grenadeThrowOrigin;

    public void PickedUpWeapon(int _whichWeapon, int _weaponType, int _clipAmmo, int _extraAmmo)
    {
        GameObject _pickedWeapon = GameManager.instance.weapons[_whichWeapon].gameObject;

        _pickedWeapon.GetComponent<WeaponTransform>().enabled = false;

        _pickedWeapon.transform.parent = weaponsHolder.transform;
        _pickedWeapon.transform.localPosition = Vector3.zero;
        _pickedWeapon.transform.localEulerAngles = Vector3.zero;

        weaponsEquiped[_weaponType] = _pickedWeapon;

        _pickedWeapon.GetComponent<Weapon>().currentClipAmmo = _clipAmmo;
        _pickedWeapon.GetComponent<Weapon>().currentExtraAmmo = _extraAmmo;

        //_pickedWeapon.GetComponent<Weapon>().enabled = true;
    }

    public void DroppedWeapon(int _weaponId, int _weaponType)
    {
        GameObject _droppedWeapon = GameManager.instance.weapons[_weaponId].gameObject;

        _droppedWeapon.GetComponent<Weapon>().Dropped();

        weaponsEquiped[_weaponType] = null;
    }
    public void DroppedWeapon(int _weaponId)
    {
        GameObject _droppedWeapon = GameManager.instance.weapons[_weaponId].gameObject;

        _droppedWeapon.GetComponent<Weapon>().Dropped();
    }

    public void UpdateWeaponUsed()
    {
        switch (weaponUsed)
        {
            case 0:
                if (weaponsEquiped[0] != null)
                {
                    weaponsEquiped[0].SetActive(true);
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
                }
                break;
        }
    }
}
