using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Serializable]
    public struct CardInDeck
    {
        public GameObject prefab;
        public int count;
        public bool canBeReshuffled;
    }

    [Serializable]
    public struct CardSpawnpoint
    {
        public Transform spawnpoint;
        public Card card;
        public bool isOccupied;
    }

    [Header("References")]
    public GameObject[] playerPiecePrefab;
    public GameObject[] opponentPiecePrefab;
    public Transform[] playerPieceSpawnpoint;
    public Transform[] opponentPieceSpawnpoint;
    public CardSpawnpoint[] cardSpawnpoint;

    [Header("Card Deck")]
    public List<CardInDeck> cardList = new List<CardInDeck>();
    public List<Card> cardDeck = new List<Card>();

    [Header("Game Settings")]
    public int maxPiecesInHand = 4;
    public int maxCardsInHand = 3;
    public int maxPlayerMoves = 2;
    public int maxOpponentMoves = 2;

    [Header("Game Stats")]
    public int playerPiecesInHand = 0;
    public int opponentPiecesInHand = 0;
    public int cardsInHand = 0;
    public int remainingPlayerMoves = 0;
    public int remainingOpponentMoves = 0;
    public int cardsDrawedCount = 0;
    public int timesReshuffled = 0;
    public int knowledge = 0;
    public bool isPlayerTurn;

    private BoardManager boardManager;

    public static GameManager instance;

    private void Awake()
    {
        instance = this;

        foreach(CardInDeck card in cardList)
            for(int i = 0; i < card.count; i++)
                cardDeck.Add(card.prefab.GetComponent<Card>());

        remainingPlayerMoves = maxPlayerMoves;
        remainingOpponentMoves = maxOpponentMoves;
        ShuffleCardDeck();
        DrawPieces();
        isPlayerTurn = true;
        DrawPieces();
    }

    private void Start()
    {
        boardManager = BoardManager.instance;
    }

    private void ShuffleCardDeck()
    {
        for (int i = 0; i < cardDeck.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(0, cardDeck.Count);
            Card currentCard = cardDeck[i];
            cardDeck[i] = cardDeck[rnd];
            cardDeck[rnd] = currentCard;
        }
        timesReshuffled++;
    }

    public void PerformPlayerMove()
    {
        remainingPlayerMoves--;
        if (remainingPlayerMoves <= 0)
        {
            remainingOpponentMoves = maxOpponentMoves;
            if (opponentPiecesInHand == 0)
                DrawPieces();

            isPlayerTurn = false;
        }
    }

    public void PerformOpponentMove()
    {
        remainingOpponentMoves--;
        if (remainingOpponentMoves <= 0)
        {
            remainingPlayerMoves = maxPlayerMoves;
            isPlayerTurn = true;
        }
    }

    public void DrawPieces()
    {
        if (isPlayerTurn)
        {
            for (int i = 0; i < maxPiecesInHand; i++)
            {
                int rnd = UnityEngine.Random.Range(0, playerPiecePrefab.Length);
                Instantiate(playerPiecePrefab[rnd], playerPieceSpawnpoint[i].position, Quaternion.identity);      
            }
            playerPiecesInHand = maxPiecesInHand;
        }
        else
        {
            for (int i = 0; i < maxPiecesInHand; i++)
            {
                int rnd = UnityEngine.Random.Range(0, opponentPiecePrefab.Length);
                Piece opponentPiece = Instantiate(opponentPiecePrefab[rnd], opponentPieceSpawnpoint[i].position, Quaternion.identity).GetComponent<Piece>();
                OpponentAI.instance.piecesInHand.Add(opponentPiece);
            }
            opponentPiecesInHand = maxPiecesInHand;
        }

    }

    public void DrawCard()
    {
        if (cardsInHand >= maxCardsInHand) return;

        GameObject drawedCard;
        for(int i = 0; i < cardSpawnpoint.Length; i++)
        {
            if (!cardSpawnpoint[i].isOccupied)
            {
                drawedCard = Instantiate(cardDeck[cardsDrawedCount].gameObject, cardSpawnpoint[i].spawnpoint.position, Quaternion.identity);
                drawedCard.transform.parent = cardSpawnpoint[i].spawnpoint;
                cardSpawnpoint[i].card = drawedCard.GetComponent<Card>();
                cardSpawnpoint[i].isOccupied = true;
                break;
            }
        }
        cardsInHand++;
        cardsDrawedCount++;
        PerformPlayerMove();
    }

    public void StartUsingCard(Card card, bool selectOnBoard, string methodToCall, float methodParameter)
    {
        if(selectOnBoard)
            boardManager.StartSelectingTiles(card, methodToCall, methodParameter);
        else
        {
            boardManager.SendMessage(methodToCall, methodParameter);
            UseCard(card);
        }
    }

    public void UseCard(Card card)
    {
        for (int i = 0; i < cardSpawnpoint.Length; i++)
        {
            if (cardSpawnpoint[i].card == card)
            {
                cardSpawnpoint[i].card = null;
                cardSpawnpoint[i].isOccupied = false;
            }
        }
        cardsInHand--;
        Destroy(card.gameObject);
        PerformPlayerMove();
    }
}
