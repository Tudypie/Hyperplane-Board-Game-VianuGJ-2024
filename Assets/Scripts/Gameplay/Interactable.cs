using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    //[SerializeField] private UnityEvent onInteractEvent;
    public bool canInteract = true;

    private const string interactableLayer = "Interactable";

    public virtual void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer(interactableLayer);
    }

    public virtual void OnInteract()
    {
        //onInteractEvent?.Invoke();
    }

    public virtual void OnFocus() { }

    public virtual void OnLoseFocus() { }

    public void SetCanInteract(bool value) => canInteract = value;
}
