using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorManager))]
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
        _currentDeck = new List<CardData>(_actor.Data.StartingCards);
        Shuffle();
    }
    #endregion

    // ========================================================================

    #region Deck management methods
    /// <summary>
    /// Shuffle the current deck.
    /// </summary>
    public void Shuffle()
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
    #endregion

    // ========================================================================
}