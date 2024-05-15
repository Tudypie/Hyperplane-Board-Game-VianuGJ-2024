using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    [Header("Rotatinon")]
    [SerializeField] private Transform cameraMiddlePoint;
    [SerializeField] private float horizontalMoveSpeed;
    [SerializeField] private float verticalMoveSpeed;
    private Vector3 rotation;

    [Header("Pickup")]
    public Card cardInHand;
    public bool isCardInHand;
    public bool lerpCard;
    [SerializeField] private float pickupSmooth;
    [SerializeField] private Transform pickupPoint;
    private Vector3 lerpPosition;
    private Quaternion lerpRotation;

    private Transform cardTransform;
    private Vector3 cardInitialPosition;
    private Quaternion cardInitialRotation;

    private InputMaster controls;
    private BoardManager boardManager;

    public static TopDownCamera instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        boardManager = BoardManager.instance;
        controls = transform.parent.GetComponent<InputManager>().INPUT;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (!boardManager.isPlacing && !boardManager.isAttacking)
            HandleCameraRotation();

        //HandleCameraZoom();
        HandleCameraPickup();
    }

    private void HandleCameraRotation()
    {
        float horizontalInput = controls.Player.RotateLeft.ReadValue<float>() - controls.Player.RotateRight.ReadValue<float>();
        float verticalInput = controls.Player.RotateDown.ReadValue<float>() - controls.Player.RotateUp.ReadValue<float>();

        float horizontalRotation = horizontalInput * horizontalMoveSpeed * Time.deltaTime;
        float verticalRotation = verticalInput * verticalMoveSpeed * Time.deltaTime;

        rotation.y += horizontalRotation;
        rotation.x -= verticalRotation;
        rotation.x = Mathf.Clamp(rotation.x, -40f, 30f);

        rotation.z = 0f;
        cameraMiddlePoint.localEulerAngles = rotation;
    }

    private void HandleCameraZoom()
    {
        Vector2 scroll = controls.UI.ScrollWheel.ReadValue<Vector2>();
        transform.parent.localPosition += new Vector3(0f, scroll.y, scroll.y);
    }

    private void HandleCameraPickup()
    {
        if (lerpCard)
        {
            if(cardTransform == null)
            {
                isCardInHand = !isCardInHand;
                lerpCard = false;
                return;
            }

            if (Vector3.Distance(cardTransform.position, lerpPosition) < 0.1f &&
            Quaternion.Angle(cardTransform.rotation, lerpRotation) < 0.1f)
            {
                cardTransform.position = lerpPosition;
                cardTransform.rotation = lerpRotation;
                isCardInHand = !isCardInHand;
                cardInHand.SetCanInteract(true);
                lerpCard = false;
                if (!isCardInHand)
                {
                    cardInHand = null;
                    cardTransform = null;
                }
                return;
            }
            cardTransform.position = Vector3.Lerp(cardTransform.position, lerpPosition, pickupSmooth);
            cardTransform.rotation = Quaternion.Slerp(cardTransform.rotation, lerpRotation, pickupSmooth);
        }

        if (isCardInHand)
            if (controls.UI.RightClick.WasPerformedThisFrame())
                DropCard();
    }

    public void DropCard()
    {
        cardInHand.SetCanInteract(false);
        lerpPosition = cardInitialPosition;
        lerpRotation = cardInitialRotation;
        lerpCard = true;
    }

    public void PickupCard(Card card, Transform cardTransform)
    {
        if (lerpCard || isCardInHand) return;
        cardInHand = card;
        cardInHand.SetCanInteract(false);
        this.cardTransform = cardTransform;
        cardInitialPosition = cardTransform.position;
        cardInitialRotation = cardTransform.rotation;
        lerpPosition = pickupPoint.position;
        lerpRotation = pickupPoint.rotation;
        lerpCard = true;
    }
}
