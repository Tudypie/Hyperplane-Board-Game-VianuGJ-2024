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

        health = pieceSO.pieceStats[pieceSO.defaultStatsIndex].volume;
        height = 1;
    }

    private void Update()
    {
        if (boardManager.isPlacing)
        {
            canInteract = false;
            GetComponent<BoxCollider>().enabled = false;
        } 
        else
        {
            canInteract = true;
            GetComponent<BoxCollider>().enabled = true;
        }
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
        {
            ChangeMaterial(onFocusMaterial);
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (!placedOnBoard && !boardManager.isPlacing)
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

    public void IncreaseHeight(int addedHeight)
    {
        height += addedHeight;
        health += health * addedHeight;
        piecePart[height - 1].SetActive(true);
    }
}
