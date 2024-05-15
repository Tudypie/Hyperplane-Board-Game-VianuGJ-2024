using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prism : Piece
{
    [Header("Prism References")]
    public Material attackMaterial;
    public LineRenderer[] lasers;

    [Header("Prism Stats")]
    public float damage;
    public List<List<BoardTile>> tilesInRange = new List<List<BoardTile>>();
    public List<BoardTile> tilesLeftInRange = new List<BoardTile>();
    public List<BoardTile> tilesForwardInRange = new List<BoardTile>();
    public List<BoardTile> tilesRightInRange = new List<BoardTile>();
    public int rotationDirectionIndex = 0;
    private bool isLerpingRotation = false;
    private Quaternion lerpRotation;

    public override void Awake()
    {
        base.Awake();

        statsIndex = pieceSO.defaultStatsIndex;
        health = pieceSO.pieceStats[statsIndex].volume * height;
        damage = pieceSO.pieceStats[statsIndex].damage * height;
        rotationDirectionIndex = 1;
    }

    private void Update()
    {
        if (isLerpingRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lerpRotation, 0.3f);
            if(Quaternion.Angle(transform.rotation, lerpRotation) < 0.1f)
            {
                transform.rotation = lerpRotation;
                isLerpingRotation = false;
            }
        }
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
        StartCoroutine(AttackSequence(piece));
    }

    private IEnumerator AttackSequence(Piece piece)
    {
        lasers[height - 1].SetPosition(0, lasers[height - 1].gameObject.transform.parent.position);
        lasers[height - 1].SetPosition(1, piece.piecePart[piece.height - 1].transform.position + new Vector3(0f, 0.4f * piece.height, 0f));
        lasers[height - 1].enabled = true;
        yield return new WaitForSeconds(0.3f);
        lasers[height - 1].enabled = false;
        piece.TakeDamage(damage);
    }

    public void AddTilesInRange()
    {
        tilesInRange.Clear();
        tilesInRange.Add(tilesLeftInRange);
        tilesInRange.Add(tilesForwardInRange);
        tilesInRange.Add(tilesRightInRange);
    }

    public void RotateRight()
    {
        if (rotationDirectionIndex == 2) return;
        rotationDirectionIndex++;
        RotateInAttackDirection();
        if (!boardManager.isPlacing)
        {
            boardManager.StopAttackingPiece();
            gameManager.PerformMove();
        }
    }

    public void RotateLeft()
    {
        if (rotationDirectionIndex == 0) return;
        rotationDirectionIndex--;
        RotateInAttackDirection();
        if (!boardManager.isPlacing)
        {
            boardManager.StopAttackingPiece();
            gameManager.PerformMove();
        }
    }

    public void RotateInAttackDirection()
    {
        if (rotationDirectionIndex == 0)
        {
            lerpRotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
            isEnemyPiece ? 45 : -45, transform.rotation.eulerAngles.z);
            isLerpingRotation = true;
        }
        else if (rotationDirectionIndex == 1)
        {
            lerpRotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
            0, transform.rotation.eulerAngles.z);
            isLerpingRotation = true;
        }
        else if (rotationDirectionIndex == 2)
        {
            lerpRotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
            isEnemyPiece ? -45 : 45, transform.rotation.eulerAngles.z);
            isLerpingRotation = true;
        }
        boardManager.ShowPrismTilesInRotationDirection(this, height, rotationDirectionIndex, isEnemyPiece ? normalMaterial : onFocusMaterial);
    }
}
