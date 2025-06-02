using Sirenix.OdinInspector;
using System.Collections;
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
    private CardUI _selectedCard = null;
    private CardInstance _card = null;  

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
        DeselectCard();

        _actor.StartNewTurn();
    }
    #endregion

    // ========================================================================

    #region Card Management Methods
    public void UseCard()
    {
        StartCoroutine(UseCardCoroutine());
    }

    private IEnumerator UseCardCoroutine()
    {
        if (_actor.Hand.ValidateCard(_selectedCard, _actor))
        {
            Console.Log($"{_actor.name} used {_card.CardName} on {TargetingManager.Instance.CurrentTarget.name}");

            _actor.Stats.SpendAction(_card.ActionCost);
            CardEffectResolver.ApplyEffects(_card.CardEffects, _actor, TargetingManager.Instance.CurrentTarget, _card);

            _actor.Hand.DiscardCard(_selectedCard);
            _targetingManager.ClearHightLights();
            _targetingManager.SetTarget(null);

            _actor.Hand.ValidateHand();
        }

        yield return new WaitForSeconds(0.5f);

        // Deselect cards when card is played
        _actor.Hand.RebuildHandLayout();
        DeselectCard();
    }

    public void OnCardClicked(CardUI Card)
    {
        // If the card is already selected, deselect it
        if (_selectedCard == Card)
        {
            DeselectCard();
            return;
        }

        // Prevents other cards from being selected, player must deselect the current selected card manually.
        if (_selectedCard != null)
            return;

        // Select new card
        _selectedCard = Card;
        _card = Card.CardInstance;
        Card.OnSelection();

        // Disable all cards
        foreach (var ui in _actor.Hand.CurrentHandUI)
        {
            if (ui != Card)
                ui.SetInteractable(false);
        }

        // Targeting system
        _targetingManager.HighLightActors(_actor, _card.TargetPositions, _card.TargetType, _card.CanTargetSelf);
        if (_card.TargetScope == UEnums.TargetAmount.All)
        {
            _targetingManager.HighTargetActors(_actor, _card.TargetPositions, _card.TargetType, _card.CanTargetSelf);
        }
    }

    private void DeselectCard()
    {
        if (_selectedCard == null) return; // If there are no cards selected, return

        _selectedCard.OnDeselection();
        _selectedCard = null;

        foreach (var ui in _actor.Hand.CurrentHandUI)
        {
            ui.SetInteractable(true);
            ui.HoverHandler.enabled = true;
        }

        _targetingManager.ClearHightLights();
        _targetingManager.SetTarget(null);
    }
    #endregion

    // ========================================================================
}
