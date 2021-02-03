using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardToPlayer : MonoBehaviour
{
    private Vector3 playerPos;

    private void Update()
    {
        playerPos = GameObject.FindWithTag("Player").transform.position;

        Transform newLookRotation = transform;
        newLookRotation.LookAt(new Vector3(playerPos.x, playerPos.y, playerPos.z));
        newLookRotation.localEulerAngles = new Vector2(-newLookRotation.localEulerAngles.x, newLookRotation.localEulerAngles.y - 180); // flip the text

        transform.localEulerAngles = newLookRotation.localEulerAngles;
    }
}
