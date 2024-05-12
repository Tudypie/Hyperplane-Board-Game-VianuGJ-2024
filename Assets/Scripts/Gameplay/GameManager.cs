using System;
using System.Collections.Generic;
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
    public GameObject[] gamePiecePrefab;
    public Transform[] pieceSpawnpoint;
    public CardSpawnpoint[] cardSpawnpoint;

    [Header("Card Deck")]
    public List<CardInDeck> cardList = new List<CardInDeck>();
    public List<Card> cardDeck = new List<Card>();

    [Header("Game Settings")]
    public int maxPiecesInHand = 4;
    public int maxCardsInHand = 3;
    public int playerMoves = 2;
    public int opponentMoves = 2;

    [Header("Game Stats")]
    public int piecesInHand = 0;
    public int cardsInHand = 0;
    public int cardsDrawedCount = 0;
    public int timesReshuffled = 0;
    public int knowledge = 0;

    private BoardManager boardManager;

    public static GameManager instance;

    private void Awake()
    {
        instance = this;

        foreach(CardInDeck card in cardList)
            for(int i = 0; i < card.count; i++)
                cardDeck.Add(card.prefab.GetComponent<Card>());

        ShuffleCardDeck();
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

    public void DrawPieces()
    {
        for(int i = 0; i < maxPiecesInHand; i++)
        {
            int rnd = UnityEngine.Random.Range(0, gamePiecePrefab.Length);
            Instantiate(gamePiecePrefab[rnd], pieceSpawnpoint[i].position, Quaternion.identity);
        }
        piecesInHand = maxPiecesInHand;

    }

    public void DrawCard()
    {
        if (cardsInHand >= maxCardsInHand) return;

        GameObject drawedCard = null;

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
    }

    public void UseCard(Card card, bool selectOnBoard, string methodToCall, float methodParameter)
    {
        if(selectOnBoard)
            boardManager.StartSelectingTiles(card, methodToCall, methodParameter);
        else
        {
            boardManager.SendMessage(methodToCall, methodParameter);
            RemoveCard(card);
        }
    }

    public void RemoveCard(Card card)
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
    }
}
