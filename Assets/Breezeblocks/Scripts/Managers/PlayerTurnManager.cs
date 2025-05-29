using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerTurnManager : MonoBehaviour
{
    #region Variables and Properties
    // Singleton
    public static PlayerTurnManager Instance = null;

    // Components
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private CombatManager _combatManager = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TargetingManager _targetingManager = null;

    // Cards
    private CardInstance _card = null;  
    private CardUI _cardUI = null;

    // Player Actor
    private PlayerActor _actor = null;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    // ========================================================================

    #region Start Turn Methods
    public void StartPlayerTurn(PlayerActor Player)
    {
        if (Player.Stats.IsStunned)
        {
            CombatManager.Instance.EndTurn();
            Console.Log($"Player {Player.name} is stunned! Skipping turn.");
            return;
        }

        _actor = Player;
        _cardUI = null;

        _actor.StartNewTurn();
    }
    #endregion

    // ========================================================================

    #region Card Management Methods
    /// <summary>
    /// Select a card to use for the current turn.
    /// </summary>
    /// <param name="Card"></param>
    public void SelectCard(CardUI Card)
    {
        // Get card Data
        _cardUI = Card;
        _card = Card.CardInstance;

        _targetingManager.HighLightActors(_actor, _card.TargetPositions, _card.TargetType);
        if (_card.TargetScope == UEnums.TargetAmount.All)
        {
            _targetingManager.HighTargetActors(_actor, _card.TargetPositions, _card.TargetType);
        }
    }

    public void UseCard()
    {
        if (_actor.Hand.ValidateCard(_cardUI, _actor))
        {
            Console.Log($"{_actor.name} used {_card.CardName} on {TargetingManager.Instance.CurrentTarget.name}");

            _actor.Stats.SpendAction(_card.ActionCost);
            CardEffectResolver.ApplyEffects(_card.CardEffects, _actor, TargetingManager.Instance.CurrentTarget, _card);

            _actor.Hand.DiscardCard(_cardUI);
            _targetingManager.ClearHightLights();
            _targetingManager.SetTarget(null);

            _actor.Hand.ValidateHand();
        }

    }
    #endregion

    // ========================================================================
}
