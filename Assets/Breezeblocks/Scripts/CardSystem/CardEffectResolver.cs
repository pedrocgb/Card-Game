using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine.EventSystems;

public static class CardEffectResolver
{
    public static void ApplyEffects(List<EffectBlock> Effects, ActorManager Source, ActorManager MainTarget, CardData Card)
    {
        List<ActorManager> targets = ResolveTargets(Card, Source, MainTarget);
        Console.Log(targets.Count + " targets found for " + Card.CardName);

        foreach (var target in targets)
        {
            foreach (var effect in Effects)
            {
                switch (effect.EffectType)
                {
                    // Hostile Effects
                    default:
                    case UEnums.CardEffects.Damage:
                        Source.Stats.DealDamage(effect.Amount, target);
                        break;
                    case UEnums.CardEffects.SelfDamage:
                        Source.Stats.DealDamage(effect.Amount, Source);
                        break;
                    case UEnums.CardEffects.Slow:
                        target.Stats.SufferSlow(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Vulnerability:
                        target.Stats.SufferVulnerability(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Weakness:
                        target.Stats.SufferWeakness(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Stun:
                        break;
                    case UEnums.CardEffects.Restrained:
                        target.Stats.SufferRestrained(effect.Amount, effect.Duration);
                        break;

                    // Buff effects
                    case UEnums.CardEffects.Heal:
                        target.Stats.Heal(effect.Amount);
                        break;
                    case UEnums.CardEffects.Block:
                        target.Stats.GainBlock(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Haste:
                        break;
                    case UEnums.CardEffects.Toughness:
                        target.Stats.GainToughness(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Draw:
                        Source.Hand.DrawCards(effect.Amount);
                        break;

                    // Other effects
                    case UEnums.CardEffects.Movement:
                        PositionsManager.MoveActor(target, effect.Amount);
                        break;
                    case UEnums.CardEffects.SelfMovement:
                        PositionsManager.MoveActor(Source, effect.Amount);
                        break;
                }
            }
        }
        
    }

    public static List<ActorManager> ResolveTargets(CardData Card, ActorManager Source, ActorManager Target)
    {
        Console.Log($"[AI] Resolving targets for {Card.CardName} (Type: {Card.TargetType}, Scope: {Card.TargetScope})");
        var targets = new List<ActorManager>();


        // Check if the card is a self-targeting card
        // If it is, set the target to the source and end the function
        if (Card.TargetType == UEnums.Target.Self)
        {
            targets.Add(Source);
            return targets;
        }

        // Check if the card is targeting the Team of the source or the enemy team of the source.
        List<ActorManager> potentialTargets = Card.TargetType switch
        {
            UEnums.Target.Ally => PositionsManager.GetTeamOf(Source),
            UEnums.Target.Enemy => PositionsManager.GetOpposingTeamOf(Source),
            _ => new List<ActorManager>()
        };

        Console.Log("Potential targets: " + potentialTargets.Count);

        // Filter by position
        potentialTargets = potentialTargets.Where(t => !t.Stats.IsDead && Card.TargetPositions.Contains(t.Positioning.CurrentPosition)).ToList();

        Console.Log("Filtered targets: " + potentialTargets.Count);

        Console.Log($"[AI] Found {potentialTargets.Count} valid targets for {Card.CardName}");

        if (Card.TargetScope == UEnums.TargetAmount.Single)
        {
            if (Target != null && potentialTargets.Contains(Target))
            {
                targets.Add(Target);
            }
        }
        else
        {
            targets.AddRange(potentialTargets);
        }

        return targets;
    }
}
