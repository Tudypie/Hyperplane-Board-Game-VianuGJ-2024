using UnityEngine;

public class Piece : Interactable
{

    [Header("Piece References")]
    public PieceSO pieceSO;
    public GameObject[] piecePart;
    public Material normalMaterial;
    public Material onFocusMaterial;

    [Header("Piece Stats")]
    public bool isEnemyPiece = false;
    public bool placedOnBoard = false;
    public float health;
    public float maxHealth;
    public int height;
    public int maxHeight;
    public int row;
    public int col;

    [HideInInspector]
    public BoardManager boardManager;

    public override void Awake()
    {
        base.Awake();

        health = pieceSO.pieceStats[pieceSO.defaultStatsIndex].volume * height;
        maxHealth = health;
    }

    private void Start()
    {
        boardManager = BoardManager.instance;
        if (isEnemyPiece) return;
        boardManager.OnStartAction += DisableCollider;
        boardManager.OnStopAction += EnableCollider;
    }

    private void OnDisable()
    {
        if (isEnemyPiece) return;
        boardManager.OnStartAction -= DisableCollider;
        boardManager.OnStopAction -= EnableCollider;
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (isEnemyPiece) return;

        if (!placedOnBoard)
        {
            boardManager.StartPlacingPiece(this);
            ChangeMaterial(normalMaterial);
        }
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (isEnemyPiece) return;

        if (!placedOnBoard && !boardManager.isPlacing)
            ChangeMaterial(onFocusMaterial);
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (isEnemyPiece) return;

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

    public virtual void ChangeHeight(int value)
    {
        height += value;
        piecePart[value > 0 ? height - 1 : height].SetActive(value > 0);
        boardManager.CalculateAllPrismTilesInRange();
    }

    public void TakeDamage(float amount)
    {
        health = Mathf.Max(health - amount, 0);
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public virtual void Heal(float amount = 0) { }

    public virtual void HealOneFourth() { }
}
