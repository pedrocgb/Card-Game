using System;
using UnityEngine;

public class UEnums
{
    [Flags]
    public enum CardTypes
    {
        Attack = 1,
        Buff = 2,
        Debuff = 4,
        Heal = 8,
        Persistent = 16,
        Movement = 32
    }

    public enum Positions
    {
        Front = 1,
        MidFront = 2,
        MidBack = 3,
        Back = 4
    }

    public enum Target
    {
        Self = 1,
        Ally = 2,
        Enemy = 3
    }

    public enum TargetAmount
    {
        Single = 1,
        All = 2,
        Random = 3
    }

    public enum Rarity
    {
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        Epic = 4,
        Unique = 5
    }

    public enum CardEffects
    {
        Damage = 0,
        SelfDamage = 1,
        Vulnerability = 2,
        Weakness = 3,
        Slow = 4,
        Stun = 5,
        Burn = 6,
        Poison = 7,
        Restrained = 8,
        Blind = 9,
        Bleed = 10,
        Lock = 11,
        BlockDamage = 12,

        Heal = 100,
        Block = 101,
        Haste = 102,
        Regen = 103,
        Dodge = 104,
        Hide = 105,
        Foucus = 106,
        Toughness = 107,
        Riposte = 108,

        Movement = 200,
        Draw = 201,
        SelfMovement = 202,

    }

    public enum  StatusEffects
    {
        // Debuffs
        Vulnerability = 2,
        Weakness = 3,
        Slow = 4,
        Stun = 5,
        Burn = 6,
        Poison = 7,
        Restrained = 8,
        Blind = 9,
        Bleed = 10,
        Lock = 11,

        // Buffs
        Block = 101,
        Haste = 102,
        Regen = 103,
        Dodge = 104,
        Hide = 105,
        Foucus = 106,
        Toughness = 107,
        Riposte = 108
    }

    public enum StatusStackingMode
    {
        None = 0,
        RefreshDurationOnly = 1,
        StackAmountOnly = 2,
        StackBoth = 4
    }

    public enum HealthModColors
    {
        BasicDamage = 0,
        BurnDamage = 1,
        PoisonDamage = 2,
        Heal = 3,
        Dodge = 4
    }

    public enum MapNodeType
    {
        Combat,
        Shop,
        Treasure,
        Elite,
        Boss,
        Corruption,
        Event,
        General
        
    }

    public enum CardOrigin
    {
        Racial = 1,
        Specialization = 2
    }
}
