using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public int id;
    public GameObject explosionPrefab;

    public float positionTransitionSpeed = 10f;
    public Vector3 transitionToPosition;

    public void Initialize(int _id)
    {
        id = _id;
    }

    private void Update()
    {
        LerpMove(transitionToPosition);
    }

    public void Explode(Vector3 _position)
    {
        transform.position = _position;
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        GameManager.instance.projectiles.Remove(id);
        Destroy(gameObject);
    }

    public void LerpMove(Vector3 _toPosition)
    {
        transitionToPosition = _toPosition;
        transform.position = Vector3.Lerp(transform.position, _toPosition, Time.deltaTime * positionTransitionSpeed);
    }
}
