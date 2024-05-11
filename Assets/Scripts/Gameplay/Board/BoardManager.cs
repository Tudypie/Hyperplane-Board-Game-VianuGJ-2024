using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public List<BoardTile[]> boardTile = new List<BoardTile[]>();
    public int boardChildCount;

    [Header("Placing Parameters")]
    public bool isPlacing = false;

    public Piece pieceSelection;
    public Transform pieceSelectionTransform;

    private Vector3 initialPiecePosition;
    private Quaternion initialPieceRotation;

    private InputMaster controls;

    public static BoardManager instance;

    private void Awake()
    {
        instance = this;

        for (int i = 0; i < 8; i++)
        {
            BoardTile[] tiles = new BoardTile[5];
            for (int j = 0; j < 5; j++)
            {
                //Debug.Log(transform.GetChild(boardChildCount).name);
                tiles[j] = transform.GetChild(boardChildCount).GetComponent<BoardTile>();
                boardChildCount++;
            }
            boardTile.Add(tiles);
        }
    }

    private void Start()
    {
        controls = InputManager.instance.INPUT;
    }

    private void Update()
    {
        HandlePiecePlacing();
    }

    private void HandlePiecePlacing()
    {
        if (isPlacing)
        {
            if (controls.UI.RightClick.WasPressedThisFrame())
            {
                isPlacing = false;
                pieceSelectionTransform.position = initialPiecePosition;
                pieceSelectionTransform.rotation = initialPieceRotation;
                pieceSelectionTransform.GetComponent<Piece>().SetCanInteract(true);
                ClearBoardMaterials();
            }

            /* Rotating piece
            if (pieceSelection.canBeRotated && controls.Player.Rotate.WasPressedThisFrame())
            {
                pieceSelectionTransform.rotation = Quaternion.Euler(pieceSelectionTransform.rotation.eulerAngles.x,
                pieceSelectionTransform.rotation.eulerAngles.y + 45,
                pieceSelectionTransform.rotation.eulerAngles.z);
            }
            */    
        }
    }

    public void StartPlacingPiece(Piece piece)
    {
        isPlacing = true;

        pieceSelection = piece;
        pieceSelectionTransform = piece.gameObject.transform;
        initialPiecePosition = pieceSelectionTransform.position;
        initialPieceRotation = pieceSelectionTransform.rotation;
        pieceSelection.SetCanInteract(false);
    }

    public void PlacePiece(BoardTile boardTile)
    {
        isPlacing = false;

        boardTile.occupied = true;
        pieceSelection.placedOnBoard = true;
        pieceSelection.SetCanInteract(true);
        pieceSelectionTransform = null;
    }

    public void ShowPrismRange(Prism prism)
    {
        ClearBoardMaterials();

        for (int i = 1; i <= prism.Height; i++)
        {
            if(prism.row + i < boardTile.Count)
                boardTile[prism.row+i][prism.col].ChangeMeshRenderer(true, prism.normalMaterial);

            if (prism.row + i < boardTile.Count && prism.col + i < boardTile[0].Length)
                boardTile[prism.row+i][prism.col + i].ChangeMeshRenderer(true, prism.normalMaterial);

            if(prism.row + i < boardTile.Count && prism.col - i >= 0)
                boardTile[prism.row + i][prism.col - i].ChangeMeshRenderer(true, prism.normalMaterial);
        }
    }

    public void ClearBoardMaterials()
    {
        for (int i = 0; i < boardTile.Count; i++)
        {
            for (int j = 0; j < boardTile[i].Length; j++)
            {
                boardTile[i][j].ChangeMeshRenderer(false, null);
            }
        }
    }
}
