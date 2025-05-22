using System.Collections.Generic;

public static class CardEffectResolver
{
    public static void ApplyEffects(List<EffectBlock> Effects, ActorManager Source, ActorManager Target, CardData Card)
    {
        foreach (var effect in Effects)
        {
            switch (effect.EffectType)
            {
                // Hostile Effects
                default:
                case UEnums.CardEffects.Damage:
                    Source.Stats.DealDamage(effect.Amount, Target);
                    break;
                case UEnums.CardEffects.SelfDamage:
                    Source.Stats.DealDamage(effect.Amount, Source);
                    break;
                case UEnums.CardEffects.Slow:
                    Target.Stats.SufferSlow(effect.Amount, effect.Duration);
                    break;
                case UEnums.CardEffects.Vulnerability:
                    Target.Stats.SufferVulnerability(effect.Amount, effect.Duration);
                    break;
                case UEnums.CardEffects.Weakness:
                    Target.Stats.SufferWeakness(effect.Amount, effect.Duration);
                    break;
                case UEnums.CardEffects.Stun:
                    break;

                // Buff effects
                case UEnums.CardEffects.Heal:
                    Target.Stats.Heal(effect.Amount);
                    break;
                case UEnums.CardEffects.Block:
                    Target.Stats.GainBlock(effect.Amount, effect.Duration);
                    break;
                case UEnums.CardEffects.Haste:
                    break;

                // Other effects
                case UEnums.CardEffects.Movement:
                    PositionsManager.MoveActor(Target, effect.Amount);
                    break;
                case UEnums.CardEffects.SelfMovement:
                    PositionsManager.MoveActor(Source, effect.Amount);
                    break;
            }
        }
    }
}
