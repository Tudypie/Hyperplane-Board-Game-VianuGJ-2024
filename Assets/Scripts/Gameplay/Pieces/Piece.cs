using UnityEngine;
using UnityEngine.UI;

public class Piece : Interactable
{

    [Header("Piece References")]
    public PieceSO pieceSO;
    public GameObject[] piecePart;
    public Image[] healthBar;
    public Material normalMaterial;
    public Material onFocusMaterial;

    [Header("Piece Stats")]
    public int row;
    public int col;
    public int statsIndex;
    public float health;
    public float maxHealth;
    public int height;
    public int maxHeight;
    public bool isEnemyPiece = false;
    public bool placedOnBoard = false;

    [HideInInspector]
    public BoardManager boardManager;

    public override void Awake()
    {
        base.Awake();

        statsIndex = pieceSO.defaultStatsIndex;
        health = pieceSO.pieceStats[statsIndex].volume * height;
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

        if (placedOnBoard)
            piecePart[height - 1].transform.GetChild(0).gameObject.SetActive(true);

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

        if (placedOnBoard)
            piecePart[height - 1].transform.GetChild(0).gameObject.SetActive(false);
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
        piecePart[height - 1].SetActive(false);
        height += value;
        piecePart[height-1].SetActive(true);

        health += pieceSO.pieceStats[statsIndex].volume * value;
        maxHealth = pieceSO.pieceStats[statsIndex].volume * height;

        boardManager.CalculateAllPrismTilesInRange();
    }

    public void TakeDamage(float amount)
    {
        health = Mathf.Max(health - amount, 0);
        healthBar[height - 1].fillAmount = health / maxHealth;
        if (health <= 0)
            Destroy(gameObject);
    }

    public void Heal(float amount = 0) 
    {
        health = Mathf.Min(health + amount, pieceSO.pieceStats[statsIndex].volume * height);
        healthBar[height - 1].fillAmount = health / maxHealth;
    }

    public void HealOneFourth() 
    {
        health = Mathf.Min(health + maxHealth / 4, maxHealth);
        healthBar[height - 1].fillAmount = health / maxHealth;
    }

    public void HealTwoFourths()
    {
        health = Mathf.Min(health + maxHealth / 2, maxHealth);
        healthBar[height - 1].fillAmount = health / maxHealth;
    }
}
