﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    private Ray ray;
    private RaycastHit hit;
    private bool isMine;
    private float interactionTimeout;

    private void Start()
    {
        playerCamera = GetComponent<PlayerCameraController>().CameraReference;
        isMine = GetComponent<PhotonView>().isMine;
    }

    private void Update()
    {
        if (!isMine)
            return;

        if (interactionTimeout > 0)
            interactionTimeout -= Time.deltaTime;

        ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity) && !EventSystem.current.IsPointerOverGameObject())
        {
            var interactable = hit.transform.GetComponentInChildren<IInteractable>();
            if (interactable == null)
            {
                Tooltip.Instance.Hide();
            }
            else if(interactable != null)
            {
                if (Input.GetMouseButtonDown(0) && interactable.IsInteractable())
                {                    
                    if (interactable.GetType() == typeof(WorldResource))
                        InteractWithWorldResource(interactable);
                    else
                        interactable.Interact(gameObject.transform.position);
                    //TODO: What if monster?

                }

                if (interactable.TooltipText() != string.Empty)
                {
                    Tooltip.Instance.Show(interactable.TooltipText());
                }
            }
        }
    }

    private void InteractWithWorldResource(IInteractable interactable)
    {
        if (interactionTimeout <= 0)
        {
            WorldResource resource = interactable as WorldResource;
            EquipmentManager equipmentManager = GetComponent<EquipmentManager>();
            if (equipmentManager.HasToolEquipped(resource.requiredToolToHarvest))
            {
                interactionTimeout = 2;
                interactable.Interact(transform.position);
            }
            else
            {
                WorldNotificationsManager.Instance.ShowNotification(new WorldNotificationArgs(transform.position, "Wrong tool", 1), true);
            }
        }
        else
        {
            WorldNotificationsManager.Instance.ShowNotification(new WorldNotificationArgs(transform.position, "Not ready yet", 1), true);
        }
    }
}
