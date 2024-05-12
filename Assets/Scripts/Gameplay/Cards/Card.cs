using UnityEngine;

public class Card : Interactable
{
    [SerializeField] private string methodToCallOnPiece;
    [SerializeField] private float methodParameter;
    [SerializeField] private bool useOnPrism = false;

    [SerializeField] private Material onFocusMaterial;
    private Material normalMaterial;
    private MeshRenderer mr;

    private BoardManager boardManager;

    public override void Awake()
    {
        base.Awake();

        mr = GetComponent<MeshRenderer>();
        normalMaterial = mr.material;
    }

    private void Start()
    {
        boardManager = BoardManager.instance;
    }

    public override void OnInteract()
    {
        base.OnInteract();

        boardManager.StartSelectingTiles(useOnPrism, methodToCallOnPiece, methodParameter);
    }

    public override void OnFocus()
    {
        base.OnFocus();

        mr.material = onFocusMaterial;
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        mr.material = normalMaterial;
    }
}
