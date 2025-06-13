using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Breezeblocks/New Card", order = 1)]
public class CardData : ScriptableObject
{
    // Card basic info
    [FoldoutGroup("Card Info", expanded: true)]
    [FoldoutGroup("Card Info/Base", expanded: true)]
    [SerializeField]
    private string _cardName = string.Empty;
    public string CardName => _cardName;
    [FoldoutGroup("Card Info/Base", expanded: true)]
    [SerializeField]
    private string _cardDescription = string.Empty;
    public string CardDescription => _cardDescription;
    [FoldoutGroup("Card Info/Base", expanded: true)]
    [SerializeField]
    private string _cardLore = string.Empty;
    public string CardLore => _cardLore;

    [FoldoutGroup("Card Info", expanded: true)]
    [FoldoutGroup("Card Info/Base", expanded: true)]
    [SerializeField]
    private UEnums.CardOrigin _cardOrigin = UEnums.CardOrigin.Racial;
    public UEnums.CardOrigin CardOrigin => _cardOrigin;
    [FoldoutGroup("Card Info/Base", expanded: true)]
    [SerializeField]
    private UEnums.CardRarity _cardRarity = UEnums.CardRarity.Common;
    public UEnums.CardRarity CardRarity => _cardRarity;

    // Card type
    [FoldoutGroup("Card Info/Base", expanded: true)]
    [SerializeField]
    private UEnums.CardTypes _cardType = UEnums.CardTypes.Attack;
    public UEnums.CardTypes CardType => _cardType;

    // ========================================================================

    [FoldoutGroup("Card Info/Images", expanded: true)]
    [SerializeField]
    [PreviewField(height: 100, alignment: ObjectFieldAlignment.Left)]
    private Sprite _cardImage = null;
    public Sprite CardImage => _cardImage;
    [FoldoutGroup("Card Info/Images", expanded: true)]
    [SerializeField]
    [PreviewField(height: 100, alignment: ObjectFieldAlignment.Left)]
    private Sprite _cardBackImage = null;
    public Sprite CardBackImage => _cardBackImage;

    // ========================================================================

    // Card Effects
    [FoldoutGroup("Card Info/Effects", expanded: true)]
    [SerializeField]
    private int _actionCost = 0;
    public int ActionCost => _actionCost;
    [FoldoutGroup("Card Info/Effects", expanded: true)]
    [SerializeField]
    private List<EffectBlock> _cardEffects = new List<EffectBlock>();
    public List<EffectBlock> CardEffects => _cardEffects;

    // ========================================================================

    // Usable Positions
    [FoldoutGroup("Positions", expanded: true)]
    [SerializeField]
    private List<UEnums.Positions> _positions = new List<UEnums.Positions>();
    public List<UEnums.Positions> Positions => _positions;
    [FoldoutGroup("Positions", expanded: true)]
    [PreviewField(height: 100, alignment: ObjectFieldAlignment.Left)]
    [SerializeField]
    private Sprite _posIcon = null;
    public Sprite PositionIcon => _posIcon;

    // ========================================================================

    // Targeting
    [FoldoutGroup("Targets", expanded: true)]
    [SerializeField]
    private UEnums.Target _targetType = UEnums.Target.Self;
    public UEnums.Target TargetType => _targetType;
    [FoldoutGroup("Targets", expanded: true)]
    [SerializeField]
    private UEnums.TargetAmount _targetScope = UEnums.TargetAmount.Single;
    public UEnums.TargetAmount TargetScope => _targetScope;
    [FoldoutGroup("Targets", expanded: true)]
    [HideIf("_targetType", UEnums.Target.Self)]
    [SerializeField]
    private List<UEnums.Positions> _targetPositions = new List<UEnums.Positions>();
    public List<UEnums.Positions> TargetPositions => _targetPositions;
    [FoldoutGroup("Targets", expanded: true)]
    [ShowIf("_targetType", UEnums.Target.Ally)]
    [SerializeField]
    private bool _canTargetSelf = false;
    public bool CanTargetSelf => _canTargetSelf;
    [FoldoutGroup("Targets", expanded: true)]
    [SerializeField]
    [PreviewField(height:100, alignment: ObjectFieldAlignment.Left)]
    private Sprite _targetIcon = null;
    public Sprite TargetIcon => _targetIcon;

    // ========================================================================
}
