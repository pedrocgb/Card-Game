using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnManager : MonoBehaviour
{
    public static EnemyTurnManager Instance;

    private void Awake() => Instance = this;

    public void StartEnemyTurn(EnemyActor enemy)
    {
        StartCoroutine(EnemyTurnCoroutine(enemy));
    }

    private IEnumerator EnemyTurnCoroutine(EnemyActor enemy)
    {
        yield return new WaitForSeconds(1f); // small delay before acting

        enemy.StartNewTurn(); // draws cards, restores mana

        bool playedCard = true;

        while (playedCard)
        {
            yield return new WaitForSeconds(1f); // delay between plays
            playedCard = TryPlayCard(enemy);
        }

        yield return new WaitForSeconds(1f); // delay before passing turn

        enemy.EndTurn();
        CombatManager.Instance.EndTurn();
    }

    private bool TryPlayCard(EnemyActor enemy)
    {
        foreach (var card in enemy.Hand.CurrentHand)
        {
            if (!UCardValidator.IsCardPlayable(card, enemy, PositionsManager.GetTeam<PlayerActor>()))
                continue;

            var possibleTargets = GetValidTargetsForCard(card, enemy);

            if (possibleTargets.Count == 0)
                continue;

            ActorManager target = possibleTargets[Random.Range(0, possibleTargets.Count)];

            // Use card
            CardEffectResolver.ApplyEffects(card.CardEffects, enemy, target, card);
            enemy.Stats.SpendAction(card.ActionCost);

            UConsole.Log($"{enemy.name} used {card.CardName} on {target.name}");

            return true;
        }

        return false; // nothing could be played
    }

    private List<ActorManager> GetValidTargetsForCard(CardData card, EnemyActor enemy)
    {
        var players = PositionsManager.GetTeam<PlayerActor>();

        return players.FindAll(p =>
            !p.Stats.IsDead &&
            card.TargetPositions.Contains(p.Positioning.CurrentPosition));
    }
}
