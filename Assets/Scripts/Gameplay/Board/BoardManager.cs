using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public bool isPlacing = false;
    public bool isAttacking = false;
    public bool isSelectingTile = false;

    [HideInInspector] public Piece pieceSelection;
    [HideInInspector] public Transform pieceSelectionTransform;
    private Vector3 initialPiecePosition;
    private Quaternion initialPieceRotation;

    private List<BoardTile[]> boardTiles = new List<BoardTile[]>();
    private int boardChildCount;
    private List<Prism> prismsOnBoard = new List<Prism>();
    private List<Cuboid> cuboidsOnBoard = new List<Cuboid>();

    private Card cardSelection;
    private string methodToCallOnSelected;
    private float methodParameterOnSelected;
    private int selectedTiles;

    public event Action OnStartAction;
    public event Action OnStopAction;

    private InputMaster controls;
    private GameManager gameManager;

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
        gameManager = GameManager.instance;
    }

    private void Update()
    {
        HandleBoardInput();
    }

    private void HandleBoardInput()
    {
        if (isPlacing && controls.UI.RightClick.WasPressedThisFrame())
        {
            StopPlacingPiece();
            pieceSelectionTransform.position = initialPiecePosition;
            pieceSelectionTransform.rotation = initialPieceRotation;
        }

        if (isAttacking && controls.UI.RightClick.WasPressedThisFrame())
            StopAttackingPiece();

        if (isSelectingTile && controls.UI.RightClick.WasPressedThisFrame())
            StopSelectingTiles();
    }

    public void StartPlacingPiece(Piece piece)
    {
        isPlacing = true;
        OnStartAction?.Invoke();

        pieceSelection = piece;
        pieceSelectionTransform = piece.gameObject.transform;
        initialPiecePosition = pieceSelectionTransform.position;
        initialPieceRotation = pieceSelectionTransform.rotation;
    }

    public void PlacePiece(BoardTile boardTiles)
    {
        pieceSelection.placedOnBoard = true;
        gameManager.piecesInHand--;
        if (gameManager.piecesInHand <= 0)
            gameManager.DrawPieces();

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
        else if(boardTiles.pieceOnTile.TryGetComponent(out Cuboid cuboid))
        {
            cuboidsOnBoard.Add(cuboid);
        }

        StopPlacingPiece();
        gameManager.PerformPlayerMove();
    }

    public void StopPlacingPiece()
    {
        isPlacing = false;
        OnStopAction?.Invoke();
        ClearBoardMaterials();
        pieceSelection = null;
        pieceSelectionTransform = null;
    }

    public void StartAttackingPiece(Prism prismAttacker)
    {
        isAttacking = true;
        OnStartAction?.Invoke();

        pieceSelection = prismAttacker;
        pieceSelectionTransform = prismAttacker.gameObject.transform;
        initialPiecePosition = pieceSelectionTransform.position;
        initialPieceRotation = pieceSelectionTransform.rotation;
        pieceSelection.TryGetComponent(out Prism prism);
        ShowPrismRange(prism, prism.height, prism.attackMaterial);
    }

    public void AttackPiece(Piece pieceToAttack)
    {
        pieceSelection.GetComponent<Prism>().Attack(pieceToAttack);
        pieceSelection = null;
        pieceSelectionTransform = null;
        ClearBoardMaterials();
        StopAttackingPiece();
        gameManager.PerformPlayerMove();
    }

    public void StopAttackingPiece()
    {
        isAttacking = false;
        OnStopAction?.Invoke();
        pieceSelection = null;
        pieceSelectionTransform = null;
        ClearBoardMaterials();
    }

    public void StartSelectingTiles(Card card, string methodToCall, float methodParameter)
    {
        isSelectingTile = true;
        selectedTiles = 0;
        cardSelection = card;
        methodToCallOnSelected = methodToCall;
        methodParameterOnSelected = methodParameter;
        OnStartAction?.Invoke();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (boardTiles[i][j].isOccupied && !boardTiles[i][j].isEnemyTile)
                {
                    if(methodToCall.Contains("Heal"))
                    {
                        if (boardTiles[i][j].pieceOnTile.health == boardTiles[i][j].pieceOnTile.maxHealth) 
                            continue;
                    }

                    if (methodToCall == "ChangeAngle")
                    {
                        if(boardTiles[i][j].pieceOnTile.TryGetComponent(out Prism prism))
                        {
                            if (methodParameter > 0 && prism.statsIndex == 4) continue;
                            else if (methodParameter < 0 && prism.statsIndex == 0) continue;
                        }
                        else if(boardTiles[i][j].pieceOnTile.TryGetComponent(out Cuboid cuboid))
                            continue;
                    }

                    boardTiles[i][j].ChangeMeshRenderer(true, boardTiles[i][j].pieceOnTile.onFocusMaterial);
                    boardTiles[i][j].canBeSelected = true;
                    selectedTiles++;
                }
            }
        }

        if (selectedTiles == 0) StopSelectingTiles();

    }

    public void SelectTile(BoardTile tile)
    {
        isSelectingTile = false;
        isSelectingTile = false;
        tile.pieceOnTile.SendMessage(methodToCallOnSelected, methodParameterOnSelected);
        StopSelectingTiles();
    }

    public void StopSelectingTiles()
    {
        OnStopAction?.Invoke();
        cardSelection = null;
        methodToCallOnSelected = "";
        methodParameterOnSelected = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (boardTiles[i][j].isOccupied && !boardTiles[i][j].isEnemyTile)
                {
                    boardTiles[i][j].ChangeMeshRenderer(false, null);
                    boardTiles[i][j].canBeSelected = false;
                }
            }
        }
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

    public void ClearBoardMaterials(List<BoardTile> boardTilesToClear = null)
    {
        if (boardTilesToClear == null)
        {
            for (int i = 0; i < boardTiles.Count; i++)
            {
                for (int j = 0; j < boardTiles[i].Length; j++)
                    boardTiles[i][j].ChangeMeshRenderer(false, null);
            }
        }
        else
        {
            foreach (BoardTile tile in boardTilesToClear)
                tile.ChangeMeshRenderer(false, null);
        }

    }

    public void RemoveAllCuboidsOnBoard()
    {
        foreach(Cuboid cuboid in cuboidsOnBoard)
            Destroy(cuboid.gameObject);
    }

    public void RemoveAllPrismsOnBoard()
    {
        foreach (Prism prism in prismsOnBoard)
            Destroy(prism.gameObject);
    }
}
