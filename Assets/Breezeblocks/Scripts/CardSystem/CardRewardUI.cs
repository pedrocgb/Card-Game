using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Breezeblocks.Managers;
using Sirenix.OdinInspector;
using System.Collections;

public class CardRewardUI : MonoBehaviour
{
    #region Variables and Properties
    public static CardRewardUI Instance;

    [FoldoutGroup("Components")]
    [SerializeField]
    private GameObject _rewardsPanel = null;
    [FoldoutGroup("Components")]
    [SerializeField]
    private GameObject _mapUi = null;

    [FoldoutGroup("Components")]
    [SerializeField]
    private RectTransform _rewardsContainer;

    [FoldoutGroup("Components")]
    [SerializeField]
    private Button _continueButton = null;

    [FoldoutGroup("Layout")]
    [SerializeField, Tooltip("Starting position for the first card in each portrait")]
    private Vector2 _startCardPos = Vector2.zero;

    [FoldoutGroup("Layout")]
    [SerializeField, Tooltip("Horizontal spacing between cards in each portrait")]
    private float _cardSpacing = 150f;

    // Tracks whether each actor has completed selection or skip
    private Dictionary<PlayerActor, bool> _actorCompletion = new Dictionary<PlayerActor, bool>();
    #endregion

    // ========================================================================

    void Awake()
    {
        Instance = this;
        // Ensure continue is disabled initially
        if (_continueButton != null)
            _continueButton.interactable = false;
    }

    // ========================================================================

    #region UI Methods
    /// <summary>
    /// Call this once combat ends.
    /// </summary>
    public void ShowRewards(List<PlayerActor> actors)
    {
        StartCoroutine(ShowRewardsCoroutine(actors));
    }

    private IEnumerator ShowRewardsCoroutine(List<PlayerActor> actors)
    {
        _rewardsPanel.SetActive(true);

        // Sort actors by their current world position enum
        var sortedActors = actors
            .OrderBy(a => (int)a.Positioning.CurrentPosition)
            .ToList();

        // Initialize completion state
        _actorCompletion.Clear();
        foreach (var actor in sortedActors)
        {
            // Dead actors are already complete
            bool isComplete = actor.Stats.IsDead;
            _actorCompletion[actor] = isComplete;
        }
        UpdateContinueButtonState();

        // Clear previous children
        foreach (Transform t in _rewardsContainer)
        {
            Debug.Log($"Clearing child: {t.name}");
            t.SetParent(null, false);
            t.gameObject.SetActive(false);
        }

        // Spawn portrait slots with manual sibling index
        int portraitIndex = 0;
        foreach (var actor in sortedActors)
        {
            // Spawn or re-activate portrait
            var slot = ObjectPooler.SpawnFromPool(
                "Card Reward Portrait",
                _rewardsContainer.position,
                Quaternion.identity);
            slot.transform.SetParent(_rewardsContainer, false);
            slot.transform.SetSiblingIndex(portraitIndex);
            portraitIndex++;

            slot.transform.SetParent(_rewardsContainer, false);
            var portrait = slot.GetComponent<CardRewardPortrait>();
            portrait.Initialize(
                actor.Data.Portrait,
                actor.ActorName,
                actor.Data.ActorRace.RaceName,
                actor.Data.ActorSpecialization?.SpecializationName,
                actor.Deck.CurrentDeck.Count,
                actor);
            var btn = portrait.SkipRewardButton;

            if (actor.Stats.IsDead)
            {
                btn.interactable = false;
                continue;
            }

            // 2) generate reward cards
            var cards = CardRewardManager.Instance.GenerateRewardsForActor(actor);

            // 3) spawn reward-card views
            // Spawn with manual sibling index and anchored positions
            var views = new List<CardRewardView>();
            var rowRoot = portrait.CardsContainer;
            float cardX = _startCardPos.x;
            for (int i = 0; i < cards.Count; i++)
            {
                var cd = cards[i];
                var go = ObjectPooler.SpawnFromPool(
                    "Card Reward UI",
                    rowRoot.position,
                    Quaternion.identity);
                go.transform.SetParent(rowRoot, false);
                go.transform.SetSiblingIndex(i);

                var rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(cardX, _startCardPos.y);
                cardX += _cardSpacing;

                var view = go.GetComponent<CardRewardView>();
                view.Initialize(cd, selectedView =>
                    OnCardSelected(actor, selectedView, views));
                views.Add(view);
            }
            // 4) wire up ignore button
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnIgnore(actor, views));

            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion

    // ========================================================================

    #region Event Handlers
    public void OnCardSelected(PlayerActor actor, CardRewardView selected, List<CardRewardView> allViews)
    {
        // Disable skip button so they can't ignore after picking
        var portrait = _rewardsContainer
            .GetComponentsInChildren<CardRewardPortrait>()
            .First(p => p.MyActor == actor);
        portrait.SkipRewardButton.interactable = false;

        // Mark actor complete and update continue button
        _actorCompletion[actor] = true;
        UpdateContinueButtonState();

        // Add the card to their deck
        actor.Deck.AddCard(selected.GetCardInstance());

        // Flip & fade away other cards
        foreach (var v in allViews)
            if (v != selected)
                v.FlipToBackAndFade();

        // Shrink & fade the chosen card
        selected.ShrinkAndFade();
    }

    private void OnIgnore(PlayerActor actor, List<CardRewardView> views)
    {
        // Mark actor complete and update continue button
        _actorCompletion[actor] = true;
        UpdateContinueButtonState();

        // Flip & fade all cards
        foreach (var v in views)
            v.FlipToBackAndFade();
    }

    private void UpdateContinueButtonState()
    {
        if (_continueButton == null) return;
        // Enable when all actors (alive only) have completed
        bool allDone = _actorCompletion.Values.All(done => done);
        _continueButton.interactable = allDone;
    }

    public void EndCombatReward()
    {
        _rewardsPanel.SetActive(false);

        foreach (Transform child in _rewardsContainer)
        {
            Debug.Log($"Clearing child: {child.name}");
            child.gameObject.SetActive(false);
        }

        _mapUi.SetActive(true);
        MapVisualizer.Instance.CompleteEvent();
    }
    #endregion

    // ========================================================================
}
