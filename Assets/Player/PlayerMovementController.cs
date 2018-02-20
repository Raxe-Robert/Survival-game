﻿using UnityEngine;
using System.Collections;

public class PlayerMovementController : Photon.MonoBehaviour
{
    Rigidbody rigidbodyComponent;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float mouseDeadZoneFromPlayer;

    private PlayerCameraController cameraController = null;

    // Use this for initialization
    void Start()
    {
        rigidbodyComponent = GetComponent<Rigidbody>();
        cameraController = GetComponent<PlayerCameraController>();

        if (photonView.isMine)
        {
            if (cameraController != null)
            {
                cameraController.StartFollowing();
            }
            else
            {
                Debug.LogError("Missing CameraController Component on playerPrefab.");
            }
        }

    }
    
    private void Update()
    {
        if (!photonView.isMine) return;

        RotatePlayer();
    }
    
    void FixedUpdate()
    {
        if (!photonView.isMine) return;
        
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            MovePlayer(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

    }

    private void RotatePlayer()
    {
        if (cameraController.CameraReference == null)
            return;

        Ray ray = cameraController.CameraReference.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);

        Vector3 lookPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);

        if (Vector3.Distance(transform.position, lookPos) > mouseDeadZoneFromPlayer)
        {
            transform.LookAt(lookPos);
        }
    }

    /// <summary>
    /// Moves the player based on horizontal en vertical input axis.
    /// </summary>
    private void MovePlayer(float horizontal, float vertical)
    {
        Vector3 movement = new Vector3(horizontal, 0.0f, vertical);

        if (horizontal != 0 && vertical != 0)
            rigidbodyComponent.AddForce(movement * movementSpeed * 0.7071f);
        else
            rigidbodyComponent.AddForce(movement * movementSpeed);
    }
}
