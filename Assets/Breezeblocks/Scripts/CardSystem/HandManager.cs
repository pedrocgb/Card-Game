using Breezeblocks.Managers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DeckManager))] 
public class HandManager : MonoBehaviour
{
    #region Variables and Properties
    private ActorManager _actor = null;
    private DeckManager _deckManager = null;

    private List<CardData> _currentHand = new List<CardData>();
    public List<CardData> CurrentHand => _currentHand;
    private List<CardUI> _currentHandUI = new List<CardUI>();

    // Card animations settings
    [FoldoutGroup("Cards Animations", expanded: true)]
    [SerializeField]
    private float _startingAnchoredX = -800f;
    [FoldoutGroup("Cards Animations", expanded: true)]
    [SerializeField]
    private float _anchoredOffSet = 260f;

    // Components
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private RectTransform _deckUi = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private RectTransform _cardGroup = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Canvas _mainCanvas = null;

    private Vector3 _visiblePosition = new Vector3(-140f, 0f, 0f);
    private Vector3 _hiddenPosition = new Vector3(-5000f, 0f, 0f);

    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        _actor = GetComponent<ActorManager>();
        _deckManager = GetComponent<DeckManager>();
    }
    #endregion

    // ========================================================================

    #region Enemy Draw Methods
    /// <summary>
    /// Draw a number of cards from the deck, add it to current hand and animate them.
    /// </summary>
    /// <param name="Quantity"></param>
    public void DrawCards(int Quantity)
    {
        if (_actor is PlayerActor)
            StartCoroutine(EnumeratorDrawCards(Quantity));
        else
            DrawCardLogic(Quantity);
    }

    private void DrawCardLogic(int Quantity)
    {
        for (int i = 0; i < Quantity; i++)
        {
            if (_currentHand.Count >= _actor.Data.MaxHandSize)
            {
                Debug.LogWarning("Hand is full!");
                break;
            }


            if (_deckManager.CurrentDeck.Count == 0)
            {
                Debug.LogWarning("Deck is empty!");
                _deckManager.ReshuffleDiscardIntoDeck();
            }

            CardData drawnCard = _deckManager.GetTopCard();
            _currentHand.Add(drawnCard);
        }
    }

    /// <summary>
    /// Discard Card. Basic method, no UI.
    /// </summary>
    /// <param name="Card"></param>
    public void DiscardCard(CardData Card)
    {
        _currentHand.Remove(Card);
        _deckManager.DiscardPile.Add(Card);
    }
    #endregion

    // ========================================================================

    #region Player Card Draw Methods
    public void ShowHand()
    {
        _cardGroup.anchoredPosition = _visiblePosition;

    }
    public void HideHand()
    {
        _cardGroup.anchoredPosition = _hiddenPosition;
    }
    private CardUI DrawPlayerCard()
    {
        if (_currentHand.Count >= _actor.Data.MaxHandSize)
        {
            Debug.LogWarning("Hand is full!");
            return null;
        }

        // Draw card logic
        if (_deckManager.CurrentDeck.Count == 0)
        {
            Debug.LogWarning("Deck is empty!");
            _deckManager.ReshuffleDiscardIntoDeck();
        }

        CardData drawnCard = _deckManager.GetTopCard();
        _currentHand.Add(drawnCard);

        // Create a new card instance UI
        CardUI card = ObjectPooler.SpawnFromPool("Card", _cardGroup.transform.position, Quaternion.identity).GetComponent<CardUI>();
        card.Initialize(drawnCard);
        card.transform.SetParent(_cardGroup, false);

        _currentHandUI.Add(card);

        // Validate if cards are playable, unplayable cards are greyout;
        ValidadeCard(_actor.Stats.CurrentActions, _actor.Positioning.CurrentPosition, card);

        return card;
    }
    /// <summary>
    /// Draw Cards UI animation method.
    /// </summary>
    /// <param name="quantity"></param>
    /// <returns></returns>
    private IEnumerator EnumeratorDrawCards(int quantity)
    {
        Vector3 deckPos = UAnchoredPositions.GetAnchoredPositionFromWorld(_deckUi, _cardGroup, _mainCanvas);

        for (int i = 0; i < quantity; i++)
        {
            CardUI ui = DrawPlayerCard();
            if (ui == null)
                yield break;

            // Temporarily place it at deck position
            RectTransform rt = ui.GetComponent<RectTransform>();
            rt.anchoredPosition = deckPos;

            // Recalculate all card positions
            UpdateCardPositions();

            // Animate only the newly drawn card to its new anchored position
            Vector3 targetPos = rt.anchoredPosition;
            rt.anchoredPosition = deckPos;

            ui.Animations.PlayDrawAnimation(deckPos, targetPos);

            yield return new WaitForSeconds(0.5f);
        }
    }
    private void UpdateCardPositions()
    {
        Vector3 pos = new Vector3(_cardGroup.anchoredPosition.x + _startingAnchoredX, 0f, 0f);

        for (int i = 0; i < _currentHandUI.Count; i++)
        {
            RectTransform rt = _currentHandUI[i].GetComponent<RectTransform>();
            rt.anchoredPosition = pos;

            pos += new Vector3(_anchoredOffSet, 0f, 0f);
        }
    }

    /// <summary>
    /// Discard Card. UI method, for player actors only.
    /// </summary>
    /// <param name="Card"></param>
    public void DiscardCard(CardUI Card)
    {
        _currentHand.Remove(Card.CardData);
        _deckManager.DiscardPile.Add(Card.CardData);
        _currentHandUI.Remove(Card);

        Card.gameObject.SetActive(false);
    }
    public void DiscardHand(bool IsPlayer)
    {
        if (IsPlayer)
        {
            // Discard all cards in the hand
            foreach (CardUI cardUI in _currentHandUI)
            {
                _deckManager.DiscardPile.Add(cardUI.CardData);
                cardUI.gameObject.SetActive(false);
            }
            _currentHand.Clear();
            _currentHandUI.Clear();
            // Reset card positions
            UpdateCardPositions();
        }
        else
        {
            // For AI or non-player actors, just clear the hand without UI
            _currentHand.Clear();
            _currentHandUI.Clear();
            _deckManager.DiscardPile.Clear();
        }
    }
    #endregion

    // ========================================================================

    #region Card Validation Methods - Player
    public void ValidadeHand(int CurrentActions, UEnums.Positions CurrentPosition)
    {
        // Loop through the current hand and check if the cards are valid.
        for (int i = 0; i < _currentHand.Count; i++)
        {
            CardUI cardUI = _currentHandUI[i];
            CardData cardData = _currentHand[i];

            bool hasEnoughActions = CurrentActions >= cardData.ActionCost;

            bool canUseFromThisPosition = false;
            switch (cardData.TargetType)
            {
                default:
                case UEnums.Target.Self:
                    canUseFromThisPosition = true;
                    break;
                case UEnums.Target.Ally:
                    canUseFromThisPosition = cardData.Positions.Contains(CurrentPosition);
                    break;
                case UEnums.Target.Enemy:
                    canUseFromThisPosition = cardData.Positions.Contains(CurrentPosition);
                    break;
            }

            bool isPlayable = hasEnoughActions && canUseFromThisPosition;

            cardUI.Validate(isPlayable);
        }
    }

    public void ValidadeCard(int CurrentActions, UEnums.Positions CurrentPosition, CardUI Card)
    {

        bool hasEnoughActions = CurrentActions >= Card.CardData.ActionCost;
        bool canUseFromThisPosition = false;
        switch (Card.CardData.TargetType)
        {
            default:
            case UEnums.Target.Self:
                canUseFromThisPosition = true;
                break;
            case UEnums.Target.Ally:
                canUseFromThisPosition = Card.CardData.Positions.Contains(CurrentPosition);
                break;
            case UEnums.Target.Enemy:
                canUseFromThisPosition = Card.CardData.Positions.Contains(CurrentPosition);
                break;
        }

        bool isPlayable = hasEnoughActions && canUseFromThisPosition;

        Card.Validate(isPlayable);
    }
    #endregion

    // ========================================================================
}
