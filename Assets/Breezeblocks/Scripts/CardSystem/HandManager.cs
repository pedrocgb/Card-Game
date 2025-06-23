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
    public List<CardUI> CurrentHandUI => _currentHandUI;

    private List<CardPreview> _currentEnemyHandUI = new List<CardPreview>();

    // Components
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private RectTransform _deckUi = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private CanvasGroup _cardGroup = null;
    private RectTransform _cardGroupRect = null;
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
        if (_cardGroup != null)
            _cardGroupRect = _cardGroup.GetComponent<RectTransform>();
    }
    #endregion

    // ========================================================================

    /// <summary>
    /// Draw a number of cards from the deck, add it to current hand and animate them.
    /// </summary>
    /// <param name="Quantity"></param>
    public void DrawCards(int Quantity)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (_actor is PlayerActor)
            StartCoroutine(DrawPlayerCardAnimation(Quantity));
        else
            StartCoroutine(DrawEnemyCardAnimation(Quantity));
    }

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
            foreach (var ui in _currentEnemyHandUI)
                ui.gameObject.SetActive(false);
            _currentHand.Clear();
            _currentEnemyHandUI.Clear();
        }

        // Update actor Deck and Discard Pile UI
        if (_actor.IsMyTurn)
            ActorsUI.UpdateCardsInterface(_deckManager.CurrentDeck.Count, _deckManager.DiscardPile.Count, _deckManager.ConsumedPile.Count);
    }

    public void ShowHand()
    {
        _cardGroupRect.anchoredPosition = _visiblePosition;
    }
    public void HideHand()
    {
        _cardGroupRect.anchoredPosition = _hiddenPosition;
    }

    // ========================================================================

    #region Enemy Draw Methods
    private IEnumerator DrawEnemyCardAnimation(int quantity)
    {
        // 1) draw logic + spawn all enemy UIs immediately
        var drawnEnemyUIs = new List<CardPreview>();
        for (int i = 0; i < quantity; i++)
        {
            // pull the model‐card
            if (_deckManager.CurrentDeck.Count == 0)
                _deckManager.ReshuffleDiscardIntoDeck();
            var drawn = _deckManager.GetTopCard();
            _currentHand.Add(drawn);

            // spawn the visual
            var go = ObjectPooler
                .SpawnFromPool("Enemy Card UI", _cardGroup.transform.position, Quaternion.identity);
            var ui = go.GetComponent<CardPreview>();
            ui.Initialize(drawn);
            ui.transform.SetParent(_cardGroupRect, false);
            _currentEnemyHandUI.Add(ui);

            drawnEnemyUIs.Add(ui);
        }

        // 2) build the same fade+pop sequence
        var seq = DOTween.Sequence();
        for (int i = 0; i < drawnEnemyUIs.Count; i++)
        {
            var ui = drawnEnemyUIs[i];
            ui.CanvasGroup.alpha = 0f;
            ui.transform.localScale = Vector3.one * 0.9f;

            seq.AppendInterval(UConstants.CARD_DRAW_FADE_STAGGER);
            seq.Append(ui.CanvasGroup
                .DOFade(1f, UConstants.CARD_DRAW_FADE_DURATION)
                .SetEase(Ease.Linear));
            seq.Join(ui.transform
                .DOScale(1f, UConstants.CARD_DRAW_FADE_DURATION)
                .SetEase(Ease.OutBack));
        }

        seq.OnComplete(() =>
        {
            // once the enemy cards are in, rebuild layout
            RebuildEnemyHandLayout();
        });

        seq.Play();
        RebuildEnemyHandLayout();
        yield return seq.WaitForCompletion();
    }

    private IEnumerator PlaySingleEnemyDrawAnimation(CardPreview ui)
    {
        // Prepare start state
        ui.CanvasGroup.alpha = 0f;
        ui.transform.localScale = Vector3.one * 0.9f;

        // Build sequence
        Sequence seq = DOTween.Sequence()
            .AppendInterval(UConstants.CARD_DRAW_FADE_STAGGER)
            .Append(ui.CanvasGroup
                .DOFade(1f, UConstants.CARD_DRAW_FADE_DURATION)
                .SetEase(Ease.Linear))
            .Join(ui.transform
                .DOScale(1f, UConstants.CARD_DRAW_FADE_DURATION)
                .SetEase(Ease.OutBack));

        // On complete, re-enable hover and rebuild
        seq.OnComplete(() =>
        {
            RebuildEnemyHandLayout();
        });

        seq.Play();
        yield return seq.WaitForCompletion();
    }

    private void OnCardUse(CardInstance Card)
    {
        _currentHand.Remove(Card);
        if (Card.ConsumeCard)
            _deckManager.ConsumedPile.Add(Card);
        else
            _deckManager.DiscardPile.Add(Card);

        if (_actor.IsMyTurn)
            ActorsUI.UpdateCardsInterface(_deckManager.CurrentDeck.Count, _deckManager.DiscardPile.Count, _deckManager.ConsumedPile.Count);
    }

    public void DiscardEnemyCard(CardInstance cardInstance)
    {
        // 1) Find UI index before backend removes it
        int idx = _currentHand.IndexOf(cardInstance);
        if (idx < 0) return;
        var ui = _currentEnemyHandUI[idx];

        // 2) Call existing backend to remove from _currentHand and move to discard/consume
        OnCardUse(cardInstance);

        // 3) Compute discard pile canvas position
        Vector2 discardPos = UAnchoredPositions.WorldToCanvasPosition(_mainCanvas, transform.position);

        // 4) Animate UI
        var seq = DOTween.Sequence()
            .Append(ui.MyRectTransform.DOAnchorPos(discardPos, 0.4f).SetEase(Ease.InCubic))
            .Join(ui.MyRectTransform.DOSizeDelta(new Vector2(50, 50), 0.4f).SetEase(Ease.InBack))
            .Join(ui.CanvasGroup.DOFade(0f, 0.4f));

        seq.OnComplete(() =>
        {
            ui.gameObject.SetActive(false);
            _currentEnemyHandUI.RemoveAt(idx);
            RebuildEnemyHandLayout();
        });

        seq.Play();
    }

    private void RebuildEnemyHandLayout()
    {
        _cardGroup.blocksRaycasts = false;
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < _currentEnemyHandUI.Count; i++)
        {
            var ui = _currentEnemyHandUI[i];
            var rt = ui.MyRectTransform;
            float targetX = UConstants.CARD_DRAW_START_ANCHORED_X
                          + i * UConstants.CARD_DRAW_ANCHORED_OFFSET;
            Vector2 pos = new Vector2(targetX, 0f);

            rt.DOKill();
            rt.DOAnchorPos(pos, 0.25f)
              .SetEase(Ease.OutCubic)
              .OnComplete(() => {
                  _cardGroup.blocksRaycasts = true;
              });

            if (rt != null)
                rt.SetSiblingIndex(i);
        }
    }
    #endregion

    // ========================================================================

    #region Player Card Draw Methods
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
        CardUI card = ObjectPooler.SpawnFromPool("Card", _cardGroupRect.position, Quaternion.identity).GetComponent<CardUI>();
        card.Initialize(drawnCard);
        card.transform.SetParent(_cardGroupRect, false);

        _currentHandUI.Add(card);

        // Validate if cards are playable, unplayable cards are greyout;
        ValidateCard(card, _actor);

        // Update Actor UI
        if (_actor.IsMyTurn)
            ActorsUI.UpdateCardsInterface(_deckManager.CurrentDeck.Count, _deckManager.DiscardPile.Count, _deckManager.ConsumedPile.Count);

        return card;
    }
    /// <summary>
    /// Draw Cards UI animation method.
    /// </summary>
    /// <param name="quantity"></param>
    /// <returns></returns>
    private IEnumerator DrawPlayerCardAnimation(int quantity)
    {
        foreach (var cardUI in _currentHandUI)
        {
            cardUI.HoverHandler.EnableHover(false);
        }

        // 1) First spawn & layout all cards immediately, collect them
        var drawnUIs = new List<CardUI>();
        for (int i = 0; i < quantity; i++)
        {
            CardUI ui = DrawPlayerCard();
            if (ui == null) yield break;

            // assign slot index and rebuild positions
            int slot = _currentHandUI.IndexOf(ui);
            ui.SlotIndex = slot;
          

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
        }

        seq.OnComplete(() =>
        {
            // Re‐enable hover on every drawn card
            foreach (var ui in drawnUIs)
            {
                ui.HoverHandler.EnableHover(true);
                ui.HoverHandler.CaptureOriginalTransform();
            }
        });

        seq.Play();
        RebuildHandLayout();
        yield return seq.WaitForCompletion();
    }

    /// <summary>
    /// Plays fade+pop animation on one drawn card, then rebuilds layout and re-enables hover.
    /// </summary>
    private IEnumerator PlaySinglePlayerDrawAnimation(CardUI ui)
    {
        // Prepare start state
        ui.HoverHandler.EnableHover(false);
        ui.HoverHandler.CaptureOriginalTransform();
        ui.CanvasGroup.alpha = 0f;
        ui.transform.localScale = Vector3.one * 0.9f;

        // Build sequence
        Sequence seq = DOTween.Sequence()
            .AppendInterval(UConstants.CARD_DRAW_FADE_STAGGER)
            .Append(ui.CanvasGroup
                .DOFade(1f, UConstants.CARD_DRAW_FADE_DURATION)
                .SetEase(Ease.Linear))
            .Join(ui.transform
                .DOScale(1f, UConstants.CARD_DRAW_FADE_DURATION)
                .SetEase(Ease.OutBack));

        // On complete, re-enable hover and rebuild
        seq.OnComplete(() =>
        {
            ui.HoverHandler.EnableHover(true);
            ui.HoverHandler.CaptureOriginalTransform();
            RebuildHandLayout();
        });

        seq.Play();
        yield return seq.WaitForCompletion();
    }

    public void RebuildHandLayout()
    {
        _cardGroup.blocksRaycasts = false;
        Canvas.ForceUpdateCanvases();

        foreach (var cardUI in _currentHandUI)
        {
            cardUI.HoverHandler.EnableHover(false);
        }


        for (int i = 0; i < _currentHandUI.Count; i++)
        {
            var cardUI = _currentHandUI[i];
            var rt = cardUI.MyRectTransform;

            float targetX = UConstants.CARD_DRAW_START_ANCHORED_X
                          + i * UConstants.CARD_DRAW_ANCHORED_OFFSET;
            Vector2 targetPos = new Vector2(targetX, 0f);

            rt.DOKill();
            rt.DOAnchorPos(targetPos, 0.25f).SetEase(Ease.OutCubic)
              .OnComplete(() =>
              {
                  // Re‐enable hover only after this card finishes sliding
                  _cardGroup.blocksRaycasts = true;
                  cardUI.HoverHandler.CaptureOriginalTransform();
              });

            rt.SetSiblingIndex(i);
        }
    }

    /// <summary>
    /// Discard Card. UI method, for player actors only.
    /// </summary>
    /// <param name="Card"></param>
    public void DiscardCard(CardUI Card)
    {
        // 1) Compute where the discard pile is in canvas-space (unchanged)
        Vector2 discardPos = UAnchoredPositions
            .WorldToCanvasPosition(_mainCanvas, transform.position);

        // 2) Start a DOTween sequence to move/scale/fade the card away
        Sequence seq = DOTween.Sequence();

        // (a) Move the card to the discard location
        seq.Append(Card.MyRectTransform
            .DOAnchorPos(discardPos, 0.4f)
            .SetEase(Ease.InCubic)
        );

        // (b) Shrink + fade out at the same time
        seq.Join(Card.MyRectTransform
            .DOSizeDelta(new Vector2(50, 50), 0.4f)
            .SetEase(Ease.InBack)
        );
        seq.Join(Card.CanvasGroup
            .DOFade(0f, 0.4f)
        );

        // 3) When the animation is done, deactivate the GameObject,
        //    **remove it from _currentHandUI**, and rebuild the layout.
        seq.OnComplete(() => {
            Card.gameObject.SetActive(false);

            // ---- NEW: remove the CardUI from the UI list BEFORE relayout ----
            _currentHandUI.Remove(Card);

            // Now animate the remaining cards into their new slots
            RebuildHandLayout();
        });

        // 4) Meanwhile, update your “model” list immediately:
        _currentHand.Remove(Card.CardInstance);

        // 5) Add the card to the discard pile or consume pile.
        if (Card.CardInstance.ConsumeCard)
            _deckManager.ConsumedPile.Add(Card.CardInstance);
        else
            _deckManager.DiscardPile.Add(Card.CardInstance);

        if (_actor.IsMyTurn)
            ActorsUI.UpdateCardsInterface(
                _deckManager.CurrentDeck.Count,
                _deckManager.DiscardPile.Count,
                _deckManager.ConsumedPile.Count
            );
    }
    #endregion

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
    private IEnumerator LockRandomCardsCoroutine(int amount)
    {
        yield return new WaitForSeconds(0.5f); // to allow any previous animations to finish

        if (_actor.Stats.IsDead)
        {
            Console.Log("Cannot lock cards, actor is dead.");
            yield break;
        }

        var unlockedCards = _currentHand
            .Where(c => !c.IsLocked)
            .ToList();

        Console.Log($"Locking {amount} random cards from {unlockedCards.Count} unlocked cards.");

        if (unlockedCards.Count == 0)
        {
            Console.Log("No unlocked cards to lock.");
            yield break;
        }

        for (int i = 0; i < unlockedCards.Count; i++)
        {
            int j = Random.Range(i, unlockedCards.Count);
            (unlockedCards[i], unlockedCards[j]) = (unlockedCards[j], unlockedCards[i]);
        }

        int toLock = Mathf.Min(amount, unlockedCards.Count);
        for (int k = 0; k < toLock; k++)
        {
            Console.Log($"Locking card: {unlockedCards[k].CardName}");
            var instance = unlockedCards[k];
            instance.LockCard(true);

            if (_actor is PlayerActor)
            {
                var ui = _currentHandUI
                    .FirstOrDefault(c => c.CardInstance == instance);
                if (ui != null)
                {
                    Console.Log($"Enabling lock effect for card UI: {ui.CardInstance.CardName}");
                    ui.EnableCardLockEffect(true);
                }
            }
            else
            {
                var ui = _currentEnemyHandUI
                    .FirstOrDefault(c => c.CardInstance == instance);
                if (ui != null)
                {
                    Console.Log($"Enabling lock effect for enemy card UI: {ui.CardInstance.CardName}");
                    ui.EnableCardLockEffect(true);
                }
            }
        }
    }

    public void LockRandomCards(int amount)
    {
        StartCoroutine(LockRandomCardsCoroutine(amount));   
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

    public void AddCardToHand(CardData Card, int Amount)
    {
        if (_actor is PlayerActor)
        {
            for (int i = 0; i < Amount; i++)
            {
                if (_currentHand.Count >= _actor.Data.MaxHandSize)
                {
                    Debug.LogWarning($"Tried to create card in hand for {_actor.ActorName} but its hand is full, card is added to deck instead.");
                    _deckManager.AddCardToDrawPile(Card);
                    return;
                }

                CardInstance newCard = new CardInstance(Card);
                _currentHand.Add(newCard);

                // Create a new card instance UI
                CardUI card = ObjectPooler.SpawnFromPool("Card", _cardGroupRect.position, Quaternion.identity).GetComponent<CardUI>();
                card.Initialize(newCard);
                card.transform.SetParent(_cardGroupRect, false);

                _currentHandUI.Add(card);

                // Validate if cards are playable, unplayable cards are greyout;
                ValidateCard(card, _actor);

                // Update Actor UI
                if (_actor.IsMyTurn)
                {
                    ActorsUI.UpdateCardsInterface(_deckManager.CurrentDeck.Count, _deckManager.DiscardPile.Count, _deckManager.ConsumedPile.Count);
                    StartCoroutine(PlaySinglePlayerDrawAnimation(card));
                }
            }
        }
        else
        {
            for (int i = 0; i < Amount; i++)
            {
                if (_currentHand.Count >= _actor.Data.MaxHandSize)
                {
                    Debug.LogWarning($"Tried to create card in hand for {_actor.ActorName} but its hand is full, card is added to deck instead.");
                    _deckManager.AddCardToDrawPile(Card);
                    return;
                }

                CardInstance newCard = new CardInstance(Card);
                _currentHand.Add(newCard);

                // Create a new card instance UI
                CardPreview card = ObjectPooler.SpawnFromPool("Enemy Card UI", _cardGroupRect.position, Quaternion.identity).GetComponent<CardPreview>();
                card.Initialize(newCard);
                card.transform.SetParent(_cardGroupRect, false);

                _currentEnemyHandUI.Add(card);

                // Update Actor UI
                if (_actor.IsMyTurn)
                {
                    ActorsUI.UpdateCardsInterface(_deckManager.CurrentDeck.Count, _deckManager.DiscardPile.Count, _deckManager.ConsumedPile.Count);
                    StartCoroutine(PlaySingleEnemyDrawAnimation(card));
                }
            }
        }
    }
    #endregion

    // ========================================================================
}
