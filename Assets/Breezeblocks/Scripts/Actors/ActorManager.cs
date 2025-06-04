using Sirenix.OdinInspector;
using System.Linq;
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
    protected ActorWorldUI _myUi = null;
    public ActorWorldUI UI => _myUi;
    protected ActorPosition _myPosition = null;
    public ActorPosition Positioning => _myPosition;

    // Actor graphics in the scene
    [FoldoutGroup("Graphics", expanded: true)]
    [SerializeField]
    private GameObject _actorModel = null;
    [FoldoutGroup("Graphics", expanded: true)]
    [SerializeField]
    private Animator _actorAnimator = null;
    [FoldoutGroup("Graphics/Effects", expanded: true)]
    [SerializeField]
    protected GameObject _hostilePositionEffect = null;
    [FoldoutGroup("Graphics/Effects", expanded: true)]
    [SerializeField]
    protected GameObject _allyPositionEffect = null;
    [FoldoutGroup("Graphics/Effects", expanded: true)]
    [SerializeField]
    protected GameObject _hostileTargetEffect = null;
    [FoldoutGroup("Graphics/Effects", expanded: true)]
    [SerializeField]
    protected GameObject _allyTargetEffect = null;
    [FoldoutGroup("Graphics/Effects/Turn", expanded: true)]
    [SerializeField]
    private Color _outlineColor = Color.white;
    [FoldoutGroup("Graphics/Effects/Turn", expanded: true)]
    [SerializeField]
    private float _outlineWidth = 2f;
    private MaterialPropertyBlock _materialBlock = null;
    private SpriteRenderer _spriteRenderer = null;

    // Deck and Hand managers
    private DeckManager _deck = null;
    public DeckManager Deck => _deck;

    protected HandManager _hand = null;
    public HandManager Hand => _hand;

    // Actor stats
    private string _actorName = string.Empty;
    public string ActorName => _actorName;
    private int _initiativeBonus = 0;
    public int InitiativeBonus { get { return _initiativeBonus; } }
    private int _currentInitiative = 0;
    public int CurrentInitiative => _currentInitiative;
    public bool IsMyTurn { get; protected set; } = false;

    // Targeter
    protected bool _targetable = false;
    private bool _highTarget = false;
    protected UEnums.Target _currentAttacker = UEnums.Target.Self;
    #endregion

    // ========================================================================

    #region Initialization
    protected virtual void Awake()
    {
        _deck = GetComponent<DeckManager>();
        _hand = GetComponent<HandManager>();

        _myStats = GetComponent<ActorStats>();
        _myPosition = GetComponent<ActorPosition>();

        _myUi = GetComponent<ActorWorldUI>();

        _spriteRenderer = _actorModel.GetComponent<SpriteRenderer>();
        _actorAnimator = _actorModel.GetComponent<Animator>();

        _materialBlock = new MaterialPropertyBlock();
        _spriteRenderer.GetPropertyBlock(_materialBlock);
    }

    public virtual void Initialize()
    {
        _actorName = UConstants.LIST_OF_NAMES[Random.Range(0, UConstants.LIST_OF_NAMES.Count)];
        gameObject.name = _actorName;
        _actorAnimator.runtimeAnimatorController = _actorData.ActorAnimatorParameter;

        _initiativeBonus = _actorData.InitiativeBonus;
    }
    #endregion

    // ========================================================================

    #region Turn Methods
    public void RollInitiative()
    {
        int init = Random.Range(1, 9) + _initiativeBonus;
        _currentInitiative = init;
        Console.Log($"{name} rolled {init} + {_initiativeBonus} = {init + _initiativeBonus}");
    }

    public virtual void StartNewTurn()
    {
        IsMyTurn = true;
        TurnOutlineEffect(true);
        _myStats.OnNewTurn();        

        _hand.DrawCards(_actorData.CardBuy);
    }

    public virtual void EndTurn()
    {
        IsMyTurn = false;
        TurnOutlineEffect(false);
        TargetingManager.Instance.ClearHightLights();

        _myStats.OnEndTurn();
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
    public void HighTargetActor()
    {
        _highTarget = true;

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
    public void RemoveHighLight()
    {
        _targetable = false;
        _highTarget = false;

        _hostilePositionEffect.SetActive(false);
        _allyPositionEffect.SetActive(false);

        _hostileTargetEffect.SetActive(false);
        _allyTargetEffect.SetActive(false);
    }

    protected void TurnOutlineEffect(bool on)
    {
        _spriteRenderer.GetPropertyBlock(_materialBlock);
        _materialBlock.SetFloat("_OutlineThickness", on ? _outlineWidth : 0f);
        _materialBlock.SetColor("_OutlineColor", on ? _outlineColor : Color.white);
        _spriteRenderer.SetPropertyBlock(_materialBlock);
    }
    #endregion

    // ========================================================================

    #region Pointer Handler Methods
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CombatManager.Instance.IsPlayerTurn) return; // Prevent interaction during enemy turn

        if (!_targetable || _highTarget) return;

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
        if (!CombatManager.Instance.IsPlayerTurn) return; // Prevent interaction during enemy turn

        if (!_targetable || _highTarget) return;

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
        if (!CombatManager.Instance.IsPlayerTurn) return; // Prevent interaction during enemy turn

        if (!_targetable) return;

        TargetingManager.Instance.SetTarget(this);
        PlayerTurnManager.Instance.UseCard();
    }
    #endregion

    // ========================================================================

    #region AI Highlight Methods
    public void ShowTargetFeedback(UEnums.Target TargetType)
    {
        switch (TargetType)
        {
            default:
            case UEnums.Target.Ally:
            case UEnums.Target.Self:
                _allyTargetEffect.SetActive(true);
                break;
            case UEnums.Target.Enemy:
                _hostileTargetEffect.SetActive(true);
                break;
        }
    }

    public void HideTargetFeedBack()
    {
        _hostileTargetEffect.SetActive(false);
        _allyTargetEffect.SetActive(false);
    }
    #endregion

    // ========================================================================
}
