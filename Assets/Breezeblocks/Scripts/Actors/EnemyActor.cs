using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyActor : ActorManager
{
    protected override void Initialize()
    {
        base.Initialize();
    }

    public override void StartNewTurn()
    {
        base.StartNewTurn();
    }

    public override void EndTurn()
    {
        base.EndTurn();

        _hand.DiscardHand(IsPlayer: false);
    }
}
