using UnityEngine;

public class Prism : Piece
{
    [Header("Prism References")]
    public LineRenderer laser;

    [Header("Prism Stats")]
    [SerializeField] private int angleIndex;
    [SerializeField] private float health;
    [SerializeField] private float damage;
    [SerializeField] private int height = 1;

    public override void Awake()
    {
        base.Awake();

        angleIndex = pieceSO.defaultAngleIndex;
        health = pieceSO.pieceStats[angleIndex].volume;
        damage = pieceSO.pieceStats[angleIndex].damage;
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (placedOnBoard)
        {
            laser.enabled = true;
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (placedOnBoard)
        {
            laser.enabled = false;
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (!placedOnBoard)
        {
            laser.enabled = true;
        }
    }

    public void ChangeAngle(int index)
    {
        angleIndex = index;
    }

    public void IncreaseHeight(int addedHeight)
    {
        height += addedHeight;
        health += health / 2 * addedHeight;
        piecePart[height - 1].SetActive(true);
    }

}
