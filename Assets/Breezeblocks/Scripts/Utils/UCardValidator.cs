using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UCardValidator
{
    public static bool IsCardPlayable(CardInstance card, ActorManager actor, List<ActorManager> validTargets)
    {
        // Lock check
        if (card.IsLocked)
        {
            Console.Log($"[CardValidator] Card {card.CardName} is locked and cannot be played.");
            return false;
        }

        validTargets = new List<ActorManager>();

        // 1. Action check
        if (actor.Stats.CurrentActions < card.ActionCost)
        {
            Console.Log($"[CardValidator] Actor {actor.ActorName} does not have enough actions to play {card.CardName}. Required: {card.ActionCost}, Available: {actor.Stats.CurrentActions}");
            return false;
        }

        // 2. Position check
        if (!card.UsablePositions.Contains(actor.Positioning.CurrentPosition))
        {
            Console.Log($"[CardValidator] Actor {actor.ActorName} is not in a valid position to play {card.CardName}. Required: {string.Join(", ", card.UsablePositions)}, Current: {actor.Positioning.CurrentPosition}");
            return false;
        }

        // 3. Target check
        List<ActorManager> potentialTargets = GetAllValidTargets(card, actor);

        foreach (var t in potentialTargets)
        {
            if (t.Stats.IsDead)
                continue;

            if (card.UsablePositions.Contains(actor.Positioning.CurrentPosition))
                validTargets.Add(actor);
        }

        return validTargets.Count > 0;
    }

    public static List<ActorManager> GetAllValidTargets(CardInstance card, ActorManager source)
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
}
