using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class PositionsManager : MonoBehaviour
{
    #region Variables and Properties
    public static PositionsManager Instance { get; private set; }

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Transform _playerPartyContainer = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Transform _enemyPartyContainer = null;

    private List<ActorManager> _playerParty = new();
    private List<ActorManager> _enemyParty = new();
    #endregion

    // ========================================================================

    #region Initialization Methods
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        _playerParty.Clear();
        _enemyParty.Clear();

        // Iterate children in hierarchy order:
        foreach (Transform child in _playerPartyContainer)
        {
            var pa = child.GetComponent<PlayerActor>();
            if (pa != null && pa.gameObject.activeInHierarchy) 
                _playerParty.Add(pa);
        }

        foreach (Transform child in _enemyPartyContainer)
        {
            var ea = child.GetComponent<EnemyActor>();
            if (ea != null && ea.gameObject.activeInHierarchy) 
                _enemyParty.Add(ea);
        }
    }
    #endregion

    // ========================================================================

    #region Static Methods
    public static void RegistarActor(ActorManager NewActor)
    {
        if (Instance == null)
            Debug.LogError("PositionsManager instance is null");
        else
            Instance.registerActor(NewActor);
    }

    public static void RemoveActor(ActorManager Actor)
    {
        if (Instance == null)
            Debug.LogError("PositionsManager instance is null");
        else
            Instance.removeActor(Actor);
    }

    public static void MoveActor(ActorManager Actor, int OffSet)
    {
        if (Instance == null)
            Debug.LogError("PositionsManager instance is null");
        else
            Instance.moveActor(Actor, OffSet);
    }

    public static List<ActorManager> GetTeam<T>() where T : ActorManager
    {
        if (Instance == null)
            Debug.LogError("PositionsManager instance is null");
        else
            return Instance.getTeam<T>();
        return null;
    }

    public static List<ActorManager> GetTeamOf(ActorManager Actor)
    {
        if (Instance == null)
        {
            Debug.LogError("PositionsManager instance is null");
            return null;
        }

        return Instance.getTeamOf(Actor);
    }

    public static List<ActorManager> GetOpposingTeamOf(ActorManager Actor)
    {
        if (Instance == null)
        {
            Debug.LogError("PositionsManager instance is null");
            return null;
        }

        return Instance.getOpposingTeamOf(Actor);
    }

    public static void UpdatePositions()
    {
        if (Instance == null)
        {
            Debug.LogError("PositionsManager instance is null");
            return;
        }
        Instance.updatePositions();
    }
    #endregion

    // ========================================================================

    #region Local Methods
    private void registerActor(ActorManager newActor)
    {
        GetPartyList(newActor).Add(newActor);
        SortAndApplyPositions(GetPartyList(newActor));
    }

    private void removeActor(ActorManager actor)
    {
        GetPartyList(actor).Remove(actor);
        SortAndApplyPositions(GetPartyList(actor));
    }

    private bool moveActor(ActorManager actor, int offset)
    {
        var list = GetPartyList(actor);
        int currentIndex = list.IndexOf(actor);
        if (currentIndex < 0)
            return false;

        // +offset → forward (lower index), –offset → backward (higher index)
        int desiredIndex = Mathf.Clamp(currentIndex - offset, 0, list.Count - 1);

        // nothing to do if same slot, or target is dead
        if (desiredIndex == currentIndex || list[desiredIndex].Stats.IsDead)
            return false;

        // remove and re-insert at the exact same desiredIndex
        list.RemoveAt(currentIndex);
        list.Insert(desiredIndex, actor);
        SortAndApplyPositions(list);

        Debug.Log($"Moving {actor.ActorName} by {offset} positions.");
        return true;
    }
    private void updatePositions()
    {
        _playerParty.Clear();
        _enemyParty.Clear();

        // Iterate children in hierarchy order:
        foreach (Transform child in _playerPartyContainer)
        {
            var pa = child.GetComponent<PlayerActor>();
            if (pa != null && pa.gameObject.activeInHierarchy)
                _playerParty.Add(pa);
        }

        foreach (Transform child in _enemyPartyContainer)
        {
            var ea = child.GetComponent<EnemyActor>();
            if (ea != null && ea.gameObject.activeInHierarchy)
                _enemyParty.Add(ea);
        }
    }
    #endregion

    // ========================================================================
    private void SortAndApplyPositions(List<ActorManager> party)
    {
        // 1) Filter out dead actors, but DON’T re-sort by their old CurrentPosition
        var ordered = party
            .Where(a => !a.Stats.IsDead)
            .ToList();

        // 2) Now assign slots based on the new list order
        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].Positioning.SetCombatPosition(i + 1);
        }
    }

    private List<ActorManager> GetPartyList(ActorManager Actor)
    {
        return Actor is PlayerActor ? _playerParty : _enemyParty;
    }

    // ========================================================================

    #region Local Methods
    private List<ActorManager> getTeam<T>() where T : ActorManager
    {
        if (typeof(T) == typeof(PlayerActor)) return _playerParty;
        if (typeof(T) == typeof(EnemyActor)) return _enemyParty;
        return null;
    }

    private List<ActorManager> getTeamOf(ActorManager actor)
    {
        return actor is PlayerActor ? _playerParty : _enemyParty;
    }

    private List<ActorManager> getOpposingTeamOf(ActorManager actor)
    {
        return actor is PlayerActor ? _enemyParty : _playerParty;
    }
    #endregion

    // ========================================================================
}
