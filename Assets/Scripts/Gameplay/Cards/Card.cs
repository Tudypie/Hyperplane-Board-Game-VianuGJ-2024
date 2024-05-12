using UnityEngine;

public class Card : Interactable
{
    [SerializeField] private string methodToCallOnPiece;
    [SerializeField] private float methodParameter;
    [SerializeField] private MeshRenderer outlineMesh;

    private BoardManager boardManager;

    private void Start()
    {
        boardManager = BoardManager.instance;
    }

    public override void OnInteract()
    {
        base.OnInteract();

        boardManager.StartSelectingTiles(methodToCallOnPiece, methodParameter);
    }

    public override void OnFocus()
    {
        base.OnFocus();

        outlineMesh.material.color *= 2;
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        outlineMesh.material.color /= 2;
    }
}
