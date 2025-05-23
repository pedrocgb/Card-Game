using Breezeblocks.Managers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    #region Draw and Descart Cards Methods
    /// <summary>
    /// Draw a number of cards from the deck, add it to current hand and animate them.
    /// </summary>
    /// <param name="Quantity"></param>
    public void DrawCards(int Quantity)
    {
        if (_actor is PlayerActor)
            StartCoroutine(EnumeratorDrawCards(Quantity));
        else
            DrawEnemyCard(Quantity);
    }

    private void DrawEnemyCard(int Quantity)
    {
        Debug.Log("Called");
        for (int i = 0; i < Quantity; i++)
        {
            CardData drawnCard = _deckManager.GetTopCard();

            _currentHand.Add(drawnCard);

            Debug.Log("Card drawn: " + i);
        }
    }

    private CardUI DrawPlayerCard()
    {
        // Deck is empty, force shuffle.
        if (_deckManager.CurrentDeck.Count == 0)
        {
            Debug.LogWarning("Deck is empty!");
            _deckManager.Shuffle();
            return null;
        }

        // Create a new card instance and apply to hand.
        CardData drawnCard = _deckManager.GetTopCard();

        CardUI card = ObjectPooler.SpawnFromPool("Card", _cardGroup.transform.position, Quaternion.identity).GetComponent<CardUI>();
        card.Initialize(drawnCard);
        card.transform.SetParent(_cardGroup, false);

        _currentHand.Add(drawnCard);
        _currentHandUI.Add(card);

        ValidadeCard(_actor.Stats.CurrentActions, _actor.Positioning.CurrentPosition, card);

        return card;
    }

    private IEnumerator EnumeratorDrawCards(int quantity)
    {
        // Get the desired position for the card to spawn and where the deck is located.
        Vector3 pos = new Vector3(_cardGroup.anchoredPosition.x + _startingAnchoredX, 0f, 0f);
        Vector3 deckPos = UAnchoredPositions.GetAnchoredPositionFromWorld(_deckUi, _cardGroup, _mainCanvas);

        // Loop through the number of cards to draw, waiting for the animation to finish before drawing the next one.
        for (int i = 0; i < quantity; i++)
        {
            if (i > 0)
                pos += new Vector3(_anchoredOffSet, 0f, 0f);

            DrawPlayerCard().Animations.PlayDrawAnimation(deckPos, pos);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void DiscardCard(CardUI Card)
    {
        _currentHand.Remove(Card.CardData);
        _deckManager.DiscardPile.Add(Card.CardData);
        _currentHandUI.Remove(Card);

        Card.gameObject.SetActive(false);
    }

    public void DiscardCard(CardData Card)
    {
        // Remove the card from the hand and add it to the discard pile.
        _currentHand.Remove(Card);
        _deckManager.DiscardPile.Add(Card);
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
