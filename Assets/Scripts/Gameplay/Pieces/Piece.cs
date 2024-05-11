using UnityEngine;

public class Piece : Interactable
{

    [Header("Piece Parameters")]
    public PieceSO pieceSO;
    public GameObject[] piecePart;

    public Material onFocusMaterial;
    public Material normalMaterial;

    [Header("General Stats")]
    public bool placedOnBoard = false;
    public int row;
    public int col;

    [HideInInspector]
    public BoardManager boardManager;

    private void Start()
    {
        boardManager = BoardManager.instance;
    }

    public override void Awake()
    {
        base.Awake();

        normalMaterial = piecePart[0].GetComponent<MeshRenderer>().material;
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (!placedOnBoard)
        {
            boardManager.StartPlacingPiece(this);
            ChangeMaterial(normalMaterial);
        }
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (!boardManager.isPlacing)
        {
            ChangeMaterial(onFocusMaterial);
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (!boardManager.isPlacing)
        {
            ChangeMaterial(normalMaterial);
        }
    }

    private void ChangeMaterial(Material material)
    {
        foreach (GameObject piece in piecePart)
        {
            if (!piece.activeSelf) break;
            piece.GetComponent<MeshRenderer>().material = material;
        }
    }
}
