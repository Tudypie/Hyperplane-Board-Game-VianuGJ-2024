using UnityEngine;

public class CardDeck : Interactable
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material onFocusMaterial;
    [SerializeField] private Material normalMaterial;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (gameManager.cardsInHand < gameManager.maxCardsInHand)
            GameManager.instance.DrawCard();
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
}
