using UnityEngine;

public class Cuboid : Piece
{
    [Header("Cuboid Stats")]
    [SerializeField] private float health;
    [SerializeField] private int height = 1;

    public override void Awake()
    {
        base.Awake();

        health = pieceSO.pieceStats[pieceSO.defaultAngleIndex].volume;
    }

    public void IncreaseHeight(int addedHeight)
    {
        height += addedHeight;
        health += health / 2 * addedHeight;
        piecePart[height - 1].SetActive(true);
    }
}
