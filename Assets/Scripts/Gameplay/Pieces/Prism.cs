using System.Collections.Generic;
using UnityEngine;

public class Prism : Piece
{
    [Header("Prism References")]
    public Material attackMaterial;
    public LineRenderer laser;

    [Header("Prism Stats")]
    public float damage;
    public List<BoardTile> tilesInRange;

    public override void Awake()
    {
        base.Awake();

        statsIndex = pieceSO.defaultStatsIndex;
        health = pieceSO.pieceStats[statsIndex].volume * height;
        damage = pieceSO.pieceStats[statsIndex].damage * height;
    }

    public override void OnFocus()
    {
        base.OnFocus();

        if (placedOnBoard && !boardManager.isPlacing && !boardManager.isAttacking)
        {
            boardManager.ShowPrismRange(this, height, onFocusMaterial);
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (placedOnBoard && !boardManager.isPlacing && !boardManager.isAttacking)
        {
            boardManager.ClearBoardMaterials(boardTilesToClear: tilesInRange);
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (placedOnBoard && !boardManager.isPlacing)
        {
            boardManager.StartAttackingPiece(this);
        }
    }

    public override void ChangeHeight(int value)
    {
        base.ChangeHeight(value);
        damage += pieceSO.pieceStats[statsIndex].damage * value;
    }

    public void ChangeAngle(float amount)
    {
        float healthDecrease = pieceSO.pieceStats[statsIndex].volume * height / health;

        if (statsIndex + amount > 4)
            amount = 1;
        else if (statsIndex + amount < 0)
            amount = -1;

        statsIndex = Mathf.Clamp(statsIndex + (int)amount, 0, 4);
        damage = pieceSO.pieceStats[statsIndex].damage * height; 
        health = pieceSO.pieceStats[statsIndex].volume * height * healthDecrease;

        transform.localScale = new Vector3(1 + 0.25f * (statsIndex - 2), transform.localScale.y, transform.localScale.z);
    }

    public void Attack(Piece piece)
    {
        piece.TakeDamage(damage);
    }
}
