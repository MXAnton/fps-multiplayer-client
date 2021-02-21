using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeSpawner : MonoBehaviour
{
    public int spawnerId;
    public bool hasGrenade;
    public MeshRenderer itemModel;

    public Animator animator;

    private Vector3 basePosition;

    public void Initialize(int _spawnerId, bool _hasGrenade)
    {
        spawnerId = _spawnerId;
        hasGrenade = _hasGrenade;
        itemModel.enabled = _hasGrenade;

        basePosition = transform.position;
    }

    public void GrenadeSpawned()
    {
        hasGrenade = true;
        itemModel.enabled = true;
        animator.enabled = true;
    }

    public void GrenadePickedUp()
    {
        hasGrenade = false;
        itemModel.enabled = false;
        animator.enabled = false;
    }
}
