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
    private CardData _cardData = null;  
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
        _actor = Player;
        _cardUI = null;

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
        _cardData = Card.CardData;

        _targetingManager.HighLightActors(_actor, _cardData.TargetPositions, _cardData.Target);
    }

    public void UseCard()
    {
        CardEffectResolver.ApplyEffects(_cardData.CardEffects, _actor, TargetingManager.Instance.CurrentTarget, _cardData);

        _actor.Hand.DiscardCard(_cardUI);
        _targetingManager.ClearHightLights();
        _targetingManager.SetTarget(null);
        _actor.Stats.SpendAction(_cardData.ActionCost);

        _actor.Hand.ValidadeHand(_actor.Stats.CurrentActions, _actor.CurrentPosition);
    }
    #endregion

    // ========================================================================
}
