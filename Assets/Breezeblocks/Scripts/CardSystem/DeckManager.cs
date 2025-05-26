using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DeckManager : MonoBehaviour
{
    #region Variablens and Properties
    // Cards variables
    private List<CardData> _currentDeck = new List<CardData>();
    public List<CardData> CurrentDeck => _currentDeck;

    private List<CardData> _discardPile = new List<CardData>();
    public List<CardData> DiscardPile => _discardPile;  

    private List<CardData> _consumedPile = new List<CardData>();
    public List<CardData> ConsumedPile => _consumedPile;

    // Components
    private ActorManager _actor = null;
    #endregion

    // ========================================================================

    #region Initialization
    private void Start()
    {
        _actor = GetComponent<ActorManager>();

        InitializeDeck();
    }

    private void InitializeDeck()
    {
        if (_actor.Data.HasSpecialization)
        {
            List<CardData> finalDeck = new List<CardData>(_actor.Data.StartingCards);

            List<CardData> randomSpecialized = _actor.Data.ActorSpecialization.SpecializationCards
                .OrderBy(_ => Random.value)
                .Take(_actor.Data.SpecializedCardsQuantity)
                .ToList();

            finalDeck.AddRange(randomSpecialized);
            _currentDeck = finalDeck;
        }
        else
        {
            _currentDeck = new List<CardData>(_actor.Data.StartingCards);
        }

        ShuffleDeck();
    }
    #endregion

    // ========================================================================

    #region Deck management methods
    public CardData GetTopCard()
    {
        CardData card = _currentDeck[0];
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

    /// <summary>
    /// Permanently removes a card from the Deck.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public bool RemoveCard(CardData card)
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
    #endregion

    // ========================================================================
}