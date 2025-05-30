using Breezeblocks.Managers;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(DeckManager))] 
public class HandManager : MonoBehaviour
{
    #region Variables and Properties
    private ActorManager _actor = null;
    private DeckManager _deckManager = null;

    private List<CardInstance> _currentHand = new List<CardInstance>();
    public List<CardInstance> CurrentHand => _currentHand;
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
            StartCoroutine(DrawCardAnimation(Quantity));
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
                Debug.LogWarning("Deck is empty! Reshuffling discard");
                _deckManager.ReshuffleDiscardIntoDeck();
                continue;
            }

            _currentHand.Add(_deckManager.GetTopCard());
        }
    }

    /// <summary>
    /// Discard Card. Basic method, no UI.
    /// </summary>
    /// <param name="Card"></param>
    public void DiscardCard(CardInstance Card)
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

        CardInstance drawnCard = _deckManager.GetTopCard();
        _currentHand.Add(drawnCard);

        // Create a new card instance UI
        CardUI card = ObjectPooler.SpawnFromPool("Card", _cardGroup.transform.position, Quaternion.identity).GetComponent<CardUI>();
        card.Initialize(drawnCard);
        card.transform.SetParent(_cardGroup, false);

        _currentHandUI.Add(card);

        // Validate if cards are playable, unplayable cards are greyout;
        ValidateCard(card, _actor);

        return card;
    }
    /// <summary>
    /// Draw Cards UI animation method.
    /// </summary>
    /// <param name="quantity"></param>
    /// <returns></returns>
    private IEnumerator DrawCardAnimation(int quantity)
    {
        // 1) First spawn & layout all cards immediately, collect them
        var drawnUIs = new List<CardUI>();
        for (int i = 0; i < quantity; i++)
        {
            CardUI ui = DrawPlayerCard();
            if (ui == null) yield break;

            // assign slot index and rebuild positions
            int slot = _currentHandUI.IndexOf(ui);
            ui.SlotIndex = slot;
            RebuildHandLayout();

            // disable hover until animation done
            ui.HoverHandler.EnableHover(false);
            ui.HoverHandler.CaptureOriginalTransform();

            drawnUIs.Add(ui);
        }

        // 2) Build one master sequence to stagger each fade+pop
        var seq = DOTween.Sequence();

        for (int i = 0; i < drawnUIs.Count; i++)
        {
            var ui = drawnUIs[i];
            // ensure start state
            ui.CanvasGroup.alpha = 0f;
            ui.transform.localScale = Vector3.one * 0.9f;

            // stagger
            seq.AppendInterval(UConstants.CARD_DRAW_FADE_STAGGER);

            // fade in
            seq.Append(ui.CanvasGroup
                .DOFade(1f, UConstants.CARD_DRAW_FADE_DURATION)
                .SetEase(Ease.Linear));
            // scale up
            seq.Join(ui.transform
                .DOScale(1f, UConstants.CARD_DRAW_FADE_DURATION)
                .SetEase(Ease.OutBack));

            // once this card’s tween is done, recapture and enable hover
            int copy = i;
            seq.AppendCallback(() =>
            {
                var finishedUi = drawnUIs[copy];
                finishedUi.HoverHandler.CaptureOriginalTransform();
                finishedUi.HoverHandler.EnableHover(true);
            });
        }

        seq.Play();
        yield return seq.WaitForCompletion();
    }

    public void RebuildHandLayout()
    {
        for (int i = 0; i < _currentHandUI.Count; i++)
        {
            var rt = _currentHandUI[i].GetComponent<RectTransform>();
            float x = _startingAnchoredX + i * _anchoredOffSet;
            rt.anchoredPosition = new Vector2(x, 0f);

            // reset sibling so the z-order always matches
            rt.SetSiblingIndex(i);

            // and let the hover handler know its new “home” spot
            _currentHandUI[i]
              .GetComponent<CardHoverHandler>()
              ?.CaptureOriginalTransform();
        }
    }

    /// <summary>
    /// Discard Card. UI method, for player actors only.
    /// </summary>
    /// <param name="Card"></param>
    public void DiscardCard(CardUI Card)
    {
        // compute discard slot in UI space
        Vector2 discardPos = UAnchoredPositions
            .WorldToCanvasPosition(_mainCanvas, transform.position);

        RectTransform rt = Card.GetComponent<RectTransform>();

        // detach from hand so it doesn't get re‐positioned by your layout
        rt.SetParent(_mainCanvas.transform, true);

        // play the fly+fade
        Sequence seq = DOTween.Sequence();
        seq.Append(rt
            .DOAnchorPos(discardPos, 0.4f)
            .SetEase(Ease.InCubic));
        seq.Join(rt
            .DOSizeDelta(new Vector2(50, 50), 0.4f)
            .SetEase(Ease.InBack));
        seq.Join(Card.CanvasGroup.DOFade(0f, 0.4f));
        seq.OnComplete(() => {
            Card.gameObject.SetActive(false);
        });

        // remove from your lists immediately
        _currentHand.Remove(Card.CardInstance);
        _deckManager.DiscardPile.Add(Card.CardInstance);
        _currentHandUI.Remove(Card);
    }
    #endregion

    /// <summary>
    /// Discard entire hand.
    /// </summary>
    /// <param name="IsPlayer"></param>
    public void DiscardHand(bool IsPlayer)
    {
        if (IsPlayer)
        {
            // Discard all cards in the hand
            foreach (CardUI cardUI in _currentHandUI)
            {
                _deckManager.DiscardPile.Add(cardUI.CardInstance);
                cardUI.gameObject.SetActive(false);
            }
            _currentHand.Clear();
            _currentHandUI.Clear();
        }
        else
        {
            // For AI or non-player actors, just clear the hand without UI
            foreach (CardInstance card in _currentHand)
            {
                _deckManager.DiscardPile.Add(card);
            }
            _currentHand.Clear();
            _currentHandUI.Clear();
        }
    }


    // ========================================================================

    #region Card Validation Methods - Player
    public bool ValidateCard(CardUI Card, ActorManager Source)
    {
        bool validate = UCardValidator.IsCardPlayable(Card.CardInstance, Source, UCardValidator.GetAllValidTargets(Card.CardInstance, Source));
        Card.Validate(validate);

        return validate;
    }

    public void ValidateHand()
    {
        if (_currentHandUI.Count <= 0)
            return;

        // Loop through the current hand and check if the cards are valid.
        for (int i = 0; i < _currentHandUI.Count; i++)
        {
            ValidateCard(_currentHandUI[i], _actor);
        }
    }
    #endregion

    // ========================================================================

    #region Effects Methods
    public void LockRandomCards(int amount)
    {
        var unlockedCards = _currentHand
            .Where(c => !c.IsLocked)
            .ToList();

        if (unlockedCards.Count == 0) return;

        for (int i = 0; i <unlockedCards.Count; i++)
        {
            int j = Random.Range(i, unlockedCards.Count);
            (unlockedCards[i], unlockedCards[j]) = (unlockedCards[j], unlockedCards[i]);
        }

        int toLock = Mathf.Min(amount, unlockedCards.Count);
        for (int k = 0; k < toLock; k++)
        {
            var instance = unlockedCards[k];
            instance.LockCard(true);

            var ui = _currentHandUI
                .FirstOrDefault(c => c.CardInstance == instance);
            if (ui != null)
                ui.EnableCardLockEffect(true);
        }
    }

    public void UnlockAllCards()
    {
        foreach (var card in _currentHand)
        {
            card.LockCard(false);
        }

        if (_currentHandUI.Count > 0)
        {
            foreach (var ui in _currentHandUI)
            {
                ui.EnableCardLockEffect(false);
            }
        } 
    }
    #endregion

    // ========================================================================
}
