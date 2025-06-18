using System.Collections.Generic;
using System.Linq;

public static class CardEffectResolver
{
    public static void ApplyEffects(List<EffectBlock> Effects, ActorManager Source, ActorManager MainTarget, CardInstance Card)
    {
        List<ActorManager> targets = ResolveTargets(Card, Source, MainTarget);
        Console.Log(targets.Count + " targets found for " + Card.CardName);

        foreach (var target in targets)
        {
            foreach (var effect in Effects)
            {
                ActorManager definitiveTarget;
                if (effect.TargetSelf)
                    definitiveTarget = Source;
                else
                    definitiveTarget = target;

                switch (effect.EffectType)
                {
                    // Hostile Effects
                    default:
                    case UEnums.CardEffects.Damage:
                        Source.Stats.DealDamage(effect.Amount, definitiveTarget);
                        break;
                    case UEnums.CardEffects.Slow:
                        definitiveTarget.Stats.SufferSlow(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Vulnerability:
                        definitiveTarget.Stats.SufferVulnerability(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Weakness:
                        definitiveTarget.Stats.SufferWeakness(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Stun:
                        definitiveTarget.Stats.SufferStun(effect.Duration);
                        break;
                    case UEnums.CardEffects.Restrained:
                        definitiveTarget.Stats.SufferRestrained(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Lock:
                        definitiveTarget.Stats.SufferLockDebuff(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Burn:
                        definitiveTarget.Stats.SufferBurn(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Poison:
                        definitiveTarget.Stats.SufferPoison(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Bleed:
                        definitiveTarget.Stats.SufferBleed(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.BlockDamage:
                        Source.Stats.DealBlockDamage(definitiveTarget);
                        break;
                    case UEnums.CardEffects.Blind:
                        definitiveTarget.Stats.SufferBlind(effect.Amount, effect.Duration);
                        break;

                    // Buff effects
                    case UEnums.CardEffects.Heal:
                        definitiveTarget.Stats.Heal(effect.Amount);
                        break;
                    case UEnums.CardEffects.Block:
                        definitiveTarget.Stats.GainBlock(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Haste:
                        definitiveTarget.Stats.GainHaste(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Toughness:
                        definitiveTarget.Stats.GainToughness(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Dodge:
                        definitiveTarget.Stats.GainDodge(effect.Amount, effect.Duration);
                        break;
                    case UEnums.CardEffects.Draw:
                        definitiveTarget.Hand.DrawCards(effect.Amount);
                        break;
                    case UEnums.CardEffects.Riposte:
                        definitiveTarget.Stats.GainRiposte(effect.Amount, effect.Duration);
                        break;

                    // Other effects
                    case UEnums.CardEffects.Movement:
                        definitiveTarget.Positioning.ForceMovement(effect.Amount);
                        break;
                    case UEnums.CardEffects.AddCardToHand:
                        definitiveTarget.Hand.AddCardToHand(effect.NewCard, effect.Amount);
                        break;
                    case UEnums.CardEffects.AddCardToDeck:
                        
                        break;
                }
            }
        }
        
    }

    public static List<ActorManager> ResolveTargets(CardInstance Card, ActorManager Source, ActorManager Target)
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
            if (Card.TargetType == UEnums.Target.Ally && !Card.CanTargetSelf)
            {
                potentialTargets = potentialTargets.Where(t => t != Source).ToList();
            }

            if (Target != null && potentialTargets.Contains(Target))
                targets.Add(Target);
        }
        else
        {
            if (Card.TargetType == UEnums.Target.Ally && !Card.CanTargetSelf)
                potentialTargets = potentialTargets.Where(t => t != Source).ToList();

            targets.AddRange(potentialTargets);
        }

        return targets;
    }
}
