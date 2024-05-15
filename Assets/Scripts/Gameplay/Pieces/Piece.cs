using UnityEngine;
using UnityEngine.UI;

public class Piece : Interactable
{

    [Header("Piece References")]
    public PieceSO pieceSO;
    public GameObject[] piecePart;
    public Image[] healthBar;
    public Transform healthFill;
    public Transform pieceFill;
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


    [HideInInspector] public BoardManager boardManager;
    [HideInInspector] public GameManager gameManager;
    [HideInInspector] public OpponentAI opponentAI;

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
        gameManager = GameManager.instance;
        opponentAI = OpponentAI.instance;

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
            AudioManager.instance.PlaySound(AudioManager.instance.pieceSelect);
            boardManager.StartPlacingPiece(this);
            ChangeMaterial(normalMaterial);
        }
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (!placedOnBoard && !boardManager.isPlacing && !isEnemyPiece)
            ChangeMaterial(onFocusMaterial);
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (!placedOnBoard && !boardManager.isPlacing && !isEnemyPiece)
            ChangeMaterial(normalMaterial);
    }

    private void EnableCollider()
    {
        Invoke(nameof(EnableColliderDelay), 0.2f);
    }

    private void EnableColliderDelay()
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
        healthBar[height - 1].fillAmount = health / maxHealth;
        healthFill.localScale = new Vector3(1f, health / maxHealth, 1f);
        pieceFill.localScale = new Vector3(1f, height, 1f);

        boardManager.CalculateAllPrismTilesInRange();
        AudioManager.instance.PlaySound(AudioManager.instance.pieceStack);
    }

    public void TakeDamage(float amount)
    {
        health = Mathf.Max(health - amount, 0);
        healthBar[height - 1].fillAmount = health / maxHealth;
        healthFill.localScale = new Vector3(1f, health / maxHealth, 1f);

        if (health <= 0)
        {
            if(TryGetComponent(out Prism prism))
            {
                if (gameManager.isPlayerTurn)
                    opponentAI.RemovePiece(this);

                boardManager.prismsOnBoard.Remove(prism);
            }
            else if (TryGetComponent(out Cuboid cuboid))
                boardManager.cuboidsOnBoard.Remove(cuboid);

            if (isEnemyPiece)
                opponentAI.RemovePiece(this);

            boardManager.boardTiles[row][col].pieceOnTile = null;
            boardManager.boardTiles[row][col].isOccupied = false;
            boardManager.CalculateAllPrismTilesInRange();
            gameManager.OnDestroyPiece(this);
            AudioManager.instance.PlaySound(AudioManager.instance.pieceDestroy);
            Destroy(gameObject);
        }
    }

    public void Heal(float percent) 
    {
        health = Mathf.Min(health + maxHealth * percent, maxHealth);
        healthBar[height - 1].fillAmount = health / maxHealth;
        healthFill.localScale = new Vector3(1f, health / maxHealth, 1f);
        pieceFill.localScale = new Vector3(1f, height, 1f);
        AudioManager.instance.PlaySound(AudioManager.instance.abilityHeal);
    }
}
