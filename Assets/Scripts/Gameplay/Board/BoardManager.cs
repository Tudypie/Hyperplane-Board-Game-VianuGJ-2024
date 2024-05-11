using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public List<BoardTile[]> boardTile = new List<BoardTile[]>();
    private int boardChildCount;

    [Header("Placing Parameters")]
    public bool isPlacing = false;
    public Piece pieceSelection;
    public Transform pieceSelectionTransform;
    private Vector3 initialPiecePosition;
    private Quaternion initialPieceRotation;

    public event Action OnStartPlacing;
    public event Action OnStopPlacing;

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

                ClearBoardMaterials();
                OnStopPlacing?.Invoke();
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

        OnStartPlacing?.Invoke();
    }

    public void PlacePiece(BoardTile boardTile)
    {
        isPlacing = false;
        pieceSelection.placedOnBoard = true;

        if (boardTile.isOccupied)
        {
            boardTile.pieceOnTile.IncreaseHeight();
            Destroy(pieceSelection);
        }
        else
        {
            boardTile.pieceOnTile = pieceSelection;
            boardTile.isOccupied = true;
        }

        pieceSelection = null;
        pieceSelectionTransform = null;

        OnStopPlacing?.Invoke();
    }

    public void ShowPrismRange(Prism prism, int range)
    {
        ClearBoardMaterials();

        for (int i = 1; i <= range; i++)
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
                boardTile[i][j].ChangeMeshRenderer(false, null);
        }
    }
}
