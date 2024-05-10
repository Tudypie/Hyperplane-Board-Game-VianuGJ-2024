using UnityEngine;

public class BoardTile : Interactable
{
    public override void OnFocus()
    {
        base.OnFocus();

        if(BoardPlacing.instance.isPlacing )
        {
            BoardPlacing.instance.pieceSelection.position = transform.position;
        }
    }
}
