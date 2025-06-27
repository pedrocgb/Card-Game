using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Actor Data", menuName = "Breezeblocks/New Actor Data", order = 1)]
public class ActorData : ScriptableObject
{
    [FoldoutGroup("Actor Info", expanded:true)]
    [InfoBox("Actor Name, leave empty for a random name in the name generator list from the Game Constants.", InfoMessageType.Warning)]
    [SerializeField]
    private string _actorName = string.Empty;
    public string ActorName => _actorName;
    [FoldoutGroup("Actor Info", expanded: true)]
    [SerializeField]
    [PreviewField(height: 100, alignment: ObjectFieldAlignment.Left)]
    private Sprite _portrait = null;
    public Sprite Portrait => _portrait;
    [FoldoutGroup("Actor Info", expanded: true)]
    [SerializeField]
    private RuntimeAnimatorController _actorAnimatorParameter = null;
    public RuntimeAnimatorController ActorAnimatorParameter => _actorAnimatorParameter;

    // ========================================================================

    [FoldoutGroup("Actor Info/Race", expanded: true)]
    [SerializeField]
    private RacesData _actorRace = null;
    public RacesData ActorRace => _actorRace;
    [FoldoutGroup("Actor Info/Race", expanded: true)]
    [SerializeField]
    private List<CardData> _startingCards = new List<CardData>();
    public List<CardData> StartingCards => _startingCards;

    // ========================================================================

    [FoldoutGroup("Actor Info/Specialization", expanded: true)]
    [SerializeField]
    private bool _hasSpecialization = false;
    public bool HasSpecialization => _hasSpecialization;
    [FoldoutGroup("Actor Info/Specialization", expanded: true)]
    [SerializeField]
    [ShowIf("_hasSpecialization")]
    private SpecializationData _actorSpecialization = null;
    public SpecializationData ActorSpecialization => _actorSpecialization;
    [FoldoutGroup("Actor Info/Specialization", expanded: true)]
    [SerializeField]
    [ShowIf("_hasSpecialization")]
    private List<CardData> _startingSpecializedCards = new List<CardData>();
    public List<CardData> StartingSpecializedCards => _startingSpecializedCards;

    // ========================================================================

    [FoldoutGroup("Actor Info/Stats", expanded: true)]
    [SerializeField]
    private int _baseMaxHealth = 0;
    public int MaxHealth => _baseMaxHealth;
    [FoldoutGroup("Actor Info/Stats", expanded: true)]
    [SerializeField]
    private int _baseActionsPerTurn = 0;
    public int ActionsPerTurn => _baseActionsPerTurn;
    [FoldoutGroup("Actor Info/Stats", expanded: true)]
    [SerializeField]
    private int _initiativeBonus = 0;
    public int InitiativeBonus => _initiativeBonus;


    // ========================================================================

    [FoldoutGroup("Actor Info/Cards", expanded: true)]
    [SerializeField]
    private int _baseCardBuy = 2;
    public int CardBuy => _baseCardBuy;
    [FoldoutGroup("Actor Info/Cards", expanded: true)]
    [SerializeField]
    private int _maxHandSize = 6;
    public int MaxHandSize => _maxHandSize;

    // ========================================================================

    [FoldoutGroup("Actor Info/Rewards", expanded: true)]
    [SerializeField]
    private int _minGold = 0;
    [FoldoutGroup("Actor Info/Rewards", expanded: true)]
    [SerializeField]
    private int _maxGold = 0;
    public int GenerateGold { get { return Random.Range(_minGold, _maxGold); } }

    [FoldoutGroup("Actor Info/Rewards", expanded: true)]
    [SerializeField]
    private int _minSoul = 0;
    [FoldoutGroup("Actor Info/Rewards", expanded: true)]
    [SerializeField]
    private int _maxSoul = 0;
    public int GenerateSoul { get { return Random.Range(_minSoul, _maxSoul); } }

    // ========================================================================
}