using UnityEngine;

public class BoardTile : Interactable
{
    public int row;
    public int col;
    public Piece pieceOnTile;
    public bool isEnemyTile = false;
    public bool isOccupied = false;
    public bool canBeSelected = false;

    private MeshRenderer meshRenderer;
    private BoardManager boardManager;

    public override void Awake()
    {
        base.Awake();

        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        boardManager = BoardManager.instance;
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (isEnemyTile)
        {
            if (boardManager.isAttacking)
            {
                if (boardManager.pieceSelection.GetComponent<Prism>().tilesInRange.Contains(this))
                    ChangeMeshRenderer(true, boardManager.pieceSelection.onFocusMaterial);
            }
        }
        else
        {
            if (boardManager.isPlacing)
            {
                boardManager.ClearBoardMaterials();
                boardManager.pieceSelection.row = row;
                boardManager.pieceSelection.col = col;

                if (isOccupied && (boardManager.pieceSelection.pieceSO.pieceType != pieceOnTile.pieceSO.pieceType ||
                    pieceOnTile.height >= pieceOnTile.maxHeight))
                {
                    return;
                }
                else if (isOccupied)
                {
                    boardManager.pieceSelectionTransform.position = transform.position + new Vector3(0, 0.8f * pieceOnTile.height, 0);
                    if (boardManager.pieceSelection.TryGetComponent(out Prism prism))
                    {
                        boardManager.CalculatePrismTilesInRange(prism, pieceOnTile.height + 1, true, prism.onFocusMaterial);
                        boardManager.pieceSelectionTransform.localScale = pieceOnTile.transform.localScale;
                    }
                }
                else
                {
                    boardManager.pieceSelectionTransform.position = transform.position;
                    if (boardManager.pieceSelection.TryGetComponent(out Prism prism))
                    {
                        boardManager.CalculatePrismTilesInRange(prism, prism.height, true, prism.onFocusMaterial);
                    }
                }
                ChangeMeshRenderer(true, boardManager.pieceSelection.onFocusMaterial);
            }
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (isEnemyTile)
        {
            if (boardManager.isAttacking)
            {
                if (boardManager.pieceSelection.GetComponent<Prism>().tilesInRange.Contains(this))
                    ChangeMeshRenderer(true, boardManager.pieceSelection.GetComponent<Prism>().attackMaterial);
            }
        }
        else
        {
            if (boardManager.isPlacing)
                boardManager.pieceSelectionTransform.localScale = new Vector3(1, 1, 1);
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (isEnemyTile)
        {
            if (boardManager.isAttacking)
            {
                if(isOccupied && boardManager.pieceSelection.GetComponent<Prism>().tilesInRange.Contains(this))
                    boardManager.AttackPiece(pieceOnTile);
            }
        }
        else
        {
            if (boardManager.isPlacing)
            {
                if (!isOccupied)
                {
                    boardManager.PlacePiece(this);
                }
                else if (isOccupied && boardManager.pieceSelection.pieceSO.pieceType == pieceOnTile.pieceSO.pieceType
                    && pieceOnTile.height < pieceOnTile.maxHeight)
                {
                    boardManager.PlacePiece(this);
                }
            }
            else if(boardManager.isSelectingTile)
            {
                if(canBeSelected)
                    boardManager.SelectTile(this);
            }
        }
    }

    public void ChangeMeshRenderer(bool active, Material material)
    {
        meshRenderer.enabled = active;
        meshRenderer.material = material;
    }
}
