using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float mouseSensivity = 100f;
    private Vector2 mouseLook;
    private float rotationX = 0;

    private InputMaster controls;
    private Transform playerBody;

    private void Awake()
    {
        controls = new InputMaster();
        playerBody = transform.parent;
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
    }

    private void HandleMouseLook()
    {
        mouseLook = controls.Player.Look.ReadValue<Vector2>();

        float mouseX = mouseLook.x * mouseSensivity * Time.deltaTime;
        float mouseY = mouseLook.y * mouseSensivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
