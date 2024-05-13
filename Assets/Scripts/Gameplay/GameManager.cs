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
    public int extraMoveRequiredKnowledge = 5;
    public int maxKnowledge = 5;
    public int requiredVolumeToWin = 1000;

    [Header("Game Stats")]
    public int playerPiecesInHand = 0;
    public int opponentPiecesInHand = 0;
    public int cardsInHand = 0;
    public int remainingPlayerMoves = 0;
    public int remainingOpponentMoves = 0;
    public int cardsDrawedCount = 0;
    public int timesReshuffled = 0;
    public int knowledge = 0;
    public float playerAccumulatedVolume = 0;
    public float opponentAccumulatedVolume = 0;
    public bool isPlayerTurn;

    private BoardManager boardManager;

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        boardManager = BoardManager.instance;

        foreach (CardInDeck card in cardList)
            for (int i = 0; i < card.count; i++)
                cardDeck.Add(card.prefab.GetComponent<Card>());

        remainingPlayerMoves = maxPlayerMoves;
        remainingOpponentMoves = maxOpponentMoves;
        ShuffleCardDeck();
        DrawPieces();
        isPlayerTurn = true;
        DrawPieces();
        UIManager.instance.SetTurnText(isPlayerTurn);
        UIManager.instance.SetMovesLeftText(remainingPlayerMoves);
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

    public void PerformMove()
    {
        if(isPlayerTurn)
        {
            remainingPlayerMoves--;
            UIManager.instance.SetMovesLeftText(remainingPlayerMoves);
            if (remainingPlayerMoves <= 0)
            {
                remainingOpponentMoves = maxOpponentMoves;
                isPlayerTurn = false;
                UIManager.instance.SetMovesLeftText(remainingOpponentMoves);
                UIManager.instance.SetTurnText(isPlayerTurn);
                boardManager.EndTurn();
            }
        }
        else
        {
            remainingOpponentMoves--;
            UIManager.instance.SetMovesLeftText(remainingOpponentMoves);
            if (remainingOpponentMoves <= 0)
            {
                remainingPlayerMoves = maxPlayerMoves;
                isPlayerTurn = true;
                UIManager.instance.SetMovesLeftText(remainingPlayerMoves);
                UIManager.instance.SetTurnText(isPlayerTurn);
                boardManager.EndTurn();
            }
        }
    }

    public void OnDestroyPiece(Piece piece)
    {
        if(piece.isEnemyPiece)
        {
            playerAccumulatedVolume += piece.maxHealth;
            UIManager.instance.UpdatePlayerVolumeFill(playerAccumulatedVolume, requiredVolumeToWin);
        }
        else
        {
            opponentAccumulatedVolume += piece.maxHealth;
            UIManager.instance.UpdateOpponentVolumeFill(opponentAccumulatedVolume, requiredVolumeToWin);
        }
    }

    public void OnPlacePiece()
    {
        if (isPlayerTurn)
        {
            playerPiecesInHand--;
            if (playerPiecesInHand <= 0)
                DrawPieces();

            PerformMove();
        }
        else
        {
            opponentPiecesInHand--;
            if (opponentPiecesInHand <= 0)
                DrawPieces();

            PerformMove();
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
                cardsInHand++;
                cardsDrawedCount++;
            }
        }
        PerformMove();
    }

    public void IncreaseKnowledge()
    {
        if (knowledge >= maxKnowledge) return;
        knowledge++;
        if (knowledge % extraMoveRequiredKnowledge == 0)
            maxPlayerMoves++;
    }

    public void StartUsingCard(Card card, string methodToCall, float methodParameter)
    {
        if (card.callOnPiece)
        {
            boardManager.StartSelectingTiles(card, methodToCall, methodParameter);
        }
        else if(card.callOnBoard)
        {
            boardManager.SendMessage(methodToCall, methodParameter);
            UseCard(card);
        }
        else if(card.callOnGameManager)
        {
            SendMessage(methodToCall, methodParameter);
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
        PerformMove();
    }
}
