
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class EffectBlock
{
    public UEnums.CardEffects EffectType;
    public int Amount;
    public int Duration;

    [Space(20)]
    [InfoBox("Use only with DEBUFF effects like Vulnerability and Weakness. For BUFFs and similar effects, it must be only CHECKED if the card " +
        "specificaly targets an enemy and you want the buff effect to affect the user. For example, Deals 8 damage and Gain 4 Block.", InfoMessageType.Warning)]
    public bool TargetSelf;
}
