using UnityEngine;

[System.Serializable]
public class StatusEffectInstance
{
    public UEnums.StatusEffects StatusEffect;
    public UEnums.StatusStackingMode StackingMode;
    public int Amount;
    public int DurationRemaining;

    public StatusEffectInstance(UEnums.StatusEffects statusEffect, int amount, int durationRemaining, UEnums.StatusStackingMode stackingMode)
    {
        StatusEffect = statusEffect;
        Amount = amount;
        DurationRemaining = durationRemaining;
        StackingMode = stackingMode;
    }
}
