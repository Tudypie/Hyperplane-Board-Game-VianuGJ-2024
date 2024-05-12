using UnityEngine;

public class Cuboid : Piece
{
    public override void ChangeHeight(int value)
    {
        base.ChangeHeight(value);
        health += pieceSO.pieceStats[pieceSO.defaultStatsIndex].volume * value;
    }

    public override void Heal(float amount)
    {
        health = Mathf.Min(health + amount, pieceSO.pieceStats[pieceSO.defaultStatsIndex].volume * height);
    }

    public override void HealOneFourth()
    {
        health = Mathf.Min(health + pieceSO.pieceStats[pieceSO.defaultStatsIndex].volume / 4, pieceSO.pieceStats[pieceSO.defaultStatsIndex].volume * height);
    }
}
