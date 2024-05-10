using System;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    [Header("Look Parameters")]
    [SerializeField] private float mouseSensitivity = 20f;
    private Vector2 mouseLook;
    private float rotationX = 0;
    private float rotationY = 0;

    [Header("Interact Parameters")]
    [SerializeField] private Vector3 interactionRayPoint;
    [SerializeField] private float interactionDistance = 1.5f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private Interactable currentInteractable;

    private InputMaster controls;
    private Transform playerBody;

    private void Awake()
    {
        controls = transform.parent.GetComponent<InputManager>().INPUT;
        playerBody = transform.parent;
        playerCamera = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        HandleMouseLook();
        HandleInteractionCheck();
        HandleInteractionInput();
    }

    private void HandleMouseLook()
    {
        mouseLook = controls.Player.Look.ReadValue<Vector2>();

        float mouseX = mouseLook.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseLook.y * mouseSensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        rotationY += mouseX;
        rotationY = Mathf.Clamp(rotationY, -80f, 80f);

        playerBody.rotation = Quaternion.Euler(0, rotationY, 0);
    }


    private void HandleInteractionCheck()
    {
        if (currentInteractable != null && !currentInteractable.canInteract)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }

        if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Interactable") && (currentInteractable == null || hit.collider.gameObject.GetInstanceID() != currentInteractable?.GetInstanceID()))
            {
                Interactable newInteractable = hit.collider.GetComponent<Interactable>();
                if (newInteractable != null && newInteractable != currentInteractable && newInteractable.canInteract)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnLoseFocus();
                    }
                    currentInteractable = newInteractable;
                    currentInteractable.OnFocus();
                }
            }
        }
        else if (currentInteractable)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

    private void HandleInteractionInput()
    {
        if (controls.Player.Interact.WasPressedThisFrame() && currentInteractable != null)
        {
            if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
            {
                currentInteractable.OnInteract();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (currentInteractable != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentInteractable.transform.position, 0.5f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(playerCamera.ViewportPointToRay(interactionRayPoint).origin, playerCamera.ViewportPointToRay(interactionRayPoint).direction * interactionDistance);
    }
}
