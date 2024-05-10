using UnityEngine;

public class BoardPlacing : MonoBehaviour
{
    [SerializeField] public Transform[] placePoints;
    public bool isPlacing = false;
    public Transform pieceSelection;

    public static BoardPlacing instance;

    private void Awake()
    {
        instance = this;
    }

    public void PlacePiece(PieceSO piece)
    {
        isPlacing = true;
        pieceSelection = Instantiate(piece.piecePrefab, new Vector3(-100, -100, -100), Quaternion.identity).transform;
    }
}
