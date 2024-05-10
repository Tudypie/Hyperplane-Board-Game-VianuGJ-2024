using UnityEngine;

public class BoardTile : Interactable
{
    public bool occupied = false;

    public override void OnFocus()
    {
        base.OnFocus();

        if (BoardPlacing.instance.isPlacing && !occupied)
        {
            BoardPlacing.instance.pieceSelectionTransform.position = transform.position;
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();

        BoardPlacing.instance.Place(this);
    }
}
