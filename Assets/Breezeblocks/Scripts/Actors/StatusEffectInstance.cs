using UnityEngine;

[System.Serializable]
public class StatusEffectInstance
{
    public UEnums.StatusEffects StatusEffect;
    public int Amount;
    public int DurationRemaining;

    public StatusEffectInstance(UEnums.StatusEffects statusEffect, int amount, int durationRemaining)
    {
        StatusEffect = statusEffect;
        Amount = amount;
        DurationRemaining = durationRemaining;
    }
}
