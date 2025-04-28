using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hero Class", menuName = "Breezeblocks/New Hero Class", order = 1)]
public class ClassesData : ScriptableObject
{
    [FoldoutGroup("Class Info", expanded:true)]
    [SerializeField]
    private string _className = string.Empty;
    public string ClassName => _className;
    [FoldoutGroup("Class Info", expanded: true)]
    [SerializeField]
    private int _baseActionsPerTurn = 1;
    public int ActionsPerTurn => _baseActionsPerTurn;
    [FoldoutGroup("Class Info", expanded: true)]
    [SerializeField]
    private int _baseMaxHealth = 20;
    public int MaxHealth => _baseMaxHealth;
    [FoldoutGroup("Class Info", expanded: true)]
    [SerializeField]
    private int _initiativeBonus = 0;
    public int InitiativeBonus => _initiativeBonus;


    [FoldoutGroup("Damage Info", expanded: true)]
    [SerializeField]
    private int _baseMinDamage = 1;
    public int MinDamage => _baseMinDamage;
    [FoldoutGroup("Damage Info", expanded: true)]
    [SerializeField]
    private int _baseMaxDamage = 5;
    public int MaxDamage => _baseMaxDamage;

    [FoldoutGroup("Deck Info", expanded: true)]
    [SerializeField]
    private List<CardData> _startingCards = new List<CardData>();
    public List<CardData> StartingCards => _startingCards;
    [FoldoutGroup("Deck Info", expanded: true)]
    [SerializeField]
    private int _baseCardBuy = 2;
    public int CardBuy => _baseCardBuy;
}