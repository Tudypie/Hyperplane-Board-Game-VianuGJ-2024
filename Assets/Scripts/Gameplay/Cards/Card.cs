using UnityEngine;

public class Card : Interactable
{
    [SerializeField] private Material onFocusMaterial;
    private Material normalMaterial;
    private MeshRenderer mr;

    public override void Awake()
    {
        base.Awake();

        mr = GetComponent<MeshRenderer>();
        normalMaterial = mr.material;
    }

    public override void OnInteract()
    {
        base.OnInteract();  
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
