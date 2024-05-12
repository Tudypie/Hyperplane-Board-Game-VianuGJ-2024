using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public bool isPlacing = false;
    public bool isAttacking = false;
    public bool isSelectingTile = false;

    public Piece pieceSelection;
    public Transform pieceSelectionTransform;
    private Vector3 initialPiecePosition;
    private Quaternion initialPieceRotation;

    public List<BoardTile[]> boardTiles = new List<BoardTile[]>();
    private int boardChildCount;
    public List<Prism> prismsOnBoard = new List<Prism>();
    public List<Cuboid> cuboidsOnBoard = new List<Cuboid>();

    private Card cardSelection;
    private string methodToCallOnSelected;
    private float methodParameterOnSelected;
    private int selectedTiles;

    public event Action OnStartAction;
    public event Action OnStopAction;

    private InputMaster controls;
    private GameManager gameManager;

    public static BoardManager instance;

    public List<BoardTile> GetEnemyTiles()
    {
        List<BoardTile> enemyTiles = new List<BoardTile>();
        for (int i = 4; i < 8; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                enemyTiles.Add(boardTiles[i][j]);
            }
        }
        return enemyTiles;
    }

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
            pieceSelectionTransform.position = initialPiecePosition;
            pieceSelectionTransform.rotation = initialPieceRotation;
            StopPlacingPiece();
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

    public void PlacePiece(BoardTile boardTile)
    {
        pieceSelection.placedOnBoard = true;

        if (boardTile.isOccupied)
        {
            pieceSelectionTransform.position = boardTile.transform.position + new Vector3(0, 0.8f * boardTile.pieceOnTile.height, 0);
            boardTile.pieceOnTile.ChangeHeight(1);
            Destroy(pieceSelection.gameObject);
        }
        else
        {
            pieceSelectionTransform.position = boardTile.transform.position;
            boardTile.pieceOnTile = pieceSelection;
            boardTile.isOccupied = true;  
        }
        
        if(boardTile.pieceOnTile.TryGetComponent(out Prism prism))
        {
            CalculatePrismTilesInRange(prism, prism.height, false);
            prismsOnBoard.Add(prism);
            if (!gameManager.isPlayerTurn)
                OpponentAI.instance.prismsOnBoard.Add(prism);
        }
        else if(boardTile.pieceOnTile.TryGetComponent(out Cuboid cuboid))
        {
            cuboidsOnBoard.Add(cuboid);
        }

        StopPlacingPiece();

        if (gameManager.isPlayerTurn)
        {
            gameManager.playerPiecesInHand--;
            if (gameManager.playerPiecesInHand <= 0)
                gameManager.DrawPieces();

            gameManager.PerformPlayerMove();
        }
        else
        {
            gameManager.opponentPiecesInHand--;
            if (gameManager.opponentPiecesInHand <= 0)
                gameManager.DrawPieces();

            gameManager.PerformOpponentMove();
        }
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
        CalculatePrismTilesInRange(prism, prism.height, true, prism.attackMaterial);
    }

    public void AttackPiece(Piece pieceToAttack)
    {
        pieceSelection.GetComponent<Prism>().Attack(pieceToAttack);
        pieceSelection = null;
        pieceSelectionTransform = null;
        ClearBoardMaterials();
        StopAttackingPiece();


        if (gameManager.isPlayerTurn)
            gameManager.PerformPlayerMove();
        else
            gameManager.PerformOpponentMove();
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
        gameManager.UseCard(cardSelection);
        tile.pieceOnTile.SendMessage(methodToCallOnSelected, methodParameterOnSelected);
        StopSelectingTiles();
    }

    public void StopSelectingTiles()
    {
        isSelectingTile = false;
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

    public void CalculatePrismTilesInRange(Prism prism, int height, bool showMaterials, Material material = null, int direction = -1)
    {
        prism.tilesInRange.Clear();
        if(showMaterials)
            ClearBoardMaterials();

        bool enemyTile = boardTiles[prism.row][prism.col].isEnemyTile;

        for (int i = 1; i <= height; i++)
        {
            if (direction != -1 && direction != 0) break;

            int value = enemyTile ? -i : i;
            if (prism.row + value < boardTiles.Count && prism.row + value > 0)
            {
                if (boardTiles[prism.row + value][prism.col].isOccupied
                    && boardTiles[prism.row + value][prism.col].pieceOnTile.height >= height)
                {
                    prism.tilesInRange.Add(boardTiles[prism.row + value][prism.col]);
                    boardTiles[prism.row + value][prism.col].ChangeMeshRenderer(true, material);
                    break;
                }
                else
                {
                    prism.tilesInRange.Add(boardTiles[prism.row + value][prism.col]);
                    boardTiles[prism.row + value][prism.col].ChangeMeshRenderer(true, material);
                }
            }
        }

        for (int i = 1; i <= height; i++)
        {
            if (direction != -1 && direction != 1) break;

            int value = enemyTile ? -i : i;
            if (prism.row + value < boardTiles.Count && prism.row + value > 0 && prism.col + i < boardTiles[0].Length)
            {
                if (boardTiles[prism.row + value][prism.col + i].isOccupied
                    && boardTiles[prism.row + value][prism.col + i].pieceOnTile.height >= height)
                {
                    prism.tilesInRange.Add(boardTiles[prism.row + value][prism.col + i]);
                    boardTiles[prism.row + value][prism.col + i].ChangeMeshRenderer(true, material);
                    break;
                }
                else
                {
                    prism.tilesInRange.Add(boardTiles[prism.row + value][prism.col + i]);
                    boardTiles[prism.row + value][prism.col + i].ChangeMeshRenderer(true, material);
                }
            }
        }

        for (int i = 1; i <= height; i++)
        {
            if (direction != -1 && direction != 2) break;

            int value = enemyTile ? -i : i;
            if (prism.row + value < boardTiles.Count && prism.row + value > 0 && prism.col - i >= 0)
            {
                if (boardTiles[prism.row + value][prism.col - i].isOccupied
                    && boardTiles[prism.row + value][prism.col - i].pieceOnTile.height >= height)
                {
                    prism.tilesInRange.Add(boardTiles[prism.row + value][prism.col - i]);
                    boardTiles[prism.row + value][prism.col - i].ChangeMeshRenderer(true, material);
                    break;
                }
                else
                {
                    prism.tilesInRange.Add(boardTiles[prism.row + value][prism.col - i]);
                    boardTiles[prism.row + value][prism.col - i].ChangeMeshRenderer(true, material);
                }
            }
        }
    }

    public void CalculateAllPrismTilesInRange()
    {
        foreach(Prism prism in prismsOnBoard)
        {
            CalculatePrismTilesInRange(prism, prism.height, false);
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
        {
            boardTiles[cuboid.row][cuboid.col].pieceOnTile = null;
            boardTiles[cuboid.row][cuboid.col].isOccupied = false;
            cuboidsOnBoard.Remove(cuboid);
            Destroy(cuboid.gameObject);
        }

    }

    public void RemoveAllPrismsOnBoard()
    {
        foreach (Prism prism in prismsOnBoard)
        {
            boardTiles[prism.row][prism.col].pieceOnTile = null;
            boardTiles[prism.row][prism.col].isOccupied = false;
            prismsOnBoard.Remove(prism);
            Destroy(prism.gameObject);
        }
    }
}
