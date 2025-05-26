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
        base.StartNewTurn();
        _hand.ShowHand();
    }

    public override void EndTurn()
    {
        base.EndTurn();
        _hand.HideHand();
    }
}
