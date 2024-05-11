using UnityEngine;

public class Cuboid : Piece
{
    [Header("Cuboid Stats")]
    [SerializeField] private float volume;
    [SerializeField] private int height = 1;
    public float Health { get { return volume; } set { volume = value; } }

    public override void Awake()
    {
        base.Awake();

        Health = pieceSO.pieceStats[pieceSO.defaultStatsIndex].volume;
    }

    public void IncreaseHeight(int addedHeight)
    {
        height += addedHeight;
        Health += Health * addedHeight;
        piecePart[height - 1].SetActive(true);
    }
}
