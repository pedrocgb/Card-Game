using static UEnums;

public class EnemyActor : ActorManager
{
    public void InitializeEnemy(ActorData NewData, int NewPosition)
    {
        // Set the actor data and initialize the actor manager
        _actorData = NewData;
        Initialize();
        _deck.InitializeDeck();
        _myStats.RemoveAllStatusEffects();
        _myStats.Initialize();

        // Initialize position
        _myPosition.SetCombatPosition(NewPosition);
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
    }

    public override void OnTurnEnd()
    {
        base.OnTurnEnd();

        _hand.DiscardHand(IsPlayer: false);
    }
}
