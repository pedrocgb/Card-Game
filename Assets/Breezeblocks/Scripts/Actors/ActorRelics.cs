using System.Collections.Generic;
using UnityEngine;

public class ActorRelics : MonoBehaviour
{
    #region Variables and Properties
    private ActorManager _actor;
    private List<RelicData> _relics = new List<RelicData>(5);
    public List<RelicData> Relics => _relics;
    #endregion

    // ========================================================================

    void Awake() => _actor = GetComponent<ActorManager>();

    // ========================================================================

    #region Equip Methods
    public bool TryEquip(RelicData relic)
    {
        if (_relics.Count >= UConstants.MAX_RELICS_PER_ACTOR) return false;

        _relics.Add(relic);
        relic.OnEquip(_actor);
        return true;
    }

    public void Unequip(RelicData relic)
    {
        if (_relics.Remove(relic))
            relic.OnUnequip(_actor);
    }
    #endregion

    // ========================================================================

    #region Turn Methods
    private void OnActorStartTurn()
    {
        foreach (var relic in _relics)
            relic.OnTurnStart(_actor);
    }

    private void OnActorEndTurn()
    {
        foreach (var relic in _relics)
            relic.OnTurnEnd(_actor);
    }
    #endregion

    // ========================================================================

    void OnEnable()
    {
        _actor.TurnStartEvent += OnActorStartTurn;
        _actor.TurnEndEvent += OnActorEndTurn;
    }

    void OnDisable()
    {
        _actor.TurnStartEvent -= OnActorStartTurn;
        _actor.TurnEndEvent -= OnActorEndTurn;
    }


    // ========================================================================
}
