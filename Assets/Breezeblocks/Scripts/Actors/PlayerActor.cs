using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerActor : ActorManager
{
    protected override void Initialize()
    {
        base.Initialize();
    }

    public override void StartNewTurn()
    {
        TurnOutlineEffect(true);

        _hand.ShowHand();
        _hand.DrawCards(_actorData.CardBuy);

        _myStats.OnNewTurn();
    }

    public override void EndTurn()
    {
        base.EndTurn();
        _hand.HideHand();
        _hand.DiscardHand(IsPlayer: true);
    }
}
