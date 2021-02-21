using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth;
    public int itemCount = 0;
    public int kills;
    public int deaths;

    [Space]
    public GameObject model;
    public Animator animator;
    public float runSpeed;
    public bool[] inputs;
    public float headXRotation;
    public Transform head;

    public float positionTransitionSpeed = 10f;
    public Vector3 transitionToPosition;
    public Vector3 oldPosition;

    public Transform usedTransform;

    public PlayerController playerController;
    public PlayerMovementController playerMovementController;
    public OtherPlayerWeaponController otherPlayerWeaponController;


    public Transform shootOrigin;
    public float shootDistance = 100f;

    public AudioSource audioSource;
    public AudioClip shootSound;
    public GameObject bulletLinePrefab;
    public GameObject bulletHolePrefab;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        if (playerMovementController != null)
        {
            usedTransform = playerMovementController.transform;
        }
        else
        {
            usedTransform = transform;
        }
    }

    private void Update()
    {
        if (playerController == null)
        {
            LerpMove();
        }
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (playerController != null)
        {
            float _mappedHealth = MapFloat(_health, 0, 1, 0, maxHealth);
            UIManager.instance.SetHealthShower(_mappedHealth);
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (playerMovementController != null)
        {
            playerMovementController.clientPredictedMovements.Clear();
            UIManager.instance.healthShowerHolder.SetActive(false);
        }
        model.SetActive(false);
    }

    public void UpdatePlayerDeathsAndKills(int _kills, int _deaths)
    {
        kills = _kills;
        deaths = _deaths;

        ScoreboardController.instance.ReloadScoreboard();
    }

    public void Respawn()
    {
        model.SetActive(true);
        if (playerController != null)
        {
            UIManager.instance.healthShowerHolder.SetActive(true);
        }
        SetHealth(maxHealth);

        // search all weapons in weaponsholder, then setup weapons
        Weapon[] _weapons = new Weapon[0];
        if (playerController != null)
        {
            _weapons = playerController.weaponsController.weaponsHolder.transform.GetComponentsInChildren<Weapon>(true);
        }
        else if (otherPlayerWeaponController != null)
        {
            _weapons = otherPlayerWeaponController.weaponsHolder.transform.GetComponentsInChildren<Weapon>(true);
        }
        foreach (Weapon _weapon in _weapons)
        {
            _weapon.reloading = false;
            _weapon.canFire = true;
        }
    }

    public void Shoot(Vector3 _fireOrigin, Vector3 _viewDirection, bool _thisClientsShot, int _weaponId, int _ammoInClip, int _extraAmmo)
    {
        if (_thisClientsShot == false)
        {
            if (GameManager.instance.weapons[_weaponId] != null)
            {
                audioSource.PlayOneShot(GameManager.instance.weapons[_weaponId].fireClip);
            }
        }
        GameManager.instance.weapons[_weaponId].currentClipAmmo = _ammoInClip;
        GameManager.instance.weapons[_weaponId].currentExtraAmmo = _extraAmmo;


        // Define ray
        Ray fireRay = new Ray(_fireOrigin, _viewDirection);

        // Store all raycast hits in shootdistance and choose the closest, accepted hit
        RaycastHit[] _hits = Physics.RaycastAll(fireRay, shootDistance);

        // If hit
        if (_hits.Length > 0)
        {
            RaycastHit _bestHit = _hits[0];

            foreach (RaycastHit _hit in _hits)
            {
                if (_hit.collider.CompareTag("Player"))
                {
                    // If hit own player
                    if (_hit.collider.GetComponent<PlayerManager>() == this)
                    {
                        return;
                    }
                }

                // If this hit is better than current best hit, set this hit to best hit
                if (_hit.distance < _bestHit.distance)
                {
                    _bestHit = _hit;
                }
                //Debug.Log(_hit.collider.gameObject.name);
            }

            if (_bestHit.collider.CompareTag("Player"))
            {
                // If hit own player
                if (_bestHit.collider.GetComponent<PlayerManager>() == this)
                {
                    Debug.Log("Only hit was the player itself");
                    return;
                }
            }


            LineRenderer _newBulletLine = Instantiate(bulletLinePrefab).GetComponent<LineRenderer>();
            if (_thisClientsShot == false)
            {
                if (otherPlayerWeaponController.weaponsEquiped[otherPlayerWeaponController.weaponUsed] != null)
                {
                    _newBulletLine.SetPosition(0, otherPlayerWeaponController.weaponsEquiped[otherPlayerWeaponController.weaponUsed].GetComponent<Weapon>().bulletSpawnPos.position);
                }
            }
            else
            {
                _newBulletLine.SetPosition(0,
                    playerController.weaponsController.weaponsEquiped[playerController.weaponsController.weaponUsed].GetComponent<Weapon>().bulletSpawnPos.position);
            }
            _newBulletLine.SetPosition(1, _bestHit.point);

            if (_bestHit.transform.tag != "Player" && _bestHit.transform.tag != "Enemy")
            {
                GameObject _newBulletHole = Instantiate(bulletHolePrefab, _bestHit.point + (_bestHit.normal * 0.025f), Quaternion.identity);
                //_newBulletHole.transform.parent = _hit.transform;
                _newBulletHole.transform.rotation = Quaternion.FromToRotation(Vector3.up, _bestHit.normal);
            }
        }
    }

    public void PickedUpWeapon(int _weaponId, int _weaponType, int _clipAmmo, int _extraAmmo)
    {
        if (playerController != null)
        {
            playerController.weaponsController.PickedUpWeapon(_weaponId, _weaponType, _clipAmmo, _extraAmmo);
        }
        else
        {
            otherPlayerWeaponController.PickedUpWeapon(_weaponId, _weaponType, _clipAmmo, _extraAmmo);
        }
    }

    public void DroppedWeapon(int _weaponId, int _weaponType)
    {
        if (playerController != null)
        {
            playerController.weaponsController.DroppedWeapon(_weaponId, _weaponType);
        }
        else
        {
            otherPlayerWeaponController.DroppedWeapon(_weaponId, _weaponType);
        }
    }

    public void LerpMove()
    {
        //if (playerController != null)
        //{ 
        //    return;
        //}

        Vector3 _lerpedPosition = Vector3.Lerp(usedTransform.transform.position, transitionToPosition, Time.deltaTime * positionTransitionSpeed);

        if (inputs.Length > 2)
        {
            Vector3 _moveDir = _lerpedPosition - usedTransform.transform.position;

            Vector3 _localVelocity = usedTransform.transform.InverseTransformDirection(_moveDir);
            float _forwardSpeed = _localVelocity.z;
            float _rightSpeed = _localVelocity.x;


            float _movementSpeedX = _rightSpeed / (Time.deltaTime * positionTransitionSpeed);
            float _movementSpeedY = _forwardSpeed / (Time.deltaTime * positionTransitionSpeed);
            animator.SetFloat("MoveX", Mathf.Round(_movementSpeedX * 10) / 10);
            animator.SetFloat("MoveY", Mathf.Round(_movementSpeedY * 10) / 10);
        }

        usedTransform.transform.position = _lerpedPosition;


        if (usedTransform != null)
        {
            return;
        }

        // Lerp move head
        head.localEulerAngles = new Vector2(headXRotation, head.localEulerAngles.y);
    }

    public void SetPosition(Vector3 _toPosition)
    {
        if (playerMovementController != null)
        {
            playerMovementController.clientPredictedMovements.Clear();
            playerMovementController.movementRequestsInputs.Clear();
            playerMovementController.movementRequestsRotations.Clear();
            playerMovementController.serverPositionObject.transform.position = _toPosition;
        }
        usedTransform.position = _toPosition;
    }

    private float MapFloat(float _value, float _minA, float _maxA, float _minB, float _maxB)
    {
        float _mappedFloat;

        float _aDifference = _maxA - _minA;
        float _bDifference = _maxB - _minB;
        float _aBMultiplier = _bDifference / _aDifference;

        _mappedFloat = _value / _aBMultiplier;
        _mappedFloat += _minB;

        return _mappedFloat;
    }
}
