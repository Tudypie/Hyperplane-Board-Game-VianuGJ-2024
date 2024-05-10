using UnityEngine;

public class Piece : Interactable
{

    [Header("Piece Parameters")]
    public PieceSO pieceSO;
    public bool placedOnBoard = false;
    public GameObject[] piecePart;
    
    [SerializeField] private Material onFocusMaterial;
    private Material normalMaterial;

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
            BoardPlacing.instance.StartPlacing(this);
            ChangeMaterial(normalMaterial);
        }
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (!BoardPlacing.instance.isPlacing)
        {
            ChangeMaterial(onFocusMaterial);
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (!BoardPlacing.instance.isPlacing)
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
