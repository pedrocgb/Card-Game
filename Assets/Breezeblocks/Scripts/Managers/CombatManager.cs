using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [FoldoutGroup("Combatents", expanded:true)]
    [SerializeField]
    private List<ActorManager> _combatents = new List<ActorManager>();

    private int _currentRound = 1;
    private int _currentTurnIndex = 0;
    private bool _firstRound = true;

    private ActorManager _currentCombatent = null;

    private void Start()
    {
        RollInitiative();
    }

    private void RollInitiative()
    {
        foreach (ActorManager c in _combatents)
        {
            int roll = Random.Range(1, 11);
            c.CurrentInitiative = roll + c.InitiativeBonus;
        }

        _combatents = _combatents.OrderByDescending(c => c.CurrentInitiative).ToList();

        Debug.Log("Initiative Rolls:");
        foreach (ActorManager c in _combatents)
        {
            Debug.Log($"{c.name}: {c.CurrentInitiative}");
        }

        Invoke(nameof(StartRound), 2f);
    }

    private void StartRound()
    {
        _currentTurnIndex = 0;
        NewTurn();
    }

    private void NewTurn()
    {
        if (_currentTurnIndex >= _combatents.Count)
        {
            _currentRound++;
            if (_firstRound)
                _firstRound = false;

            StartRound();
            Debug.Log($"=========== START NEW ROUND - ROUND {_currentRound} ============");

            return;
        }

        _currentCombatent = _combatents[_currentTurnIndex];
        _currentCombatent.StartNewTurn();
        Debug.Log($"Starting turn for {_currentCombatent.name} in round {_currentRound}");

        if (_firstRound)
        {
            _currentCombatent.Deck.DrawCards(5);
        }
        else
        {
            _currentCombatent.Deck.DrawCards(_currentCombatent.CardBuy);
        }
    }

    private void EndTurn()
    {
        Debug.Log($"Ending turn for {_currentCombatent.name}");

        _currentTurnIndex++;

        NewTurn();
    }
}
