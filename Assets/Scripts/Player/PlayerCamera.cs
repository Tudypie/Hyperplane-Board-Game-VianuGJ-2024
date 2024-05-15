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
    [SerializeField] private Vector3 firstPersonPoint;
    [SerializeField] private Vector3 firstPersonRotation;
    private Vector3 initialPosition;
    private Vector3 initialRotation;
    private Vector3 movePointPosition;
    private Vector3 movePointRotation;
    public bool isMoving = false;
    public bool isRotating = false;
    public bool isTopDown = false;

    [Header("Interact Parameters")]
    [SerializeField] private Vector3 interactionRayPoint;
    [SerializeField] private float interactionDistance = 1.5f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private Interactable currentInteractable;

    private Transform playerBody;
    private InputMaster controls;
    private GameManager gameManager;

    public static PlayerCamera instance { get; private set; }

    private void Awake()
    {
        instance = this;
        playerBody = transform.parent;
        playerCamera = GetComponent<Camera>();
        initialPosition = playerBody.position;
        initialRotation = transform.eulerAngles;
    }

    private void Start()
    {
        gameManager = GameManager.instance;
        controls = transform.parent.GetComponent<InputManager>().INPUT;
        controls.Enable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (!isTopDown)
        {
            HandleMouseLook();
            //HandleBodyMove();
            HandleInteractionCheck();
            HandleInteractionInput();
        }

        if(isMoving)
        {
            playerBody.position = Vector3.Lerp(playerBody.position, movePointPosition, moveSmooth);

            if(Vector3.Distance(playerBody.position, movePointPosition) < 0.1f)
            {
                playerBody.position = movePointPosition;
                isMoving = false;
            }
        }

        if(isRotating)
        {
            //transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, movePointRotation, moveSmooth);
            //if(Vector3.Distance(transform.eulerAngles, movePointRotation) < 0.1f)

                transform.eulerAngles = movePointRotation;
                isRotating = false;
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

    public void ChangeCameraPerspective(bool topDown)
    {
        isTopDown = topDown;
        movePointPosition = topDown ? initialPosition : firstPersonPoint;
        movePointRotation = topDown ? initialRotation : firstPersonRotation;
        isMoving = true;
        isRotating = true;

        UIManager.instance.ActivateCursor(!topDown);
        Cursor.visible = topDown;
        Cursor.lockState = topDown ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
