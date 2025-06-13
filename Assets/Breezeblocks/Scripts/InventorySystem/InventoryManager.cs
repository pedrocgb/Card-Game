using System.Collections.Generic;
using System.Linq;
using Breezeblocks.Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    #region Variables and Properties
    private static InventoryManager Instance = null;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private GameObject _inventoryPanel = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _actorNameText = null;
    [FoldoutGroup("Components/Cards", expanded: true)]
    [SerializeField] private RectTransform _cardsSlidesContainer = null;
    [FoldoutGroup("Components/Cards", expanded: true)]
    [SerializeField] private CardPreview _cardPreview = null;
    private List<CardSlideUI> _spawnedCards = new List<CardSlideUI>();

    [FoldoutGroup("Components/Portraits", expanded: true)]
    [SerializeField] private Transform _portraitContainer = null;
    private List<InventoryActorPortrait> _spawnedPortraits = new List<InventoryActorPortrait>();

    private bool _portraitsSpawned = false;
    private ActorManager _lastActor = null;
    #endregion

    // ========================================================================

    #region Initialization Methods
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // Ensure the inventory panel is hidden at start
        _inventoryPanel.SetActive(false);
    }
    #endregion

    // ========================================================================

    #region Static Methods
    public static void UpdateCardPreview(CardInstance Card)
    {
        if (Instance == null) return;

        Instance.updateCardPreview(Card);
    }

    public static  void SelectPortrait(ActorManager actor)
    {
        if (Instance == null) return;
        Instance.selectPortrait(actor);
    }
    #endregion

    // ========================================================================

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);
            if (!_portraitsSpawned)
                InstantiatePortraits();
            else
                ReorderPortraits();
        }
    }

    // ========================================================================

    private void InstantiatePortraits()
    {
        // tear down previous
        foreach (var ui in _spawnedCards)
            ui.gameObject.SetActive(false);
        _spawnedCards.Clear();

        // 1) hide/clear any previous portraits
        foreach (Transform child in _portraitContainer)
            child.gameObject.SetActive(false);

        // 2) grab only the alive player actors, sorted by their enum value
        var sorted = CombatManager.Instance.PlayerActors
                        .Where(a => !a.Stats.IsDead)
                        .OrderBy(a => a.Positioning.CurrentPosition)  // casts enum to its underlying int
                        .ToList();

        // 3) spawn in that order
        foreach (var actor in sorted)
        {
            var p = ObjectPooler
                       .SpawnFromPool("Inventory Actor Portrait",
                                      _portraitContainer.position,
                                      Quaternion.identity)
                       .GetComponent<InventoryActorPortrait>();

            p.transform.SetParent(_portraitContainer, worldPositionStays: false);
            p.Initialize(actor);

            _spawnedPortraits.Add(p);
        }

        InstantiateCards(sorted[0]);
        _portraitsSpawned = true;
    }

    private void ReorderPortraits()
    {
        foreach (var p in _spawnedPortraits)
        {
            if (p.Actor.Stats.IsDead)
            {
                p.gameObject.SetActive(false);
                continue; // skip dead actors
            }

            p.transform.SetSiblingIndex((int)p.Actor.Positioning.CurrentPosition);
        }
    }   

    public void InstantiateCards(ActorManager actor)
    {
        if ( actor == _lastActor)
            return;

        _lastActor = actor;
        _actorNameText.text = actor.ActorName + "'s Deck";

        // tear down previous
        foreach (var ui in _spawnedCards)
            ui.gameObject.SetActive(false);
        _spawnedCards.Clear();

        // sort ascending by cost, then spawn
        var sorted = actor.Deck.MainDeck
                         .OrderBy(c => c.ActionCost)      // or c.ActionCost
                         .ToList();

        foreach (var card in sorted)
        {
            var ui = ObjectPooler
                .SpawnFromPool("Card Slide UI", _cardsSlidesContainer.position, Quaternion.identity)
                .GetComponent<CardSlideUI>();

            ui.transform.SetParent(_cardsSlidesContainer, worldPositionStays: false);
            ui.Initialize(card);
            _spawnedCards.Add(ui);
        }

        updateCardPreview(_spawnedCards[0].Card);
    }

    // ========================================================================

    #region Local Methods
    public void selectPortrait(ActorManager actor)
    {
         InstantiateCards(actor);
    }

    public void updateCardPreview(CardInstance card)
    {
        _cardPreview.Initialize(card);
    }
    #endregion

    // ========================================================================
}
