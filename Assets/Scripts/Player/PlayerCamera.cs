using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    [Header("Look Parameters")]
    [SerializeField] private float mouseSensitivity = 20f;
    private Vector2 mouseLook;
    private float rotationX = 0;
    private float rotationY = 0;

    [Header("Move Parameters")]
    [SerializeField] private float moveSmooth = 0.1f;
    [SerializeField] private Vector3 forwardMovePoint;
    private Vector3 initialPosition;
    private Vector3 movePointPosition;
    private bool isMoving = false;

    [Header("Interact Parameters")]
    [SerializeField] private Vector3 interactionRayPoint;
    [SerializeField] private float interactionDistance = 1.5f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private Interactable currentInteractable;

    private Transform playerBody;
    private InputMaster controls;
    private GameManager gameManager;

    private void Awake()
    {
        playerBody = transform.parent;
        playerCamera = GetComponent<Camera>();
        initialPosition = transform.position;
    }

    private void Start()
    {
        gameManager = GameManager.instance;
        controls = transform.parent.GetComponent<InputManager>().INPUT;
        controls.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        transform.eulerAngles = new Vector3(0f, 0f, 0f);
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        HandleMouseLook();
        HandleBodyMove();

        HandleInteractionCheck();
        HandleInteractionInput();

        if(isMoving)
        {
            playerBody.position = Vector3.Lerp(playerBody.position, movePointPosition, moveSmooth);
            if(Vector3.Distance(playerBody.position, movePointPosition) < 0.1f)
            {
                playerBody.position = movePointPosition;
                isMoving = false;
            }
        }
    }

    private void HandleMouseLook()
    {
        mouseLook = controls.Player.Look.ReadValue<Vector2>();

        float mouseX = mouseLook.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseLook.y * mouseSensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        rotationY += mouseX;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f);

        playerBody.rotation = Quaternion.Euler(0, rotationY, 0);
    }

    private void HandleBodyMove()
    {
        if(controls.Player.Jump.WasPerformedThisFrame())
        {
            movePointPosition = forwardMovePoint;
            isMoving = true;
        }

        if (controls.Player.Jump.WasPerformedThisFrame() && Vector3.Distance(playerBody.position, movePointPosition) < 0.1f)
        {
            movePointPosition = initialPosition;
            isMoving = true;
        }
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
        if (controls.Player.Interact.WasPressedThisFrame() && currentInteractable != null && gameManager.isPlayerTurn)
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
