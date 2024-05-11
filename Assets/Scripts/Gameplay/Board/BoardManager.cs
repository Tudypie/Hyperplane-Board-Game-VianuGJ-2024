using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public List<BoardTile[]> boardTiles = new List<BoardTile[]>();
    public List<Prism> prismsOnBoard = new List<Prism>();
    private int boardChildCount;

    [Header("Board Parameters")]
    public Piece pieceSelection;
    public Transform pieceSelectionTransform;
    private Vector3 initialPiecePosition;
    private Quaternion initialPieceRotation;

    public bool isPlacing = false;
    public bool isAttacking = false;

    public event Action OnStartPlacing;
    public event Action OnStopPlacing;
    public event Action OnStartAttacking;
    public event Action OnStopAttacking;

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
            boardTiles.Add(tiles);
        }
    }

    private void Start()
    {
        controls = InputManager.instance.INPUT;
    }

    private void Update()
    {
        HandleBoardInput();
    }

    private void HandleBoardInput()
    {
        if (isPlacing)
        {
            if (controls.UI.RightClick.WasPressedThisFrame())
            {
                isPlacing = false;
                OnStopPlacing?.Invoke();

                pieceSelectionTransform.position = initialPiecePosition;
                pieceSelectionTransform.rotation = initialPieceRotation;
                pieceSelection = null;
                pieceSelectionTransform = null;

                ClearBoardMaterials();
            }
        }

        if (isAttacking)
        {
            if (controls.UI.RightClick.WasPressedThisFrame())
            {
                isAttacking = false;
                OnStopAttacking?.Invoke();

                pieceSelectionTransform.position = initialPiecePosition;
                pieceSelectionTransform.rotation = initialPieceRotation;
                pieceSelection = null;
                pieceSelectionTransform = null;

                ClearBoardMaterials();
            }

            /*if (controls.Player.Rotate.WasPressedThisFrame())
            {
                if (pieceSelectionTransform.rotation.eulerAngles.y == 0)
                {
                    pieceSelectionTransform.rotation = Quaternion.Euler(pieceSelectionTransform.rotation.eulerAngles.x,
                    45, pieceSelectionTransform.rotation.eulerAngles.z);
                    ShowPrismRange(pieceSelection.GetComponent<Prism>(), pieceSelection.height, 0);
                }
                else if (pieceSelectionTransform.rotation.eulerAngles.y == 45)
                {
                        pieceSelectionTransform.rotation = Quaternion.Euler(pieceSelectionTransform.rotation.eulerAngles.x,
                    -45, pieceSelectionTransform.rotation.eulerAngles.z);
                    ShowPrismRange(pieceSelection.GetComponent<Prism>(), pieceSelection.height, 1);
                }
                else if (pieceSelectionTransform.rotation.eulerAngles.y == -45)
                {
                    pieceSelectionTransform.rotation = Quaternion.Euler(pieceSelectionTransform.rotation.eulerAngles.x,
                    0, pieceSelectionTransform.rotation.eulerAngles.z);
                    ShowPrismRange(pieceSelection.GetComponent<Prism>(), pieceSelection.height, 2);
                }
            }*/
        }
    }

    public void StartPlacingPiece(Piece piece)
    {
        isPlacing = true;
        OnStartPlacing?.Invoke();

        pieceSelection = piece;
        pieceSelectionTransform = piece.gameObject.transform;
        initialPiecePosition = pieceSelectionTransform.position;
        initialPieceRotation = pieceSelectionTransform.rotation;
    }

    public void PlacePiece(BoardTile boardTiles)
    {
        isPlacing = false;
        OnStopPlacing?.Invoke();
        pieceSelection.placedOnBoard = true;

        if (boardTiles.isOccupied)
        {
            boardTiles.pieceOnTile.ChangeHeight(1);
            Destroy(pieceSelection.gameObject);
        }
        else
        {
            boardTiles.pieceOnTile = pieceSelection;
            boardTiles.isOccupied = true;  
        }
        
        if(boardTiles.pieceOnTile.TryGetComponent(out Prism prism))
        {
            CalculatePrismTilesInRange(prism, prism.height);
            prismsOnBoard.Add(prism);
        }

        pieceSelection = null;
        pieceSelectionTransform = null;
    }

    public void StartAttackingPiece(Prism prismAttacker)
    {
        Debug.Log("Start Attacking");
        isAttacking = true;
        OnStartAttacking?.Invoke();

        pieceSelection = prismAttacker;
        pieceSelectionTransform = prismAttacker.gameObject.transform;
        initialPiecePosition = pieceSelectionTransform.position;
        initialPieceRotation = pieceSelectionTransform.rotation;
        pieceSelection.TryGetComponent(out Prism prism);
        ShowPrismRange(prism, prism.height, prism.attackMaterial);
    }

    public void AttackPiece(Piece pieceToAttack)
    {
        Debug.Log("Attack Piece");
        isAttacking = false;
        OnStopAttacking?.Invoke();

        pieceSelection.GetComponent<Prism>().Attack(pieceToAttack);

        pieceSelection = null;
        pieceSelectionTransform = null;
        ClearBoardMaterials();
    }

    public void ShowPrismRange(Prism prism, int height, Material material, int direction = -1)
    {
        ClearBoardMaterials();

        for (int i = 1; i <= height; i++)
        {
            if (direction != -1 && direction != 0) break;

            if (prism.row + i < boardTiles.Count)
            {
                if (boardTiles[prism.row + i][prism.col].isOccupied
                    && boardTiles[prism.row + i][prism.col].pieceOnTile.height >= height)
                {
                    boardTiles[prism.row + i][prism.col].ChangeMeshRenderer(true, material);
                    break;
                }
                else
                {
                    boardTiles[prism.row + i][prism.col].ChangeMeshRenderer(true, material);
                }
            }
        }

        for (int i = 1; i <= height; i++)
        {
            if (direction != -1 && direction != 1) break;

            if (prism.row + i < boardTiles.Count && prism.col + i < boardTiles[0].Length)
            {
                if (boardTiles[prism.row + i][prism.col + i].isOccupied
                    && boardTiles[prism.row + i][prism.col + i].pieceOnTile.height >= height)
                {
                    boardTiles[prism.row + i][prism.col + i].ChangeMeshRenderer(true, material);
                    break;
                }
                else
                {
                    boardTiles[prism.row + i][prism.col + i].ChangeMeshRenderer(true, material);
                }
            }
        }

        for (int i = 1; i <= height; i++)
        {
            if (direction != -1 && direction != 2) break;

            if (prism.row + i < boardTiles.Count && prism.col - i >= 0)
            {
                if (boardTiles[prism.row + i][prism.col - i].isOccupied
                    && boardTiles[prism.row + i][prism.col - i].pieceOnTile.height >= height)
                {
                    boardTiles[prism.row + i][prism.col - i].ChangeMeshRenderer(true, material);
                    break;
                }
                else
                {
                    boardTiles[prism.row + i][prism.col - i].ChangeMeshRenderer(true, material);
                }
            }
        }
    }

    public void CalculatePrismTilesInRange(Prism prism, int height, int direction = -1)
    {
        for (int i = 1; i <= height; i++)
        {
            if (direction != -1 && direction != 0) break;

            if (prism.row + i < boardTiles.Count)
            {
                if (boardTiles[prism.row + i][prism.col].isOccupied
                    && boardTiles[prism.row + i][prism.col].pieceOnTile.height >= height)
                {
                    prism.tilesInRange.Add(boardTiles[prism.row + i][prism.col]);
                    break;
                }
                else
                {
                    prism.tilesInRange.Add(boardTiles[prism.row + i][prism.col]);
                }
            }
        }

        for (int i = 1; i <= height; i++)
        {
            if (direction != -1 && direction != 1) break;

            if (prism.row + i < boardTiles.Count && prism.col + i < boardTiles[0].Length)
            {
                if (boardTiles[prism.row + i][prism.col + i].isOccupied
                    && boardTiles[prism.row + i][prism.col + i].pieceOnTile.height >= height)
                {
                    prism.tilesInRange.Add(boardTiles[prism.row + i][prism.col + i]);
                    break;
                }
                else
                {
                    prism.tilesInRange.Add(boardTiles[prism.row + i][prism.col + i]);
                }
            }
        }

        for (int i = 1; i <= height; i++)
        {
            if (direction != -1 && direction != 2) break;

            if (prism.row + i < boardTiles.Count && prism.col - i >= 0)
            {
                if (boardTiles[prism.row + i][prism.col - i].isOccupied
                    && boardTiles[prism.row + i][prism.col - i].pieceOnTile.height >= height)
                {
                    prism.tilesInRange.Add(boardTiles[prism.row + i][prism.col - i]);
                    break;
                }
                else
                {
                    prism.tilesInRange.Add(boardTiles[prism.row + i][prism.col - i]);
                }
            }
        }
    }

    public void CalculateAllPrismTilesInRange()
    {
        foreach(Prism prism in prismsOnBoard)
        {
            CalculatePrismTilesInRange(prism, prism.height);
        }
    }

    public void ClearBoardMaterials()
    {
        for (int i = 0; i < boardTiles.Count; i++)
        {
            for (int j = 0; j < boardTiles[i].Length; j++)
                boardTiles[i][j].ChangeMeshRenderer(false, null);
        }
    }
}
