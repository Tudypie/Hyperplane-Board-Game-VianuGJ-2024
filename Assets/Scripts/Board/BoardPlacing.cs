using UnityEngine;

public class BoardPlacing : MonoBehaviour
{
    public bool isPlacing = false;

    [HideInInspector]
    public Transform pieceSelectionTransform;
    private Piece pieceSelection;
    private Vector3 initialPiecePosition;
    private Quaternion initialPieceRotation;

    private InputMaster controls;

    public static BoardPlacing instance;

    private void Awake()
    {
        instance = this;
        controls = InputManager.instance.INPUT;
    }

    private void Update()
    {
        if (isPlacing)
        {
            if (pieceSelection.canBeRotated && controls.Player.Rotate.WasPressedThisFrame())
            {
                pieceSelectionTransform.rotation = Quaternion.Euler(pieceSelectionTransform.rotation.eulerAngles.x,
                    pieceSelectionTransform.rotation.eulerAngles.y + 45,
                    pieceSelectionTransform.rotation.eulerAngles.z);
            }

            if (controls.UI.RightClick.WasPressedThisFrame())
            {
                isPlacing = false;
                pieceSelectionTransform.position = initialPiecePosition;
                pieceSelectionTransform.rotation = initialPieceRotation;
                pieceSelectionTransform.GetComponent<Piece>().SetCanInteract(true);
            }
        }
    }

    public void StartPlacing(Piece piece)
    {
        isPlacing = true;

        pieceSelection = piece;
        pieceSelectionTransform = piece.gameObject.transform;
        initialPiecePosition = pieceSelectionTransform.position;
        initialPieceRotation = pieceSelectionTransform.rotation;
        pieceSelection.SetCanInteract(false);
    }

    public void Place(BoardTile boardTile)
    {
        isPlacing = false;

        boardTile.occupied = true;
        pieceSelection.placedOnBoard = true;
        pieceSelection.SetCanInteract(true);
        pieceSelectionTransform = null;
    }
}
