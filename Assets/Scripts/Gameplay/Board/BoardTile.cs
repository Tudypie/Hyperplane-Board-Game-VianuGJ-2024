using TreeEditor;
using UnityEngine;

public class BoardTile : Interactable
{
    public int row;
    public int col;

    public bool isOccupied = false;

    [SerializeField] private Piece pieceOnTile;

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

        if (boardManager.isPlacing)
        {
            boardManager.pieceSelection.row = row;
            boardManager.pieceSelection.col = col;


            if (isOccupied)
            {
                pieceOnTile.SetCanInteract(false);
                boardManager.pieceSelectionTransform.position = transform.position + new Vector3(0, 0.8f * pieceOnTile.height, 0);

                if (boardManager.pieceSelection.TryGetComponent(out Prism prism))
                {
                    prism.height++;
                    boardManager.ShowPrismRange(prism, prism.height);
                }
            }
            else
            {
                boardManager.pieceSelectionTransform.position = transform.position;

                if (boardManager.pieceSelection.TryGetComponent(out Prism prism))
                {
                    boardManager.ShowPrismRange(prism, prism.height);
                }
            }
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if(boardManager.isPlacing)
        {
            if (boardManager.pieceSelection.TryGetComponent(out Prism prism))
            {
                prism.height--;
            }
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (boardManager.isPlacing)
        {
            boardManager.PlacePiece(this);
            pieceOnTile = boardManager.pieceSelection;
        }
    }

    public void ChangeMeshRenderer(bool active, Material material)
    {
        meshRenderer.enabled = active;
        meshRenderer.material = material;
    }
}
