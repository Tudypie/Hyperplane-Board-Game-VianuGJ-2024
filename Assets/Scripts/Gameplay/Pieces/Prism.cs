using UnityEngine;

public class Prism : Piece
{
    [Header("Prism References")]
    public LineRenderer laser;

    [Header("Prism Stats")]
    [SerializeField] private int angleIndex;
    [SerializeField] private float health;
    [SerializeField] private float damage;
    [SerializeField] private int height;

    public int Height { get { return  height; } }

    public override void Awake()
    {
        base.Awake();

        angleIndex = pieceSO.defaultStatsIndex;
        damage = pieceSO.pieceStats[angleIndex].damage;
        health = pieceSO.pieceStats[angleIndex].volume;
        height = 1;
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (placedOnBoard)
        {
            boardManager.ShowPrismRange(this);
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (placedOnBoard)
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

    public void IncreaseHeight(int addedHeight)
    {
        height += addedHeight;
        damage += damage * addedHeight;
        health += health * addedHeight;
        piecePart[height - 1].SetActive(true);
    }

}
