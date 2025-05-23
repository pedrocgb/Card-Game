using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    #region Variables and Properties
    public static CombatManager Instance { get; private set; }

    // Combatents
    [FoldoutGroup("Combatents", expanded:true)]
    [SerializeField]
    private List<ActorManager> _combatents = new List<ActorManager>();


    // Actors
    private ActorManager _currentCombatent = null;
    private List<PlayerActor> _playerActors = new List<PlayerActor>();
    public List<PlayerActor> PlayerActors => _playerActors;
    private List<EnemyActor> _enemyActors = new List<EnemyActor>();
    public List<EnemyActor> EnemyActors => _enemyActors;


    // Turn and round
    private int _currentRound = 1;
    private int _currentTurnIndex = 0;
    private bool _firstRound = true;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    private void Start()
    {
        RollInitiative();

        _enemyActors = _combatents.OfType<EnemyActor>().ToList();
        _playerActors = _combatents.OfType<PlayerActor>().ToList();
    }
    #endregion

    // ========================================================================

    #region Static Methods
    public static void RemoveCombatent(ActorManager Combatent)
    {
        if (Instance == null)
            return;

        Instance.removeCombatent(Combatent);
    }
    #endregion

    // ========================================================================

    #region Combat Management Methods
    private void RollInitiative()
    {
        // Roll initiative and reorder the list to fit the order.
        foreach (ActorManager c in _combatents)
        {
            int roll = Random.Range(1, 11);
            c.CurrentInitiative = roll + c.InitiativeBonus;

            Debug.Log($"{c.name} rolled {roll} + {c.InitiativeBonus} = {c.CurrentInitiative}");
        }

        _combatents = _combatents.OrderByDescending(c => c.CurrentInitiative).ToList();


        // DEBUGGING
        // -------------------------------------------------
        Debug.Log("Initiative Rolls:");
        foreach (ActorManager c in _combatents)
        {
            Debug.Log($"{c.name}: {c.CurrentInitiative}");
        }

        Invoke(nameof(StartRound), 2f);
        // -------------------------------------------------
    }

    private void StartRound()
    {
        // Reset turn index each roud.
        _currentTurnIndex = 0;
        NewTurn();
    }

    private void NewTurn()
    {
        // Check if all combatents have played their turn.
        if (_currentTurnIndex >= _combatents.Count)
        {
            _currentRound++;
            if (_firstRound)
                _firstRound = false;

            StartRound();
            Debug.Log($"=========== START NEW ROUND - ROUND {_currentRound} ============");

            return;
        }

        // Set this turn's combatent ability to ACT.
        _currentCombatent = _combatents[_currentTurnIndex];
        _currentCombatent.StartNewTurn();
        Debug.Log($"Starting turn for {_currentCombatent.name} in round {_currentRound}");

        // If the combatent is a player, show the UI and initialize its turn.
        if (_currentCombatent is PlayerActor)            
            PlayerTurnManager.Instance.StartPlayerTurn(_currentCombatent as PlayerActor);
        else if (_currentCombatent is EnemyActor)
            EnemyTurnManager.Instance.StartEnemyTurn(_currentCombatent as EnemyActor);

        // Draw cards for the combatent.
        _currentCombatent.Hand.DrawCards(_currentCombatent.Stats.CardBuy);        
    }

    public void EndTurn()
    {
        Debug.Log($"Ending turn for {_currentCombatent.name}");

        _currentTurnIndex++;

        NewTurn();
    }
    #endregion

    // ========================================================================

    #region Combatents Management Methods
    private void removeCombatent(ActorManager combatent)
    {
        // Remove the combatent from the list.
        _combatents.Remove(combatent);
        // If the combatent is a player, remove it from the player list.
        if (combatent is PlayerActor)
        {
            _playerActors.Remove(combatent as PlayerActor);
        }
        else if (combatent is EnemyActor)
        {
            _enemyActors.Remove(combatent as EnemyActor);
        }

        //// Check if all combatents have played their turn.
        //if (_currentTurnIndex >= _combatents.Count)
        //{
        //    _currentRound++;
        //    if (_firstRound)
        //        _firstRound = false;
        //    StartRound();
        //    Debug.Log($"=========== START NEW ROUND - ROUND {_currentRound} ============");
        //}
    }
    #endregion

    // ========================================================================
}
