using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Actor Data", menuName = "Breezeblocks/New Actor Data", order = 1)]
public class ActorData : ScriptableObject
{
    [FoldoutGroup("Actor Info", expanded:true)]
    [SerializeField]
    private string _className = string.Empty;
    public string ClassName => _className;
    [FoldoutGroup("Actor Info", expanded: true)]
    [SerializeField]
    private int _baseActionsPerTurn = 1;
    public int ActionsPerTurn => _baseActionsPerTurn;
    [FoldoutGroup("Actor Info", expanded: true)]
    [SerializeField]
    private int _baseMaxHealth = 20;
    public int MaxHealth => _baseMaxHealth;
    [FoldoutGroup("Actor Info", expanded: true)]
    [SerializeField]
    private int _initiativeBonus = 0;
    public int InitiativeBonus => _initiativeBonus;

    // ========================================================================

    [FoldoutGroup("Actor Info/Damage", expanded: true)]
    [SerializeField]
    private int _baseMinDamage = 1;
    public int MinDamage => _baseMinDamage;
    [FoldoutGroup("Actor Info/Damage", expanded: true)]
    [SerializeField]
    private int _baseMaxDamage = 5;
    public int MaxDamage => _baseMaxDamage;

    // ========================================================================

    [FoldoutGroup("Actor Info/Cards", expanded: true)]
    [SerializeField]
    private List<CardData> _startingCards = new List<CardData>();
    public List<CardData> StartingCards => _startingCards;
    [FoldoutGroup("Actor Info/Cards", expanded: true)]
    [SerializeField]
    private int _baseCardBuy = 2;
    public int CardBuy => _baseCardBuy;

    // ========================================================================
}