using static UEnums;

public class EnemyActor : ActorManager
{
    public void InitializeEnemy(ActorData NewData, int NewPosition)
    {
        // Set the actor data and initialize the actor manager
        _actorData = NewData;
        Initialize();
        _myStats.Initialize();

        // Initialize position
        _myPosition.SetCombatPosition(NewPosition);
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
