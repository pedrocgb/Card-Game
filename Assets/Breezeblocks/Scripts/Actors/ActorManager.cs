using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(DeckManager))]
public abstract class ActorManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Variables and Properties
    // Actor data
    [FoldoutGroup("Data", expanded: true)]
    [SerializeField]
    private ActorData _actorData = null;
    public ActorData ActorData => _actorData;

    // Actor position
    [FoldoutGroup("Position", expanded: true)]
    [SerializeField]
    private UEnums.Positions _currentPosition = UEnums.Positions.Front;
    public UEnums.Positions CurrentPosition
    {
        get { return _currentPosition; }
        set { _currentPosition = value; }
    }

    // Actor graphics in the scene
    [FoldoutGroup("Graphics", expanded: true)]
    [SerializeField]
    private GameObject _actorModel = null;
    private SpriteRenderer _spriteRenderer = null;
    [FoldoutGroup("Graphics/Effects", expanded: true)]
    [SerializeField]
    private GameObject _hostilePositionEffect = null;
    [FoldoutGroup("Graphics/Effects", expanded: true)]
    [SerializeField]
    private GameObject _allyPositionEffect = null;
    [FoldoutGroup("Graphics/Effects", expanded: true)]
    [SerializeField]
    private GameObject _hostileTargetEffect = null;
    [FoldoutGroup("Graphics/Effects", expanded: true)]
    [SerializeField]
    private GameObject _allyTargetEffect = null;

    // Deck and Hand managers
    private DeckManager _deck = null;
    public DeckManager Deck => _deck;

    private HandManager _hand = null;
    public HandManager Hand => _hand;

    // Actor stats
    private int _maxHealth = 0;
    private int _currentHealth = 0;

    private int _actionsPerTurn = 0;
    private int _currentActions = 0;

    private int _cardBuy = 0;
    public int CardBuy => _cardBuy;

    private int _minDamage = 0;
    private int _maxDamage = 0;
    public int Damage { get { return Random.Range(_minDamage, _maxDamage); } }

    private int _initiativeBonus = 0;
    public int InitiativeBonus { get { return _initiativeBonus; } }
    private int _currentInitiative = 0;
    public int CurrentInitiative
    {
        get { return _currentInitiative; }
        set { _currentInitiative = value; }
    }

    private bool _targetable = false;
    private UEnums.Target _currentAttacker = UEnums.Target.Self;

    protected bool _isMyTurn = false;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        _deck = GetComponent<DeckManager>();
        _hand = GetComponent<HandManager>();
        _spriteRenderer = _actorModel.GetComponent<SpriteRenderer>();

        Initialize();
    }

    private void Initialize()
    {
        _maxHealth = _actorData.MaxHealth;
        _currentHealth = _maxHealth;
        _actionsPerTurn = _actorData.ActionsPerTurn;
        _initiativeBonus = _actorData.InitiativeBonus;
        _minDamage = _actorData.MinDamage;
        _maxDamage = _actorData.MaxDamage;
    }
    #endregion

    // ========================================================================

    #region Turn Methods
    public void StartNewTurn()
    {
        _currentActions = _actionsPerTurn;
    }

    public void EndTurn()
    {

    }
    #endregion

    // ========================================================================

    #region Animations and Effects Methods
    public void HightLightActor(UEnums.Target TargetFaction)
    {
        switch (TargetFaction)
        {
            default:
            case UEnums.Target.Enemy:
                _hostilePositionEffect.SetActive(true);                
                break;
            case UEnums.Target.Ally:
                _allyPositionEffect.SetActive(true);                
                break;
            case UEnums.Target.Self:
                _allyPositionEffect.SetActive(true);                
                break;
        }

        _currentAttacker = TargetFaction;
        _targetable = true;
    }

    public void RemoveHighLight()
    {
        _targetable = false;
        _hostilePositionEffect.SetActive(false);
        _allyPositionEffect.SetActive(false);
    }
    #endregion

    // ========================================================================

    #region Pointer Handler Methods
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_targetable) return;

        switch (_currentAttacker)
        {
            default:
            case UEnums.Target.Self:
                _allyTargetEffect.SetActive(true);
                break;
            case UEnums.Target.Ally:
                _allyTargetEffect.SetActive(true);
                break;
            case UEnums.Target.Enemy:
                _hostileTargetEffect.SetActive(true);
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_targetable) return;

        switch (_currentAttacker)
        {
            default:
            case UEnums.Target.Self:
                _allyTargetEffect.SetActive(false);
                break;
            case UEnums.Target.Ally:
                _allyTargetEffect.SetActive(false);
                break;
            case UEnums.Target.Enemy:
                _hostileTargetEffect.SetActive(false);
                break;
        }
    }
    #endregion

    // ========================================================================
}
