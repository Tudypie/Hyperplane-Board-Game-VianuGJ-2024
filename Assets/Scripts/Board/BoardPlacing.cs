using UnityEngine;

public class BoardPlacing : MonoBehaviour
{
    public bool isPlacing = false;

    public Transform pieceSelection;
    private Vector3 initialPiecePosition;

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
            if (controls.UI.RightClick.WasPressedThisFrame())
            {
                isPlacing = false;
                pieceSelection.position = initialPiecePosition;
                pieceSelection.GetComponent<Piece>().SetCanInteract(true);
            }
        }
    }

    public void StartPlacing(Piece piece)
    {
        isPlacing = true;
        pieceSelection = piece.gameObject.transform;
        initialPiecePosition = pieceSelection.position;
        pieceSelection.GetComponent<Piece>().SetCanInteract(false);
    }

    public void Place(BoardTile boardTile)
    {
        boardTile.occupied = true;
        pieceSelection.GetComponent<Piece>().placedOnBoard = true;
        isPlacing = false;
        pieceSelection = null;
    }
}
