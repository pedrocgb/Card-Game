using System.Collections.Generic;
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
        List<ActorManager> potentialTargets = PositionsManager.GetTeam<PlayerActor>();

        foreach (var t in potentialTargets)
        {
            if (t.Stats.IsDead)
                continue;

            if (card.Positions.Contains(actor.Positioning.CurrentPosition))
                validTargets.Add(actor);
        }

        return validTargets.Count > 0;
    }
}
