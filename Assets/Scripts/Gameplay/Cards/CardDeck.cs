using UnityEngine;

public class CardDeck : Interactable
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material onFocusMaterial;
    [SerializeField] private Material normalMaterial;

    private BoardManager boardManager;
    private GameManager gameManager;

    private void Start()
    {
        boardManager = BoardManager.instance;
        gameManager = GameManager.instance;

        boardManager.OnStartAction += DisableCollider;
        boardManager.OnStopAction += EnableCollider;
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (gameManager.cardsInHand < gameManager.maxCardsInHand)
            gameManager.DrawCard();
    }

    public override void OnFocus()
    {
        base.OnFocus();

        meshRenderer.material = onFocusMaterial;
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        meshRenderer.material = normalMaterial;
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
}
