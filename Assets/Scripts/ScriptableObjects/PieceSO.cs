using UnityEngine;

[CreateAssetMenu(fileName = "Piece", menuName = "Board Game/Piece")]
public class PieceSO : ScriptableObject
{
    public enum PieceType { prism, cuboid, sphere }

    public PieceType pieceType;
    public GameObject piecePrefab;
    public float volume;
    public float angle;

}
