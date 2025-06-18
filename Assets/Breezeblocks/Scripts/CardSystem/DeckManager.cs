using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    #region Variablens and Properties
    // Cards variables
    private List<CardInstance> _deck = new List<CardInstance>();
    public List<CardInstance> MainDeck => _deck;
    private List<CardInstance> _currentDeck = new List<CardInstance>();
    public List<CardInstance> CurrentDeck => _currentDeck;

    private List<CardInstance> _discardPile = new List<CardInstance>();
    public List<CardInstance> DiscardPile => _discardPile;  

    private List<CardInstance> _consumedPile = new List<CardInstance>();
    public List<CardInstance> ConsumedPile => _consumedPile;

    // Components
    private ActorManager _actor = null;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        _actor = GetComponent<ActorManager>();
    }
    private void Start()
    {
        InitializeDeck();
        _deck = new List<CardInstance>(_currentDeck);
    }

    public void InitializeDeck()
    {
        _currentDeck.Clear();
        _discardPile.Clear();
        _consumedPile.Clear();

        // Create cards instances and add to deck.
        foreach (var c in _actor.Data.StartingCards)
        {
            CardInstance card = new CardInstance(c);
            _currentDeck.Add(card);
        }

        // If the actor has a specialization, add specialized cards to the deck.
        if (_actor.Data.HasSpecialization)
        {
            foreach (var sc in _actor.Data.StartingSpecializedCards)
            {
                CardInstance card = new CardInstance(sc);
                _currentDeck.Add(card);
            }
        }
        ShuffleDeck();
    }
    #endregion

    // ========================================================================

    #region Deck management methods
    public CardInstance GetTopCard()
    {
        CardInstance card = _currentDeck[0];
        _currentDeck.RemoveAt(0);

        return card;
    }

    /// <summary>
    /// Shuffle current deck.
    /// </summary>
    public void ShuffleDeck()
    {
        for (int i = 0; i < _currentDeck.Count; i++)
        {
            int randomIndex = Random.Range(i, _currentDeck.Count);
            (_currentDeck[i], _currentDeck[randomIndex]) = (_currentDeck[randomIndex], _currentDeck[i]);
        }
    }    

    /// <summary>
    /// Add a new card to the Deck.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public bool AddCard(CardInstance card)
    {
        if (_currentDeck.Count >= 100)
        {
            Debug.LogWarning("Cannot add more cards, deck is full.");
            return false;
        }

        _currentDeck.Add(card);
        _deck.Add(card);
        return true;
    }

    /// <summary>
    /// Permanently removes a card from the Deck.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public bool RemoveCard(CardInstance card)
    {
        return _currentDeck.Remove(card);
    }

    /// <summary>
    /// Reshuffle the discard pile into the draw pile.
    /// </summary>
    public void ReshuffleDiscardIntoDeck()
    {
        if (_discardPile.Count == 0) return;
        
        _currentDeck.AddRange(_discardPile);
        _discardPile.Clear();
        ShuffleDeck();
    }

    /// <summary>
    /// Return deck to original state, removing any temporary cards.
    /// </summary>
    public void ResumeDeck()
    {
        _discardPile.Clear();
        _currentDeck.Clear();
        _currentDeck.AddRange(_deck);

        ShuffleDeck();
    }
    #endregion

    // ========================================================================

    #region Effects Methods
    public bool AddTemporaryCard(CardInstance card)
    {
        if (_currentDeck.Count >= 100)
        {
            Debug.LogWarning("Cannot add more cards, deck is full.");
            return false;
        }

        _currentDeck.Add(card);
        return true;
    }
    #endregion

    // ========================================================================
}