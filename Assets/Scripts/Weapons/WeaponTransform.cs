using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTransform : MonoBehaviour
{
    public Vector3 lerpToPosition;
    public Vector3 lerpToRotation;

    public float lerpSpeed = 500;

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, lerpToPosition, lerpSpeed * Time.deltaTime);
        transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, lerpToRotation, lerpSpeed * Time.deltaTime);
    }
}
