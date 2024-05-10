using UnityEngine;

public class Prism : Piece
{

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
