using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(DeckManager))]
public abstract class ActorManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Variables and Properties
    // Actor data
    [FoldoutGroup("Data", expanded: true)]
    [SerializeField]
    protected ActorData _actorData = null;
    public ActorData Data => _actorData;
    protected ActorStats _myStats = null;
    public ActorStats Stats => _myStats;
    protected ActorUI _myUi = null;
    public ActorUI UI => _myUi;
    protected ActorPosition _myPosition = null;
    public ActorPosition Positioning => _myPosition;

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

    private int _initiativeBonus = 0;
    public int InitiativeBonus { get { return _initiativeBonus; } }
    private int _currentInitiative = 0;
    public int CurrentInitiative
    {
        get { return _currentInitiative; }
        set { _currentInitiative = value; }
    }

    // Targeter
    private bool _targetable = false;
    private UEnums.Target _currentAttacker = UEnums.Target.Self;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        _deck = GetComponent<DeckManager>();
        _hand = GetComponent<HandManager>();
        _myStats = GetComponent<ActorStats>();
        _myPosition = GetComponent<ActorPosition>();
        _myUi = GetComponent<ActorUI>();
        _spriteRenderer = _actorModel.GetComponent<SpriteRenderer>();

        Initialize();
    }

    protected virtual void Initialize()
    { 
        _initiativeBonus = _actorData.InitiativeBonus;
    }
    #endregion

    // ========================================================================

    #region Turn Methods
    public void StartNewTurn()
    {
        
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

        _hostileTargetEffect.SetActive(false);
        _allyTargetEffect.SetActive(false);
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_targetable) return;

        TargetingManager.Instance.SetTarget(this);
        PlayerTurnManager.Instance.UseCard();
    }
    #endregion

    // ========================================================================

}
