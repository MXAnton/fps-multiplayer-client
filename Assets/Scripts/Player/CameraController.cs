using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FieldOfViewState
{
    normal,
    running
}

public class CameraController : MonoBehaviour
{
    private Camera camera;

    public PlayerManager player;
    public float sensivity = 100f;
    public float clampAngle = 85f;

    private float verticalRotation;
    private float horizontalRotation;

    private Vector3 lerpPosition;
    public float smoothSpeed = 0.1f;

    private void Start()
    {
        camera = GetComponent<Camera>();

        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = player.transform.eulerAngles.y;
    }

    private void Update()
    {
        LerpMove();

        sensivity = Settings.sensivity;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Look();
        }

        Debug.DrawRay(transform.position, transform.forward * 2, Color.red);
    }

    private void Look()
    {
        float _mouseVertical = -Input.GetAxis("Mouse Y");
        float _mouseHorizontal = Input.GetAxis("Mouse X");

        verticalRotation += _mouseVertical * sensivity * Time.deltaTime * 100;
        horizontalRotation += _mouseHorizontal * sensivity * Time.deltaTime * 100;

        verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

        transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
        player.transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
    }

    private void LerpMove()
    {
        lerpPosition = Vector3.Lerp(transform.position, player.transform.position + player.GetComponent<PlayerController>().camPos, smoothSpeed);
        transform.position = lerpPosition;
    }

    public void SetFieldOfView(FieldOfViewState _state)
    {
        switch (_state)
        {
            case FieldOfViewState.normal:
                camera.fieldOfView = 60;
                break;
            case FieldOfViewState.running:
                camera.fieldOfView = 65;
                break;
            default:
                camera.fieldOfView = 60;
                break;
        }
    }
}
