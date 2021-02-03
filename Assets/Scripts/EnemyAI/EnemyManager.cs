using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public int id;
    public float health;
    public float maxHealth = 100f;

    public float positionTransitionSpeed = 10f;
    public Vector3 transitionToPosition;


    public Transform shootOrigin;
    public float shootDistance = 100f;

    public AudioSource audioSource;
    public AudioClip shootSound;
    public GameObject bulletLinePrefab;
    public GameObject bulletHolePrefab;

    public void Initialize(int _id)
    {
        id = _id;
        health = maxHealth;
    }

    private void Update()
    {
        LerpMove(transitionToPosition);
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0)
        {
            GameManager.instance.enemies.Remove(id);
            Destroy(gameObject);
        }
    }

    public void LerpMove(Vector3 _toPosition)
    {
        transitionToPosition = _toPosition;
        transform.position = Vector3.Lerp(transform.position, _toPosition, Time.deltaTime * positionTransitionSpeed);
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
}
