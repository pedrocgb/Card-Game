using System.Collections.Generic;
using System.Linq;
using Breezeblocks.Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    #region Variables and Properties
    private static InventoryManager Instance = null;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private GameObject _inventoryPanel = null;

    [FoldoutGroup("Components/Deck", expanded: true)]
    [SerializeField] private GameObject _deckPanel = null;
    [FoldoutGroup("Components/Deck", expanded: true)]
    [SerializeField] private Transform _deckDummyPanel = null;
    [FoldoutGroup("Components/Deck", expanded: true)]
    [SerializeField] private Button _showDeckButton = null;
    [FoldoutGroup("Components/Deck", expanded: true)]
    [SerializeField] private TextMeshProUGUI _actorNameText = null;
    [FoldoutGroup("Components/Deck", expanded: true)]
    [SerializeField] private RectTransform _cardsSlidesContainer = null;
    [FoldoutGroup("Components/Deck", expanded: true)]
    [SerializeField] private CardPreview _cardPreview = null;
    private List<CardSlideUI> _spawnedCards = new List<CardSlideUI>();

    [FoldoutGroup("Components/Portraits", expanded: true)]
    [SerializeField] private Transform _portraitContainer = null;
    private List<InventoryActorPortrait> _spawnedPortraits = new List<InventoryActorPortrait>();

    [FoldoutGroup("Components/Relics", expanded: true)]
    [SerializeField] private TextMeshProUGUI _relicOwnerText = null;
    [FoldoutGroup("Components/Relics", expanded: true)]
    [SerializeField] private GameObject _relicsPanel = null;
    [FoldoutGroup("Components/Relics", expanded: true)]
    [SerializeField] private Button _showRelicButton = null;
    [FoldoutGroup("Components/Relics", expanded: true)]
    [SerializeField] private TextMeshProUGUI _relicTitleText = null;
    [FoldoutGroup("Components/Relics", expanded: true)]
    [SerializeField] private TextMeshProUGUI _relicDescriptionText = null;
    [FoldoutGroup("Components/Relics", expanded: true)]
    [SerializeField] private RelicInventoryUI[] _relicsUI = new RelicInventoryUI[UConstants.MAX_RELICS_PER_ACTOR];
    private List<RelicData> _actorRelics = new List<RelicData>();
    private RelicData _selectedRelic = null;

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

    public static  void SelectPortrait(ActorManager Actor)
    {
        if (Instance == null) return;
        Instance.selectPortrait(Actor);
    }

    public static void SelectRelic(RelicData Relic)
    {
        if (Instance == null) return;
        Instance.selectRelic(Relic);
    }
    #endregion

    // ========================================================================

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            _inventoryPanel.SetActive(!_inventoryPanel.activeSelf);
            if (!_portraitsSpawned)
            {
                InstantiatePortraits();
                UpdateRelics(_lastActor);
            }
            else
                ReorderPortraits();
        }
    }

    // ========================================================================

    #region Portraits and Cards
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
        Debug.Log("Reordering Portraits");
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


    // Local methods
    public void selectPortrait(ActorManager actor)
    {
        InstantiateCards(actor);
        UpdateRelics(actor);
    }
    #endregion

    // ========================================================================

    #region Deck Methods
    public void InstantiateCards(ActorManager actor)
    {
        if (actor == _lastActor)
            return;        

        _lastActor = actor;
        _actorNameText.text = actor.ActorName + "'s Deck";

        // tear down previous
        foreach (var ui in _spawnedCards)
        {
            ui.transform.SetParent(null, false);    
            ui.gameObject.SetActive(false);
        }
        _spawnedCards.Clear();

        // sort ascending by cost, then spawn
        var sorted = actor.Deck.MainDeck
                         .OrderBy(c => c.ActionCost)      // or c.ActionCost
                         .ToList();        

        foreach (var card in sorted)
        {
            var ui = ObjectPooler
                .SpawnFromPool("Card Slide UI", _deckDummyPanel.position, Quaternion.identity)
                .GetComponent<CardSlideUI>();

            ui.transform.SetParent(_deckDummyPanel, worldPositionStays: false);
            ui.Initialize(card);
            _spawnedCards.Add(ui);
        }

        foreach (var ui in _spawnedCards)
        {
            ui.transform.SetParent(_cardsSlidesContainer, worldPositionStays: false);
        }

        updateCardPreview(_spawnedCards[0].Card);
    }


    // Local methods
    public void updateCardPreview(CardInstance card)
    {
        _cardPreview.Initialize(card);
    }
    #endregion

    // ========================================================================

    #region Relics
    private void UpdateRelics(ActorManager actor)
    {
        _relicOwnerText.text = actor.ActorName + "'s Relics";
        _actorRelics.Clear();

        // If there are no relics, clear the UI
        if (actor == null ||
            actor.MyRelics.Relics.Count <= 0)
        {
            _relicTitleText.text = string.Empty;
            _relicDescriptionText.text = string.Empty;

            foreach (var relic in _relicsUI)
            {
                relic.Initialize();
            }

            return;
        }

        // Add relics to the UI
        for (int i = 0; i < UConstants.MAX_RELICS_PER_ACTOR; i++)
        {
            if (i < actor.MyRelics.Relics.Count)
            {
                // fill with real relic data
                var relic = actor.MyRelics.Relics[i];
                _relicsUI[i].Initialize(relic);
                _actorRelics.Add(relic);
            }
            else
            {
                // clear out any leftover UI in empty slots
                _relicsUI[i].Initialize();
            }
        }
        selectRelic(_actorRelics[0]);
    }


    // Local methods
    private void selectRelic(RelicData relic)
    {
        _selectedRelic = relic;
        _relicTitleText.text = relic.RelicName;
        _relicDescriptionText.text = relic.RelicDescription;
    }
    #endregion

    // ========================================================================

    #region Local Methods
    public void ShowRelicButton()
    {
        _deckPanel.SetActive(false);
        _relicsPanel.SetActive(true);

        _showDeckButton.interactable = true;
        _showRelicButton.interactable = false;
    }

    public void ShowDeckButton()
    {
        _relicsPanel.SetActive(false);
        _deckPanel.SetActive(true);

        _showRelicButton.interactable = true;
        _showDeckButton.interactable = false;
    }
    #endregion

    // ========================================================================
}
