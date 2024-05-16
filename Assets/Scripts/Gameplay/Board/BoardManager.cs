using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int rows = 6;
    public int cols = 5;
    public bool isPlacing = false;
    public bool onOccupiedTile = false;
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
    private UIManager uiManager;

    public static BoardManager instance;

    public List<BoardTile> GetEnemyTiles()
    {
        List<BoardTile> enemyTiles = new List<BoardTile>();
        for (int i = rows/2; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                enemyTiles.Add(boardTiles[i][j]);
            }
        }
        return enemyTiles;
    }

    private void Awake()
    {
        instance = this;

        for (int i = 0; i < rows; i++)
        {
            BoardTile[] tiles = new BoardTile[cols];
            for (int j = 0; j < cols; j++)
            {
                tiles[j] = transform.GetChild(boardChildCount).GetComponent<BoardTile>();
                tiles[j].row = i;
                tiles[j].col = j;
                boardChildCount++;
            }
            boardTiles.Add(tiles);
        }
    }

    private void Start()
    {
        controls = InputManager.instance.INPUT;
        gameManager = GameManager.instance;
        uiManager = UIManager.instance;
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
                pieceSelectionTransform.position = initialPiecePosition;
                pieceSelectionTransform.rotation = initialPieceRotation;
                StopPlacingPiece();
            }
        }

        if (isAttacking && controls.UI.RightClick.WasPressedThisFrame())
            StopAttackingPiece();

        if (isSelectingTile && controls.UI.RightClick.WasPressedThisFrame())
            StopSelectingTiles();

        if ((isPlacing || isAttacking) && pieceSelection.TryGetComponent(out Prism prism) && !onOccupiedTile)
        {
            if (controls.Player.RotatePieceR.WasPressedThisFrame())
                prism.RotateRight();
            else if (controls.Player.RotatePieceL.WasPressedThisFrame())
                prism.RotateLeft();
        }
    }

    public void StartPlacingPiece(Piece piece)
    {
        isPlacing = true;
        OnStartAction?.Invoke();
        uiManager.SetPlaceText(piece.TryGetComponent(out Prism p));
        uiManager.ActivateControlsPanel(true);

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
            if (boardTile.pieceOnTile.TryGetComponent(out Prism prism))
            {
                CalculatePrismTilesInRange(prism, prism.height);
                prismsOnBoard.Add(prism);
                if (!gameManager.isPlayerTurn)
                    OpponentAI.instance.prismsOnBoard.Add(prism);
            }
            else if (boardTile.pieceOnTile.TryGetComponent(out Cuboid cuboid))
            {
                cuboidsOnBoard.Add(cuboid);
            }
        }

        AudioManager.instance.PlaySound(AudioManager.instance.piecePlace);
        gameManager.OnPlacePiece();
        CalculateAllPrismTilesInRange();
        StopPlacingPiece();
    }

    public void StopPlacingPiece()
    {
        isPlacing = false;
        onOccupiedTile = false;
        OnStopAction?.Invoke();
        ClearBoardMaterials();
        pieceSelection = null;
        pieceSelectionTransform = null;
        uiManager.ClearActionText();
        uiManager.ActivateControlsPanel(false);

    }

    public void StartAttackingPiece(Prism prismAttacker)
    {
        isAttacking = true;
        OnStartAction?.Invoke();
        uiManager.SetAttackText();

        pieceSelection = prismAttacker;
        pieceSelectionTransform = prismAttacker.gameObject.transform;
        pieceSelection.TryGetComponent(out Prism prism);
        ShowPrismTilesInRotationDirection(prism, prism.height, prism.rotationDirectionIndex, prism.attackMaterial);
        uiManager.ActivateControlsPanel(true);
    }

    public void AttackPiece(Piece pieceToAttack)
    {
        pieceSelection.TryGetComponent(out Prism prism);
        prism.RotateInAttackDirection();
        prism.Attack(pieceToAttack);

        ClearBoardMaterials();
        StopAttackingPiece();
        gameManager.PerformMove();
    }

    public void StopAttackingPiece()
    {
        isAttacking = false;
        OnStopAction?.Invoke();
        pieceSelection = null;
        pieceSelectionTransform = null;
        ClearBoardMaterials();
        uiManager.ClearActionText();
        uiManager.ActivateControlsPanel(false);
    }

    public void StartSelectingTiles(Card card, string methodToCall, float methodParameter)
    {
        isSelectingTile = true;
        selectedTiles = 0;
        cardSelection = card;
        methodToCallOnSelected = methodToCall;
        methodParameterOnSelected = methodParameter;
        OnStartAction?.Invoke();
        uiManager.SetCardText();
        uiManager.ActivateControlsPanel(true);
        uiManager.ActivateControlsRotate(false);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
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

        if (selectedTiles == 0)
        {
            StopSelectingTiles();
        }

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
        uiManager.ClearActionText();
        uiManager.ActivateControlsPanel(false);
        uiManager.ActivateControlsRotate(true);
        cardSelection = null;
        methodToCallOnSelected = "";
        methodParameterOnSelected = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (boardTiles[i][j].isOccupied && !boardTiles[i][j].isEnemyTile)
                {
                    boardTiles[i][j].ChangeMeshRenderer(false, null);
                    boardTiles[i][j].canBeSelected = false;
                }
            }
        }
    }

    public void EndTurn()
    {
        if (isAttacking) StopAttackingPiece();
        OnStopAction?.Invoke();
    }

    public void ShowPrismTilesInRotationDirection(Prism prism, int height, int rotation, Material material = null)
    {
        ClearBoardMaterials();
        bool enemyTile = boardTiles[prism.row][prism.col].isEnemyTile;

        for (int i = 1; i <= height; i++)
        {
            if (rotation != 0) break;

            int value = enemyTile ? -i : i;
            if (prism.row + value < boardTiles.Count && prism.row + value >= 0 && prism.col - i >= 0)
            {
                if (boardTiles[prism.row + value][prism.col - i].isOccupied
                    && boardTiles[prism.row + value][prism.col - i].pieceOnTile.height >= height)
                {
                    boardTiles[prism.row + value][prism.col - i].ChangeMeshRenderer(true, material);
                    break;
                }
                else
                {
                    boardTiles[prism.row + value][prism.col - i].ChangeMeshRenderer(true, material);
                }
            }
        }

        for (int i = 1; i <= height; i++)
        {
            if (rotation != 1) break;

            int value = enemyTile ? -i : i;
            if (prism.row + value < boardTiles.Count && prism.row + value >= 0)
            {
                if (boardTiles[prism.row + value][prism.col].isOccupied
                    && boardTiles[prism.row + value][prism.col].pieceOnTile.height >= height)
                {
                    boardTiles[prism.row + value][prism.col].ChangeMeshRenderer(true, material);
                    break;
                }
                else
                {
                    boardTiles[prism.row + value][prism.col].ChangeMeshRenderer(true, material);
                }
            }
        }

        for (int i = 1; i <= height; i++)
        {
            if (rotation != 2) break;

            int value = enemyTile ? -i : i;
            if (prism.row + value < boardTiles.Count && prism.row + value >= 0 && prism.col + i < boardTiles[0].Length)
            {
                if (boardTiles[prism.row + value][prism.col + i].isOccupied
                    && boardTiles[prism.row + value][prism.col + i].pieceOnTile.height >= height)
                {
                    boardTiles[prism.row + value][prism.col + i].ChangeMeshRenderer(true, material);
                    break;
                }
                else
                {
                    boardTiles[prism.row + value][prism.col + i].ChangeMeshRenderer(true, material);
                }
            }
        }
    }

    public void CalculatePrismTilesInRange(Prism prism, int height)
    {
        bool enemyTile = boardTiles[prism.row][prism.col].isEnemyTile;
        prism.tilesLeftInRange.Clear();
        prism.tilesForwardInRange.Clear();
        prism.tilesRightInRange.Clear();

        for (int i = 1; i <= height; i++)
        {
            int value = enemyTile ? -i : i;
            if (prism.row + value < boardTiles.Count && prism.row + value >= 0 && prism.col - i >= 0)
            {
                if (boardTiles[prism.row + value][prism.col - i].isOccupied
                    && boardTiles[prism.row + value][prism.col - i].pieceOnTile.height >= height)
                {
                    prism.tilesLeftInRange.Add(boardTiles[prism.row + value][prism.col - i]);
                    break;
                }
                else
                {
                    prism.tilesLeftInRange.Add(boardTiles[prism.row + value][prism.col - i]);
                }
            }
        }

        for (int i = 1; i <= height; i++)
        {
            int value = enemyTile ? -i : i;
            if (prism.row + value < boardTiles.Count && prism.row + value >= 0)
            {
                if (boardTiles[prism.row + value][prism.col].isOccupied
                    && boardTiles[prism.row + value][prism.col].pieceOnTile.height >= height)
                {
                    prism.tilesForwardInRange.Add(boardTiles[prism.row + value][prism.col]);
                    break;
                }
                else
                {
                    prism.tilesForwardInRange.Add(boardTiles[prism.row + value][prism.col]);
                }
            }
        }

        for (int i = 1; i <= height; i++)
        {

            int value = enemyTile ? -i : i;
            if (prism.row + value < boardTiles.Count && prism.row + value >= 0 && prism.col + i < boardTiles[0].Length)
            {
                if (boardTiles[prism.row + value][prism.col + i].isOccupied
                    && boardTiles[prism.row + value][prism.col + i].pieceOnTile.height >= height)
                {
                    prism.tilesRightInRange.Add(boardTiles[prism.row + value][prism.col + i]);
                    break;
                }
                else
                {
                    prism.tilesRightInRange.Add(boardTiles[prism.row + value][prism.col + i]);
                }
            }
        }

        prism.AddTilesInRange();
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
        AudioManager.instance.PlaySound(AudioManager.instance.abilityRemoveAll);
        if (cuboidsOnBoard.Count == 0) return;

        foreach(Cuboid cuboid in cuboidsOnBoard)
        {
            boardTiles[cuboid.row][cuboid.col].pieceOnTile = null;
            boardTiles[cuboid.row][cuboid.col].isOccupied = false;
            OpponentAI.instance.RemovePiece(cuboid);
            Destroy(cuboid.gameObject);
        }
        cuboidsOnBoard.Clear();
    }

    public void RemoveAllPrismsOnBoard()
    {
        AudioManager.instance.PlaySound(AudioManager.instance.abilityRemoveAll);
        if (prismsOnBoard.Count == 0) return;

        foreach (Prism prism in prismsOnBoard)
        {
            boardTiles[prism.row][prism.col].pieceOnTile = null;
            boardTiles[prism.row][prism.col].isOccupied = false;
            OpponentAI.instance.RemovePiece(prism);
            Destroy(prism.gameObject);
        }
        prismsOnBoard.Clear();
    }
}
