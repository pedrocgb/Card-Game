using System.Collections.Generic;

public static class CardEffectResolver
{
    public static void ApplyEffects(List<EffectBlock> Effects, ActorManager Source, ActorManager Target, CardData Card)
    {
        foreach (var effect in Effects)
        {
            switch (effect.EffectType)
            {
                default:
                case UEnums.CardEffects.Damage:
                    Target.Stats.TakeDamage(Source.Stats.Damage);
                    break;
                case UEnums.CardEffects.Heal:
                    Target.Stats.Heal(effect.Amount);
                    break;
                case UEnums.CardEffects.Block:
                    Target.Stats.GainBlock(effect.Amount, effect.Duration);
                    break;
                case UEnums.CardEffects.Haste:

                    break;
                case UEnums.CardEffects.Slow:
                    break;
                case UEnums.CardEffects.Vulnerability:
                    break;
                case UEnums.CardEffects.Weakness:
                    break;
                case UEnums.CardEffects.Stun:
                    break;

                case UEnums.CardEffects.Movement:
                    PositionsManager.MoveActor(Target, effect.Amount);
                    break;
            }
        }
    }
}
