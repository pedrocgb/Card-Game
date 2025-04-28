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
        MidFront =2,
        MidBack = 3,
        Back = 4
    }

    public enum Target
    {
        Self = 1,
        Ally = 2,
        Enemy = 3
    }

    public enum TargetType
    {
        Single = 1,
        All = 2,
        Random = 3
    }
}
