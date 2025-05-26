using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTurnManager : MonoBehaviour
{
    public static EnemyTurnManager Instance;
    private bool _cardPlayed = false;

    // ========================================================================

    private void Awake() => Instance = this;

    // ========================================================================

    #region Turn Methods
    public void StartEnemyTurn(EnemyActor enemy)
    {
        StartCoroutine(EnemyTurnCoroutine(enemy));
    }

    /// <summary>
    /// Coroutine to handle the enemy's turn.
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    private IEnumerator EnemyTurnCoroutine(EnemyActor enemy)
    {
        yield return new WaitForSeconds(1f); // small delay before acting

        enemy.StartNewTurn(); // draws cards, restores mana

        while (true)
        {
            _cardPlayed = false;
            yield return StartCoroutine(TryPlayCard(enemy));

            if (!_cardPlayed)
                break;

            yield return new WaitForSeconds(0.5f); // delay between actions 
        }

        yield return new WaitForSeconds(1f); // delay before passing turn

        TargetingManager.Instance.ClearHightLights();
        CombatManager.Instance.EndTurn();
    }
    #endregion

    // ========================================================================

    #region AI Logic
    /// <summary>
    /// Iterate the current hand and try to play a card randomly.
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    private IEnumerator TryPlayCard(EnemyActor enemy)
    {
        List<CardData> playableCards = new List<CardData>();

        // Loop through all cards in the enemy's hand
        foreach (var card in enemy.Hand.CurrentHand)
        {
            // Check if the card is playable
            if (!UCardValidator.IsCardPlayable(card, enemy, PositionsManager.GetTeam<PlayerActor>()))
            {
                UConsole.Log($"[AI] Skipping card {card.CardName} (not playable)");
                continue;
            }

            var possibleTargets = GetAllValidTargets(card, enemy);
            if (card.TargetScope == UEnums.TargetAmount.All ||
                possibleTargets.Count > 0)
            {
                playableCards.Add(card);
            }

        }

        if (playableCards.Count == 0)
        {
            _cardPlayed = false;
            yield break;
        }

        // Choose random card to play
        CardData selectedCard = playableCards[Random.Range(0, playableCards.Count)];
        // Get all valid targets
        List<ActorManager> validTargets = GetAllValidTargets(selectedCard, enemy);
        // Highlight valid targets
        TargetingManager.Instance.HighLightActors(enemy, selectedCard.TargetPositions, selectedCard.TargetType);

        yield return new WaitForSeconds(0.4f); // simulate thinking time

        if (selectedCard.TargetScope == UEnums.TargetAmount.Single)
        {
            var chosenTarget = validTargets[Random.Range(0, validTargets.Count)];
            chosenTarget.ShowTargetFeedback(selectedCard.TargetType);

            yield return new WaitForSeconds(0.5f);

            CardEffectResolver.ApplyEffects(selectedCard.CardEffects, enemy, chosenTarget, selectedCard);
            chosenTarget.HideTargetFeedBack();

            UConsole.Log($"Enemy {enemy.name} played {selectedCard.CardName} on {chosenTarget.name}");
        }
        else
        {
            foreach (var t in validTargets)
                t.ShowTargetFeedback(selectedCard.TargetType);

            yield return new WaitForSeconds(0.5f);

            CardEffectResolver.ApplyEffects(selectedCard.CardEffects, enemy, null, selectedCard);

            foreach (var t in validTargets)
                t.HideTargetFeedBack();

            UConsole.Log($"Enemy {enemy.name} played {selectedCard.CardName} on all valid targets");
        }

        TargetingManager.Instance.ClearHightLights();
        enemy.Stats.SpendAction(selectedCard.ActionCost);
        enemy.Hand.DiscardCard(selectedCard);
        _cardPlayed = true;
    }

    /// <summary>
    /// Get all valid targets for a card based on its target type, positions and the source actor.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    private List<ActorManager> GetAllValidTargets(CardData card, ActorManager source)
    {
        if (card.TargetType == UEnums.Target.Self)
        {
            return new List<ActorManager> { source };
        }

        var team = card.TargetType switch
        {
            UEnums.Target.Ally => PositionsManager.GetTeamOf(source),
            UEnums.Target.Enemy => PositionsManager.GetOpposingTeamOf(source),
            _ => new List<ActorManager>()
        };

        return team
            .Where(t => !t.Stats.IsDead && card.TargetPositions.Contains(t.Positioning.CurrentPosition))
            .ToList();
    }
    #endregion

    // ========================================================================
}
