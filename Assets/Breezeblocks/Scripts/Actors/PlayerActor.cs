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

        _hand.ShowHand();
        _myStats.OnNewTurn();
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
