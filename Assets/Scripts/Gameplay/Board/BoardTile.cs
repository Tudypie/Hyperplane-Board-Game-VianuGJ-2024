using TreeEditor;
using UnityEngine;

public class BoardTile : Interactable
{
    public int row;
    public int col;

    public bool occupied = false;

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
            boardManager.pieceSelectionTransform.position = transform.position;
            boardManager.pieceSelection.row = row;
            boardManager.pieceSelection.col = col;

            if (boardManager.pieceSelection.TryGetComponent(out Prism prism))
            {
                boardManager.ShowPrismRange(prism);
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
