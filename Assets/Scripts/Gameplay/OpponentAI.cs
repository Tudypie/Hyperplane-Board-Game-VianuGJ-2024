using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OpponentAI : MonoBehaviour
{
    public Piece pieceSelection;
    public List<Piece> piecesInHand;
    public List<BoardTile> occupiedTiles;
    public List<BoardTile> unoccupiedTiles;
    public List<Prism> prismsOnBoard;
    [Space]
    public Prism prismAttacker;
    public BoardTile tileToAttack;
    public int attackDirection = 0;
    public bool canAttackInCurrentDirection = false;

    private float moveDelay = 2.5f;
    private float currentMoveDelay;

    private BoardManager boardManager;
    private GameManager gameManager;

    public static OpponentAI instance { get; private set; }

    public bool CanAttack()
    {
        foreach (Prism prism in prismsOnBoard)
            for (int i = 0; i < prism.tilesInRange.Count; i++)
                foreach (BoardTile tileInRange in prism.tilesInRange[i])
                    if (!tileInRange.isEnemyTile && tileInRange.isOccupied) {
                        if(prism.rotationDirectionIndex == i) {
                            canAttackInCurrentDirection = true;
                        } else {
                            canAttackInCurrentDirection = false;
                        }
                        prismAttacker = prism;
                        tileToAttack = tileInRange;
                        attackDirection = i;
                        return true;
                    }
        return false;
    }

    private void Awake()
    {
        instance = this;
        currentMoveDelay = moveDelay;
    }

    private void Start()
    {
        boardManager = BoardManager.instance;
        gameManager = GameManager.instance;

        unoccupiedTiles = boardManager.GetEnemyTiles();
    }

    private void Update()
    {
        if (gameManager.isPlayerTurn) return;

        if (currentMoveDelay > 0)
        {
            currentMoveDelay -= Time.deltaTime;
        }
        else
        {
            Move();
            currentMoveDelay = moveDelay;
        }
    }

    private void Move()
    {
        if (CanAttack())
        {
            if(!canAttackInCurrentDirection)
            {
                if(prismAttacker.rotationDirectionIndex < attackDirection)
                    prismAttacker.RotateRight();
                else
                    prismAttacker.RotateLeft();
                return;
            }
            else
            {
                Debug.Log("Opponent " + prismAttacker.name + " attacks tile " + tileToAttack);
                Attack(prismAttacker, tileToAttack);
                return;
            }
        }
        else
        {
            for (int i = 0; i < piecesInHand.Count; i++)
            {
                if (piecesInHand[i].TryGetComponent(out Prism prism))
                {
                    Debug.Log("Opponent selects prism");
                    pieceSelection = prism;
                    break;
                }

                if (i == piecesInHand.Count - 1)
                {
                    Debug.Log("Opponent selects cuboid");
                    pieceSelection = piecesInHand[0];
                }
            }

            bool placedPiece = false;
            foreach (BoardTile tile in occupiedTiles)
            {
                if (tile.pieceOnTile.pieceSO.pieceType == pieceSelection.pieceSO.pieceType)
                {
                    if (tile.pieceOnTile.height >= tile.pieceOnTile.maxHeight-2) continue;

                    Debug.Log("Opponent places piece on occupied tile: " + tile.name);
                    PlacePiece(tile);
                    placedPiece = true;
                    break;
                }    
            }
            if (placedPiece) return;

            int rndTile = Random.Range(0, unoccupiedTiles.Count);
            Debug.Log("Opponent places piece on unnocupied tile: " + unoccupiedTiles[rndTile].name);
            PlacePiece(unoccupiedTiles[rndTile]);
        }
    }

    private void PlacePiece(BoardTile boardTile)
    {
        pieceSelection.row = boardTile.row;
        pieceSelection.col = boardTile.col;
        boardManager.pieceSelection = pieceSelection;
        boardManager.pieceSelectionTransform = pieceSelection.transform;
        pieceSelection.canInteract = true;

        piecesInHand.Remove(pieceSelection);
        if(!occupiedTiles.Contains(boardTile)) 
        {
            occupiedTiles.Add(boardTile);
            unoccupiedTiles.Remove(boardTile);
        }

        boardManager.PlacePiece(boardTile);
    }

    private void Attack(Prism prism, BoardTile boardTile)
    {
        boardManager.pieceSelection = prism;
        boardManager.pieceSelectionTransform = prism.transform;
        piecesInHand.Remove(pieceSelection);
        boardManager.AttackPiece(boardTile.pieceOnTile);
    }

    public void RemovePiece(Piece piece)
    {
        BoardTile tile = boardManager.boardTiles[piece.row][piece.col];
        occupiedTiles.Remove(tile);
        unoccupiedTiles.Add(tile);
        if (piece.TryGetComponent(out Prism prism))
            prismsOnBoard.Remove(prism);
    }
}
