using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Deck Configuration")]
    [SerializeField] 
    private ClassesData _heroClass;

    private List<CardData> _currentDeck = new List<CardData>();

    private void Awake()
    {
        InitializeDeck();
    }

    private void InitializeDeck()
    {
        _currentDeck = new List<CardData>(_heroClass.StartingCards);
        Shuffle();
    }

    public void Shuffle()
    {
        for (int i = 0; i < _currentDeck.Count; i++)
        {
            int randomIndex = Random.Range(i, _currentDeck.Count);
            (_currentDeck[i], _currentDeck[randomIndex]) = (_currentDeck[randomIndex], _currentDeck[i]);
        }
    }

    public CardData DrawCard()
    {
        if (_currentDeck.Count == 0)
        {
            Debug.LogWarning("Deck is empty!");
            return null;
        }

        CardData drawnCard = _currentDeck[0];
        _currentDeck.RemoveAt(0);
        return drawnCard;
    }

    public bool AddCard(CardData card)
    {
        if (_currentDeck.Count >= 100)
        {
            Debug.LogWarning("Cannot add more cards, deck is full.");
            return false;
        }

        _currentDeck.Add(card);
        return true;
    }

    public bool RemoveCard(CardData card)
    {
        return _currentDeck.Remove(card);
    }
}