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

    // ========================================================================

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        _hand.ShowHand();

        TurnOrderUI.Instance.ChangeCombatPanel(false);
    }

    public override void OnTurnEnd()
    {
        base.OnTurnEnd();

        _hand.HideHand();
        _hand.DiscardHand(IsPlayer: false);
    }

    // ========================================================================
}
