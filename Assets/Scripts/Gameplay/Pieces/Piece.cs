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
    public float health;
    public int height;
    public int maxHeight;
    public int row;
    public int col;

    [HideInInspector]
    public BoardManager boardManager;

    public override void Awake()
    {
        base.Awake();

        health = pieceSO.pieceStats[pieceSO.defaultStatsIndex].volume;
        height = 1;
    }

    private void Start()
    {
        boardManager = BoardManager.instance;
        boardManager.OnStartPlacing += DisableCollider;
        boardManager.OnStopPlacing += EnableCollider;
    }

    private void OnDisable()
    {
        boardManager.OnStartPlacing -= DisableCollider;
        boardManager.OnStopPlacing -= EnableCollider;
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

        if (!placedOnBoard && !boardManager.isPlacing)
            ChangeMaterial(onFocusMaterial);
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (!placedOnBoard && !boardManager.isPlacing)
            ChangeMaterial(normalMaterial);
    }

    private void EnableCollider()
    {
        canInteract = true;
        GetComponent<BoxCollider>().enabled = true;
    }

    private void DisableCollider()
    {
        canInteract = false;
        GetComponent<BoxCollider>().enabled = false;
    }

    private void ChangeMaterial(Material material)
    {
        foreach (GameObject piece in piecePart)
        {
            if (!piece.activeSelf) break;
            piece.GetComponent<MeshRenderer>().material = material;
        }
    }

    public void IncreaseHeight(int addedHeight = 1)
    {
        height += addedHeight;
        health += health * addedHeight;
        piecePart[height - 1].SetActive(true);
    }

    public void DecreaseHeight(int decreasedHeight = 1)
    {
        height -= decreasedHeight;
        health -= health * decreasedHeight;
        piecePart[height].SetActive(false);
    }
}
