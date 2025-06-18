using System.Collections.Generic;
using UnityEngine;
using static UEnums;

[System.Serializable]
public class CardInstance
{
    #region Variables and Properties
    // Data
    public CardData Data { get; private set; }

    // Card basic info
    public string CardName { get; set; }
    public string CardDescription { get; set; }
    public Sprite CardImage { get; set; }
    public int ActionCost { get; set; }

    // Card Effects
    public bool ConsumeCard { get; set; } = false;
    public List<EffectBlock> CardEffects { get; set; } = new List<EffectBlock>();

    // Positions
    public CardTypes CardType { get; set; }
    public List<Positions> UsablePositions { get; set; }
    public Sprite PositionIcon { get; set; }

    // Targeting
    public Target TargetType { get; set; }
    public TargetAmount TargetScope { get; set; }
    public List<Positions> TargetPositions { get; set; }
    public Sprite TargetIcon { get; set; }
    public bool CanTargetSelf { get; private set; }

    // Card Status
    public bool IsLocked { get; private set; } = false;
    #endregion

    // ========================================================================

    public CardInstance(CardData data)
    {
        Data = data;

        CardName = data.CardName;
        CardDescription = data.CardDescription;
        CardImage = data.CardImage;
        ActionCost = data.ActionCost;

        ConsumeCard = data.ConsumeCard;
        CardEffects = new List<EffectBlock>(data.CardEffects);

        CardType = data.CardType;
        UsablePositions = new List<Positions>(data.Positions);
        PositionIcon = data.PositionIcon;

        TargetType = data.TargetType;
        TargetScope = data.TargetScope;
        TargetPositions = new List<Positions>(data.TargetPositions);
        TargetIcon = data.TargetIcon;
        CanTargetSelf = data.CanTargetSelf;
    }

    // ========================================================================

    public void LockCard(bool Lock)
    {
        IsLocked = Lock;
    }

    // ========================================================================
}
