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

    public PlayerController playerController;


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
    }

    private void Update()
    {
        LerpMove(transitionToPosition);
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        if (playerController != null)
        {
            //playerController.controller.enabled = false;
            playerController.clientPredictedMovements.Clear();
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
        //if (playerController != null)
        //{
        //    playerController.controller.enabled = true;
        //}
        SetHealth(maxHealth);
    }

    public void Shoot(Vector3 _viewDirection)
    {
        audioSource.PlayOneShot(shootSound);

        if (Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, shootDistance))
        {
            LineRenderer _newBulletLine = Instantiate(bulletLinePrefab).GetComponent<LineRenderer>();
            _newBulletLine.SetPosition(0, shootOrigin.position);
            _newBulletLine.SetPosition(1, _hit.point);
            //_newBulletLine.transform.parent = transform;

            if (_hit.transform.tag != "Player" && _hit.transform.tag != "Enemy")
            {
                GameObject _newBulletHole = Instantiate(bulletHolePrefab, _hit.point + (_hit.normal * 0.025f), Quaternion.identity);
                //_newBulletHole.transform.parent = _hit.transform;
                _newBulletHole.transform.rotation = Quaternion.FromToRotation(Vector3.up, _hit.normal);
            }
        }
    }

    public void LerpMove(Vector3 _toPosition)
    {
        if (playerController != null)
        { 
            return;
        }


        transitionToPosition = _toPosition;

        Vector3 _lerpedPosition =  Vector3.Lerp(transform.position, _toPosition, Time.deltaTime * positionTransitionSpeed);


        if (inputs.Length > 2)
        {
            Vector3 _moveDir = _lerpedPosition - transform.position;

            Vector3 _localVelocity = transform.InverseTransformDirection(_moveDir);
            float _forwardSpeed = _localVelocity.z;
            float _rightSpeed = _localVelocity.x;


            float _movementSpeedX = _rightSpeed / (Time.deltaTime * positionTransitionSpeed);
            float _movementSpeedY = _forwardSpeed / (Time.deltaTime * positionTransitionSpeed);
            animator.SetFloat("MoveX", Mathf.Round(_movementSpeedX * 10) / 10);
            animator.SetFloat("MoveY", Mathf.Round(_movementSpeedY * 10) / 10);
        }

        transform.position = _lerpedPosition;


        // Lerp move head
        head.localEulerAngles = new Vector2(headXRotation, head.localEulerAngles.y);
    }

    public void SetPosition(Vector3 _toPosition)
    {
        if (playerController != null)
        {
            playerController.clientPredictedMovements.Clear();
            playerController.movementRequestsInputs.Clear();
            playerController.movementRequestsRotations.Clear();

            //playerController.controller.enabled = false;
            //transform.position = _toPosition;
            //playerController.controller.enabled = true;
            playerController.serverPositionObject.transform.position = _toPosition;
        }
        //else
        //{
        //    transform.position = _toPosition;
        //}
        transform.position = _toPosition;
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
