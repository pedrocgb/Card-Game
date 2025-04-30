using System.Collections;
using System.Collections.Generic;
using Breezeblocks.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [FoldoutGroup("Deck Configuration", expanded: true)]
    [SerializeField] 
    private ActorData _actorData;
    [FoldoutGroup("Deck Configuration", expanded: true)]
    [SerializeField]
    private bool _isPlayerDeck = false;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Canvas _mainCanvas = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private RectTransform _deckUi = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private RectTransform _cardGroup = null;

    [FoldoutGroup("Cards Animations", expanded: true)]
    [SerializeField]
    private float _startingAnchoredX = -800f;
    [FoldoutGroup("Cards Animations", expanded: true)]
    [SerializeField]
    private float _anchoredOffSet = 260f;

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
        _currentDeck = new List<CardData>(_actorData.StartingCards);
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

    private CardUI DrawCard()
    {
        if (_currentDeck.Count == 0)
        {
            Debug.LogWarning("Deck is empty!");
            Shuffle();
            return null;
        }

        CardData drawnCard = _currentDeck[0];
        _currentDeck.RemoveAt(0);

        CardUI card = ObjectPooler.SpawnFromPool("Card", _cardGroup.transform.position, Quaternion.identity).GetComponent<CardUI>();
        card.Initialize(drawnCard);
        card.transform.SetParent(_cardGroup, false);     

        return card;
    }

    public void DrawCards(int Quantity)
    {
        StartCoroutine(EnumeratorDrawCards(Quantity));
    }

    private IEnumerator EnumeratorDrawCards(int quantity)
    {
        Vector3 pos = new Vector3(_cardGroup.anchoredPosition.x + _startingAnchoredX, 0f, 0f);
        Vector3 deckPos = UAnchoredPositions.GetAnchoredPositionFromWorld(_deckUi, _cardGroup, _mainCanvas);

        for (int i = 0; i < quantity; i++)
        {
            if (i > 0)
                pos += new Vector3(_anchoredOffSet, 0f, 0f);

            DrawCard().Animations.PlayDrawAnimation(deckPos, pos);
            yield return new WaitForSeconds(0.5f);
        }
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