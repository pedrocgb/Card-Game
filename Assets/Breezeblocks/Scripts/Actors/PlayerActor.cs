using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerActor : ActorManager
{
    protected override void Awake()
    {
        base.Awake();

        Initialize();
        _myStats.Initialize();
    }

    public override void StartNewTurn()
    {
        IsMyTurn = true;
        TurnOutlineEffect(true);

        _myStats.OnNewTurn();

        if (_myStats.IsDead)
        {
            CombatManager.Instance.EndTurn();
            return;
        }

        _hand.ShowHand();
        _myUi.UpdateTurnMarker(false);

        _hand.DrawCards(_actorData.CardBuy);
    }

    public override void EndTurn()
    {
        base.EndTurn();
        _hand.HideHand();
        _hand.DiscardHand(IsPlayer: true);
    }
}
