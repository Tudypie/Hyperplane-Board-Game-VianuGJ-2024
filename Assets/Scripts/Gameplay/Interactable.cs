using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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

    public void OnMouseOver()
    {
        if(GameManager.instance.isPlayerTurn && !TutorialManager.instance.tutorialIsOpen)
        {
            OnFocus();
            if(InputManager.instance.INPUT.UI.Click.WasPerformedThisFrame() && !PlayerCamera.instance.isMoving)
                OnInteract();
        }
    }

    public void OnMouseExit()
    {
        if (GameManager.instance.isPlayerTurn)
            OnLoseFocus();
    }
}
