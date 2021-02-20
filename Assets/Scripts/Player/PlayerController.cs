using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public UIManager uiManager;
    public PlayerManager playerManager;
    public WeaponsController weaponsController;

    //public GameObject cameraPrefab;
    public Transform camTransform;
    public Vector3 camPos;

    private void Start()
    {
        uiManager = UIManager.instance;
        uiManager.ToggleCursorMode(true);

        //camTransform = Instantiate(cameraPrefab).GetComponent<Transform>();
        //camTransform.GetComponent<CameraController>().player = playerManager;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //{
        //    if (playerManager.health <= 0 || uiManager.escapeMenuUp)
        //    {
        //        return;
        //    }

        //    ClientSend.PlayerShoot(camTransform.forward);
        //    weaponsController.FireInput();
        //}
        //if (Input.GetKeyDown(KeyCode.Mouse1))
        //{
        //    if (playerManager.health <= 0 || uiManager.escapeMenuUp)
        //    {
        //        return;
        //    }

        //    ClientSend.PlayerThrowItem(camTransform.forward);
        //}
    }
}