using System.Collections.Generic;
using Breezeblocks.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [FoldoutGroup("Deck Configuration", expanded: true)]
    [SerializeField] 
    private ClassesData _heroClass;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private RectTransform _canvasGroup = null;

    private List<CardData> _currentDeck = new List<CardData>();

    private void Start()
    {
        InitializeDeck();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DrawCard();
        }
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

        CardUI card = ObjectPooler.SpawnFromPool("Card", _canvasGroup.transform.position, Quaternion.identity).GetComponent<CardUI>();
        card.Initialize(drawnCard);
        card.transform.SetParent(_canvasGroup, false);

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