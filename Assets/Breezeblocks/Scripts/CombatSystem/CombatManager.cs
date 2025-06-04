using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    #region Variables and Properties
    public static CombatManager Instance { get; private set; }

    // Combatents
    private List<ActorManager> _combatents = new List<ActorManager>();
    private List<ActorManager> _turnOrder = new List<ActorManager>();


    // Actors
    private ActorManager _currentCombatent = null;
    public ActorManager CurrentCombatent => _currentCombatent;

    private List<PlayerActor> _playerActors = new List<PlayerActor>();
    public List<PlayerActor> PlayerActors => _playerActors;

    private List<EnemyActor> _enemyActors = new List<EnemyActor>();
    public List<EnemyActor> EnemyActors => _enemyActors;

    public bool IsPlayerTurn
    {
        get
        {
            if (_currentCombatent is PlayerActor)
                return true;
            else
                return false;
        }
    }


    // Turn and round
    private int _currentRound = 1;
    private int _currentTurnIndex = 0;
    private bool _firstRound = true;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    [InfoBox("All enemies actors in the world should be assigned to this list in the editor, the system will feed them and activate/deactivate" +
        "them on demmand based on the map nodes.", InfoMessageType.Warning)]
    private List<GameObject> _persistentEnemiesObjects = new List<GameObject>();
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        if (Instance == null) Instance = this;

        var allActors = FindObjectsByType<ActorManager>(FindObjectsSortMode.None);
        _combatents = new List<ActorManager>(allActors);
    }
    private void Start()
    {
        _enemyActors = _combatents.OfType<EnemyActor>().ToList();
        _playerActors = _combatents.OfType<PlayerActor>().ToList();

       // Invoke("StartRound", 2f);
    }
    #endregion

    // ========================================================================

    #region Static Methods
    public static void StartRound()
    {
        if (Instance == null)
            return;
        Instance.startRound();
    }

    public static void CreateCombatent(List<ActorData> NewEnemyParty)
    {
        if (Instance == null)
            return;
        Instance.createCombatent(NewEnemyParty);
    }

    public static void RemoveCombatent(ActorManager Combatent)
    {
        if (Instance == null)
            return;

        Instance.removeCombatent(Combatent);
    }

    public static void EndCombat()
    {
        if (Instance == null)
            return;
        Instance.endCombat();
    }
    #endregion

    // ========================================================================

    #region Combat Management Methods
    private void RollInitiative()
    {
        _turnOrder.Clear();

        foreach (var actor in _combatents.Where(a => !a.Stats.IsDead))
        {
            actor.RollInitiative();
            _turnOrder.Add(actor);
        }

        _turnOrder = _turnOrder.OrderByDescending(a => a.CurrentInitiative).ToList(); 
    }

    private void NewTurn()
    {
        // Check if all combatents have played their turn.
        if (_currentTurnIndex >= _turnOrder.Count)
        {
            _currentRound++;
            if (_firstRound)
                _firstRound = false;

            startRound();
            Debug.Log($"=========== START NEW ROUND - ROUND {_currentRound} ============");

            return;
        }

        // Set this turn's combatent ability to ACT.
        _currentCombatent = _turnOrder[_currentTurnIndex];

        Debug.Log($"Starting turn for {_currentCombatent.name} in round {_currentRound}");

        // If the combatent is a player, show the UI and initialize its turn.
        if (_currentCombatent is PlayerActor)            
            PlayerTurnManager.Instance.StartPlayerTurn(_currentCombatent as PlayerActor);
        else if (_currentCombatent is EnemyActor)
            EnemyTurnManager.Instance.StartEnemyTurn(_currentCombatent as EnemyActor);     
    }

    public void EndTurn()
    {
        Debug.Log($"Ending turn for {_currentCombatent.name}");

        _currentTurnIndex++;
        _currentCombatent.EndTurn();

        TurnOrderUI.Instance.AdvanceTurn();

        NewTurn();
    }

    private void UpdateCombatents()
    {
        var allActors = FindObjectsByType<ActorManager>(FindObjectsSortMode.None);
        _combatents = new List<ActorManager>(allActors);

        _enemyActors = _combatents.OfType<EnemyActor>().ToList();
        _playerActors = _combatents.OfType<PlayerActor>().ToList();
    }
    #endregion

    // ========================================================================

    #region Combatents Management Methods
    private void startRound()
    {
        // Reset turn index each roud.
        _currentTurnIndex = 0;

        RollInitiative();

        TurnOrderUI.Instance.UpdateUI(_turnOrder);

        NewTurn();
    }
    private void createCombatent(List<ActorData> newEnemyParty)
    {
        for (int i = 0; i < _persistentEnemiesObjects.Count; i++)
        {
            if (i < newEnemyParty.Count)
            {
                _persistentEnemiesObjects[i].gameObject.SetActive(true);
            }
            else
            {
                _persistentEnemiesObjects[i].gameObject.SetActive(false);
            }
        }

        UpdateCombatents();
        for (int i = 0; i < newEnemyParty.Count; i++)
        {
            _enemyActors[i].InitializeEnemy(newEnemyParty[i], i + 1);
        }
    }
    private void removeCombatent(ActorManager combatent)
    {
        _turnOrder.Remove(combatent);
    }

    private void endCombat()
    {
        foreach (var actor in _combatents)
        {
            actor.Hand.DiscardHand(actor is PlayerActor);
            actor.Deck.ReshuffleDiscardIntoDeck();
            actor.Stats.UpdateAllStatusDuration();
        }
    }
    #endregion

    // ========================================================================
}
