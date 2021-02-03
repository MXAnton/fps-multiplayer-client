using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public int spawnerId;
    public bool hasItem;
    public MeshRenderer itemModel;

    public Animator animator;

    private Vector3 basePosition;

    public void Initialize(int _spawnerId, bool _hasItem)
    {
        spawnerId = _spawnerId;
        hasItem = _hasItem;
        itemModel.enabled = _hasItem;

        basePosition = transform.position;
    }

    public void ItemSpawned()
    {
        hasItem = true;
        itemModel.enabled = true;
        animator.enabled = true;
    }

    public void ItemPickedUp()
    {
        hasItem = false;
        itemModel.enabled = false;
        animator.enabled = false;
    }
}
