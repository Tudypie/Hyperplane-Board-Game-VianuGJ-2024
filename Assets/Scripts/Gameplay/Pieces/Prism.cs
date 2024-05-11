using UnityEngine;

public class Prism : Piece
{
    [Header("Prism References")]
    public LineRenderer laser;

    [Header("Prism Stats")]
    public int angleIndex;
    public float damage;

    public override void Awake()
    {
        base.Awake();

        angleIndex = pieceSO.defaultStatsIndex;
        health = pieceSO.pieceStats[pieceSO.defaultStatsIndex].volume;
        damage = pieceSO.pieceStats[angleIndex].damage;
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (placedOnBoard && !boardManager.isPlacing)
        {
            boardManager.ShowPrismRange(this, height);
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (placedOnBoard && !boardManager.isPlacing)
        {
            boardManager.ClearBoardMaterials();
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();
    }

    public void ChangeAngle(int index)
    {
        angleIndex = index;
        damage = pieceSO.pieceStats[angleIndex].damage * height;
        health = pieceSO.pieceStats[angleIndex].volume * height;
    }
}
