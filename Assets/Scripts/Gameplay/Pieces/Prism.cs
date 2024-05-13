using System.Collections.Generic;
using UnityEngine;

public class Prism : Piece
{
    [Header("Prism References")]
    public Material attackMaterial;
    public LineRenderer laser;

    [Header("Prism Stats")]
    public float damage;
    public List<List<BoardTile>> tilesInRange = new List<List<BoardTile>>();
    public List<BoardTile> tilesForwardInRange = new List<BoardTile>();
    public List<BoardTile> tilesRightInRange = new List<BoardTile>();
    public List<BoardTile> tilesLeftInRange = new List<BoardTile>();
    public int rotationDirectionIndex = 0;

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
            boardManager.ShowPrismTilesInRotationDirection(this, height, rotationDirectionIndex, isEnemyPiece ? normalMaterial : onFocusMaterial);
        }
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        if (placedOnBoard && !boardManager.isPlacing && !boardManager.isAttacking)
        {
            boardManager.ClearBoardMaterials();
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (!isEnemyPiece && placedOnBoard && !boardManager.isPlacing)
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
        float healthDecrease = health / maxHealth;

        if (statsIndex + amount > 4)
            amount = 1;
        else if (statsIndex + amount < 0)
            amount = -1;

        statsIndex = Mathf.Clamp(statsIndex + (int)amount, 0, 4);
        damage = pieceSO.pieceStats[statsIndex].damage * height; 
        health = pieceSO.pieceStats[statsIndex].volume * height * healthDecrease;
        maxHealth = pieceSO.pieceStats[statsIndex].volume * height;

        transform.localScale = new Vector3(1 + 0.25f * (statsIndex - 2), transform.localScale.y, transform.localScale.z);
    }

    public void Attack(Piece piece)
    {
        piece.TakeDamage(damage);
    }

    public void RotateInAttackDirection()
    {
        if (rotationDirectionIndex == 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
            0, transform.rotation.eulerAngles.z);
        }
        else if (rotationDirectionIndex == 1)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
            isEnemyPiece ? -45 : 45, transform.rotation.eulerAngles.z);
        }
        else if (rotationDirectionIndex == 2)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
            isEnemyPiece ? 45 : -45, transform.rotation.eulerAngles.z);
        }
        boardManager.ShowPrismTilesInRotationDirection(this, height, rotationDirectionIndex, isEnemyPiece ? normalMaterial : onFocusMaterial);
    }
}
