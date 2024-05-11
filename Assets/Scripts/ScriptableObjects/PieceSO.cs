using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Piece", menuName = "Board Game/Piece")]
public class PieceSO : ScriptableObject
{
    public enum PieceType { Attack, Defense }

    [Serializable]
    public struct PieceStats
    {
        public float angle;
        public float volume;
        public float damage;
    }

    public PieceType pieceType;
    public GameObject piecePrefab;
    public int defaultStatsIndex;
    public PieceStats[] pieceStats;

    //public float Health(float angle) { return defaultVolume * (angle / defaultAngle); }

    //public float Damage(float angle) { return defaultVolume * (2 - (angle / defaultAngle)); }

}
