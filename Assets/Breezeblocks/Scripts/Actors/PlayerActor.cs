public class PlayerActor : ActorManager
{
    protected override void Awake()
    {
        base.Awake();

        Initialize();
        _myStats.Initialize();
    }

    // ========================================================================

    public override void OnTurnStart()
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

        _hand.DrawCards(_myStats.CardBuy);

        RaiseTurnStartEvent();
    }

    public override void OnTurnEnd()
    {
        base.OnTurnEnd();
        _hand.HideHand();
        _hand.DiscardHand(IsPlayer: true);
    }

    // ========================================================================
}
