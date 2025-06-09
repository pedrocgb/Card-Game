using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    #region Variables and Properties
    public static CombatManager Instance { get; private set; }

    private List<ActorManager> _combatents = new List<ActorManager>();
    private List<ActorManager> _turnOrder = new List<ActorManager>();
    public List<ActorManager> TurnOrder => _turnOrder;

    private ActorManager _currentCombatent = null;
    public ActorManager CurrentCombatent => _currentCombatent;

    private List<PlayerActor> _playerActors = new List<PlayerActor>();
    public List<PlayerActor> PlayerActors => _playerActors;

    private List<EnemyActor> _enemyActors = new List<EnemyActor>();
    public List<EnemyActor> EnemyActors => _enemyActors;

    private int _currentRound = 1;
    private int _currentTurnIndex = 0;
    private bool _firstRound = true;
    public bool IsPlayerTurn => _currentCombatent is PlayerActor;
    public bool CombatEnded { get; private set; }

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    [InfoBox("All enemy actor objects for this combat. They'll be activated/deactivated based on your map nodes.", InfoMessageType.Warning)]
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
    }
    #endregion

    // ========================================================================

    #region Static Entrypoints
    public static void StartRound() { if (Instance == null) return; Instance.startRound(); }
    public static void CreateCombatent(List<ActorData> newParty)
    { if (Instance == null) return; Instance.createCombatent(newParty); }
    public static void RemoveCombatent(ActorManager a)
    { if (Instance == null) return; Instance.removeCombatent(a); }

    public static void StartNewCombat() { if (Instance == null) return; Instance.startNewCombat(); }
    public static void EndCombat() { if (Instance == null) return; Instance.endCombat(); }
    #endregion

    // ========================================================================

    #region Combat Flow
    private void RollInitiative()
    {
        _turnOrder.Clear();
        foreach (var a in _combatents.Where(x => !x.Stats.IsDead))
        {
            a.RollInitiative();
            _turnOrder.Add(a);
            a.UI.UpdateTurnMarker(true);
        }
        _turnOrder = _turnOrder.OrderByDescending(x => x.CurrentInitiative).ToList();
        TurnOrderUI.Instance.UpdateUI(_turnOrder);
    }

    private void NewTurn()
    {
        if (CombatEnded) return;

        if (_currentTurnIndex >= _turnOrder.Count)
        {
            _currentRound++;
            _firstRound = false;
            startRound();
            Console.Log($"=========== START NEW ROUND - ROUND {_currentRound} ============");
            return;
        }

        _currentCombatent = _turnOrder[_currentTurnIndex];
        Console.Log($"Starting turn for {_currentCombatent.name} in round {_currentRound}");

        if (_currentCombatent is PlayerActor p)
            PlayerTurnManager.Instance.StartPlayerTurn(p);
        else if (_currentCombatent is EnemyActor e)
            EnemyTurnManager.Instance.StartEnemyTurn(e);
    }

    public void EndTurn()
    {
        TurnOrderUI.Instance.AdvanceTurn();
        _currentCombatent.EndTurn();
        AdvanceToNextLivingActor();

        NewTurn();
    }

    private void AdvanceToNextLivingActor()
    {
        if (_turnOrder.Count == 0)
            return;
        int cnt = _turnOrder.Count;
        int start = _currentTurnIndex;
        for (int i = 1; i <= cnt; i++)
        {
            int nxt = start + i;            // no %cnt here
            if (nxt >= cnt)
                break;                      // we’ve gone past the last actor
            if (!_turnOrder[nxt].Stats.IsDead)
            {
                _currentTurnIndex = nxt;
                return;
            }
        }
        // if we get here, we've exhausted the list → will trigger new round
        _currentTurnIndex = cnt;
    }

    #endregion

    // ========================================================================

    #region Party & Removal
    private void startRound()
    {
        if (CombatEnded) return;

        _currentTurnIndex = 0;
        RollInitiative();       
        NewTurn();
    }

    private void createCombatent(List<ActorData> newParty)
    {
        for (int i = 0; i < _persistentEnemiesObjects.Count; i++)
            _persistentEnemiesObjects[i].SetActive(i < newParty.Count);

        UpdateCombatents();
        for (int i = 0; i < newParty.Count; i++)
            _enemyActors[i].InitializeEnemy(newParty[i], i + 1);
    }

    // CombatManager.cs
    private void removeCombatent(ActorManager dead)
    {
        // 1) Find the slot in the current turn order
        int idx = _turnOrder.IndexOf(dead);
        if (idx < 0) return;

        Console.Log($"[Combat] Removing {dead.name} @ idx={idx}.  pre-idx={_currentTurnIndex}, count={_turnOrder.Count}");

        // 2) If they were at or before the current pointer, back it up so we don't skip
        if (idx <= _currentTurnIndex)
            _currentTurnIndex = Mathf.Max(0, _currentTurnIndex - 1);

        // 3) Remove exactly that entry
        _turnOrder.RemoveAt(idx);
        TurnOrderUI.Instance.RemoveActorFromTurn(dead);

        Console.Log($"[Combat] After removal: new-idx={_currentTurnIndex}, count={_turnOrder.Count}");

        // update the actor lists too, so _playerActors/_enemyActors reflect the new state
        _enemyActors = _combatents.OfType<EnemyActor>().ToList();
        _playerActors = _combatents.OfType<PlayerActor>().ToList();

        // now check for end-of-combat
        CheckForVictoryOrDefeat();

    }

    private void UpdateCombatents()
    {
        var all = FindObjectsByType<ActorManager>(FindObjectsSortMode.None);
        _combatents = new List<ActorManager>(all);
        _enemyActors = _combatents.OfType<EnemyActor>().ToList();
        _playerActors = _combatents.OfType<PlayerActor>().ToList();
    }

    private void CheckForVictoryOrDefeat()
    {
        if (_enemyActors.All(e => e.Stats.IsDead))
        {
            CombatEnded = true;
            Console.Log("Victory!");
            EndCombat();

            CardRewardUI.Instance.ShowRewards(_playerActors);
        }
        else if (_playerActors.All(p => p.Stats.IsDead))
        {
            CombatEnded = true;
            Console.Log("Defeat...");
            // game-over screen, reload, etc.
            EndCombat();
        }
    }
    #endregion

    // ========================================================================

    #region Cleanup
    private void startNewCombat()
    {
        CombatEnded = false;
        startRound();
    }
    private void endCombat()
    {
        foreach (var a in _combatents)
        {
            a.Hand.DiscardHand(a is PlayerActor);
            a.Deck.ReshuffleDiscardIntoDeck();
            a.Stats.RemoveAllStatusEffects();
            a.RemoveHighLight();
            a.TurnOutlineEffect(false);
        }
    }
    #endregion

    // ========================================================================
}
