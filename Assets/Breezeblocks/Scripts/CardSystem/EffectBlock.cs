using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class EffectBlock
{
    public UEnums.CardEffects EffectType = UEnums.CardEffects.None;
    [HideIf("EffectType", UEnums.CardEffects.None)]
    public int Amount;
    [HideIf("@(EffectType) == UEnums.CardEffects.AddCardToHand || (EffectType) == UEnums.CardEffects.AddCardToDeck || (EffectType) == UEnums.CardEffects.None")]
    public int Duration;

    [Space(20)]
    [InfoBox("Use only with DEBUFF effects like Vulnerability and Weakness. For BUFFs and similar effects, it must be only CHECKED if the card " +
        "specificaly targets an enemy and you want the buff effect to affect the user. For example, Deals 8 damage and Gain 4 Block.", InfoMessageType.Warning)]
    public bool TargetSelf = false;

    [Space(20)]
    [InfoBox("Card Data is used to apply specific card effect like create a specific card in hand or add a card to a deck, etc.", InfoMessageType.Warning)]
    [ShowIf("@(EffectType) == UEnums.CardEffects.AddCardToHand || (EffectType) == UEnums.CardEffects.AddCardToDeck")]
    public CardData NewCard;
}
