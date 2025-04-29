using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Breezeblocks/New Card", order = 1)]
public class CardData : ScriptableObject
{
    [FoldoutGroup("Card Info", expanded: true)]
    [SerializeField]
    private string _cardName = string.Empty;
    public string CardName => _cardName;
    [FoldoutGroup("Card Info", expanded: true)]
    [SerializeField]
    private string _cardDescription = string.Empty;
    public string CardDescription => _cardDescription;
    [FoldoutGroup("Card Info", expanded: true)]
    [SerializeField]
    private Sprite _cardImage = null;
    public Sprite CardImage => _cardImage;
    [FoldoutGroup("Card Info", expanded: true)]
    [SerializeField]
    private int _cardCost = 0;
    public int CardCost => _cardCost;

    [Space(20)]
    [FoldoutGroup("Card Info", expanded: true)]
    [SerializeField]
    private UEnums.CardTypes _cardType = UEnums.CardTypes.Attack;
    public UEnums.CardTypes CardType => _cardType;


   
    [FoldoutGroup("Positions", expanded: true)]
    [HideIf("_target",UEnums.Target.Self)]
    [SerializeField]
    private List<UEnums.Positions> _positions = new List<UEnums.Positions>();
    public List<UEnums.Positions> Positions => _positions;
    [FoldoutGroup("Positions", expanded: true)]
    [SerializeField]
    private Sprite _posIcon = null;
    public Sprite PositionIcon => _posIcon;


    [FoldoutGroup("Targets", expanded: true)]
    [SerializeField]
    private UEnums.Target _target = UEnums.Target.Self;
    public UEnums.Target Target => _target;
    [FoldoutGroup("Targets", expanded: true)]
    [SerializeField]
    private UEnums.TargetType _targetType = UEnums.TargetType.Single;
    public UEnums.TargetType TargetType => _targetType;
    [FoldoutGroup("Targets", expanded: true)]
    [HideIf("_target", UEnums.Target.Self)]
    [SerializeField]
    private List<UEnums.Positions> _targetPositions = new List<UEnums.Positions>();
    public List<UEnums.Positions> TargetPositions => _targetPositions;
    [FoldoutGroup("Targets", expanded: true)]
    [SerializeField]
    private Sprite _targetIcon = null;
    public Sprite TargetIcon => _targetIcon;

}
