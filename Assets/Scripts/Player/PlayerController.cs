using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    UIManager uiManager;
    public PlayerManager playerManager;

    public GameObject cameraPrefab;
    public Transform camTransform;
    public Vector3 camPos;

    private void Start()
    {
        uiManager = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
        uiManager.ToggleCursorMode(true);

        camTransform = Instantiate(cameraPrefab).GetComponent<Transform>();
        camTransform.GetComponent<CameraController>().player = playerManager;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (playerManager.health <= 0 || uiManager.escapeMenuUp)
            {
                return;
            }

            ClientSend.PlayerShoot(camTransform.forward);

            playerManager.Shoot(camTransform.forward);
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (playerManager.health <= 0 || uiManager.escapeMenuUp)
            {
                return;
            }

            ClientSend.PlayerThrowItem(camTransform.forward);
        }
    }
}