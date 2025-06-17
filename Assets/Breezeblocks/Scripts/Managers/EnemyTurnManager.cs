using Rewired;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTurnManager : MonoBehaviour
{
    #region Variables and Properties
    public static EnemyTurnManager Instance;
    private bool _cardPlayed = false;
    #endregion

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
        yield return new WaitForSeconds(0.5f);
        enemy.OnTurnStart();

        if (enemy.Stats.IsDead)
        {
            Console.Log($"[AI] {enemy.name} died mid-turn. Removing then ending.");
            CombatManager.RemoveCombatent(enemy);
            TargetingManager.Instance.ClearHightLights();
            CombatManager.Instance.EndTurn();
            yield break;
        }

        yield return new WaitForSeconds(1f);
        if (enemy.Stats.IsStunned)
        {
            Console.Log($"[AI] {enemy.name} is stunned. Skipping.");
            CombatManager.Instance.EndTurn();
            yield break;
        }

        while (true)
        {
            _cardPlayed = false;
            yield return StartCoroutine(TryPlayCard(enemy));

            if (enemy.Stats.IsDead)
            {
                Console.Log($"[AI] {enemy.name} died mid-turn after playing a card. Ending turn.");
                TargetingManager.Instance.ClearHightLights();
                CombatManager.Instance.EndTurn();
                yield break;
            }

            if (!_cardPlayed) break;
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1f);
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
        List<CardInstance> playableCards = new List<CardInstance>();

        // 1) Build list of playable cards, respecting CanTargetSelf
        foreach (var card in enemy.Hand.CurrentHand)
        {
            // a) base playability
            if (!UCardValidator.IsCardPlayable(card, enemy, PositionsManager.GetTeam<PlayerActor>()))
                continue;

            // b) get all valid targets
            var possibleTargets = UCardValidator.GetAllValidTargets(card, enemy);

            // c) if this card cannot target self, remove the enemy from the list
            if (!card.CanTargetSelf && card.TargetType != UEnums.Target.Self)            
                possibleTargets = possibleTargets.Where(t => t != (ActorManager)enemy).ToList();
            

            // d) decide if card is okay:
            //    - if it's an AoE/all card, we need at least one valid target
            //    - if it's a single-target card, we also need at least one
            bool ok = possibleTargets.Count > 0;
            if (card.TargetScope == UEnums.TargetAmount.All && ok)
                playableCards.Add(card);
            else if (card.TargetScope == UEnums.TargetAmount.Single && ok)
                playableCards.Add(card);
        }

        // 2) nothing to play?
        if (playableCards.Count == 0)
        {
            _cardPlayed = false;
            yield break;
        }

        // 3) pick one and re-calc its filtered target list
        var selectedCard = playableCards[Random.Range(0, playableCards.Count)];
        var validTargets = UCardValidator.GetAllValidTargets(selectedCard, enemy);
        if (!selectedCard.CanTargetSelf && selectedCard.TargetType != UEnums.Target.Self)
            validTargets = validTargets.Where(t => t != (ActorManager)enemy).ToList();

        // 4) highlight
        TargetingManager.Instance.HighLightActors(
            enemy,
            selectedCard.TargetPositions,
            selectedCard.TargetType,
            selectedCard.CanTargetSelf
        );
        yield return new WaitForSeconds(0.4f);

        // 5) actually play it
        if (selectedCard.TargetScope == UEnums.TargetAmount.Single)
        {
            var chosen = validTargets[Random.Range(0, validTargets.Count)];
            chosen.ShowTargetFeedback(selectedCard.TargetType);
            yield return new WaitForSeconds(0.5f);
            CardPreviewManager.ShowEnemyCard(enemy, selectedCard);
            yield return new WaitForSeconds(0.5f);

            CardEffectResolver.ApplyEffects(
                selectedCard.CardEffects,
                enemy,
                chosen,
                selectedCard
            );
            chosen.HideTargetFeedBack();            
        }
        else
        {
            // multi‐target / AoE
            foreach (var t in validTargets)
                t.ShowTargetFeedback(selectedCard.TargetType);
            yield return new WaitForSeconds(0.5f);
            CardPreviewManager.ShowEnemyCard(enemy, selectedCard);
            yield return new WaitForSeconds(0.5f);

            CardEffectResolver.ApplyEffects(
                selectedCard.CardEffects,
                enemy,
                null,  
                selectedCard
            );
            foreach (var t in validTargets)
                t.HideTargetFeedBack();            
        }

        // 6) cleanup
        TargetingManager.Instance.ClearHightLights();
        enemy.Stats.SpendAction(selectedCard.ActionCost);
        enemy.Hand.DiscardCard(selectedCard);
        _cardPlayed = true;
    }

    #endregion

    // ========================================================================
}
