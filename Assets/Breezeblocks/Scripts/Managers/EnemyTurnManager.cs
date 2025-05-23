using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnManager : MonoBehaviour
{
    public static EnemyTurnManager Instance;
    private bool _cardPlayed = false;

    private void Awake() => Instance = this;

    public void StartEnemyTurn(EnemyActor enemy)
    {
        StartCoroutine(EnemyTurnCoroutine(enemy));
    }

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
        enemy.EndTurn();
        CombatManager.Instance.EndTurn();
    }

    private IEnumerator TryPlayCard(EnemyActor enemy)
    {
        List<CardData> playableCards = new List<CardData>();

        // Loop through all cards in the enemy's hand
        foreach (var card in enemy.Hand.CurrentHand)
        {
            // Check if the card is playable
            if (!UCardValidator.IsCardPlayable(card, enemy, PositionsManager.GetTeam<PlayerActor>()))
                continue;

            var targets = CardEffectResolver.ResolveTargets(card, enemy, null);
            if (card.TargetScope == UEnums.TargetAmount.All ||
                targets.Count > 0)
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
        var validTargets = CardEffectResolver.ResolveTargets(selectedCard, enemy, null);
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
}
