using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UCardValidator
{
    public static bool IsCardPlayable(CardData card, ActorManager actor, List<ActorManager> validTargets)
    {
        validTargets = new List<ActorManager>();

        // 1. Mana check
        if (actor.Stats.CurrentActions < card.ActionCost)
            return false;

        // 2. Position check
        if (!card.Positions.Contains(actor.Positioning.CurrentPosition))
            return false;

        // 3. Target check
        List<ActorManager> potentialTargets = GetAllValidTargets(card, actor);

        foreach (var t in potentialTargets)
        {
            if (t.Stats.IsDead)
                continue;

            if (card.Positions.Contains(actor.Positioning.CurrentPosition))
                validTargets.Add(actor);
        }

        return validTargets.Count > 0;
    }

    public static List<ActorManager> GetAllValidTargets(CardData card, ActorManager source)
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
