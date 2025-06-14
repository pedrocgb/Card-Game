﻿using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class PositionsManager : MonoBehaviour
{
    public static PositionsManager Instance { get; private set; }

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Transform _playerPartyContainer = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Transform _enemyPartyContainer = null;

    private List<ActorManager> _playerParty = new();
    private List<ActorManager> _enemyParty = new();

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

    private bool moveActor(ActorManager actor, int offSet)
    {
        // Get the list of actors in the party, either player or enemy
        var list = GetPartyList(actor);
        if (!list.Contains(actor)) return false;

        // Get the current index of the actor and calculate the target index (the new position it will move)
        int currentIndex = list.IndexOf(actor);
        int targetIndex = Mathf.Clamp(currentIndex + offSet, 0, list.Count - 1);

        // Prevent swaping with dead actors
        if (targetIndex == currentIndex ||
            list[targetIndex].Stats.IsDead)
            return false;

        // Swap the actor with the target index
        list.RemoveAt(currentIndex);
        list.Insert(targetIndex, actor);

        SortAndApplyPositions(list);
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

    

    // ========================================================================
}
