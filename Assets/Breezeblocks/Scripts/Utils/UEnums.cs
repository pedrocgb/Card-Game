using System;
using UnityEngine;

public class UEnums
{
    public enum HeroClasses
    {
        Warrior,
        Archer,
        Rogue,
        Mage,
        Cleric,
        Paladin
    }

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

    public enum CardRarity
    {
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        Unique = 4
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

        Heal = 100,
        Block = 101,
        Haste = 102,
        Regen = 103,
        Dodge = 104,
        Hide = 105,
        Foucus = 106,
        Toughness = 107,

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

        // Buffs
        Block = 101,
        Haste = 102,
        Regen = 103,
        Dodge = 104,
        Hide = 105,
        Foucus = 106,
        Toughness = 107,
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
        Heal = 3
    }
}
