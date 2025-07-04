using Sirenix.OdinInspector;
using System;
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
    private ActorRelics _myRelics = null;
    public ActorRelics MyRelics => _myRelics;

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
    private Material _defaultMaterial = null;
    [FoldoutGroup("Graphics/Effects/Turn", expanded: true)]
    [SerializeField]
    private Material _outlineMaterial = null;

    private Material _defaultMaterialInstance;
    private Material _outlineMaterialInstance;
    private SpriteRenderer _spriteRenderer;

    // Deck and Hand managers
    protected DeckManager _deck = null;
    public DeckManager Deck => _deck;

    protected HandManager _hand = null;
    public HandManager Hand => _hand;

    // Actor stats
    private string _actorName = string.Empty;
    public string ActorName => _actorName;
    private int _initiativeBonus = 0;
    public int InitiativeBonus => _initiativeBonus;
    private int _currentInitiative = 0;
    public int CurrentInitiative => _currentInitiative;
    public bool IsMyTurn { get; protected set; } = false;

    // Targeter
    protected bool _targetable = false;
    private bool _highTarget = false;
    protected UEnums.Target _currentAttacker = UEnums.Target.Self;

    // Actions
    public event Action TurnStartEvent;
    public event Action TurnEndEvent;
    #endregion

    // ========================================================================

    #region Initialization
    protected virtual void Awake()
    {
        _deck = GetComponent<DeckManager>();
        _hand = GetComponent<HandManager>();
        _myRelics = GetComponent<ActorRelics>();

        _myStats = GetComponent<ActorStats>();
        _myPosition = GetComponent<ActorPosition>();
        _myUi = GetComponent<ActorWorldUI>();

        _spriteRenderer = _actorModel.GetComponent<SpriteRenderer>();

        // Create unique material instances for this actor
        _defaultMaterialInstance = new Material(_defaultMaterial);
        _outlineMaterialInstance = new Material(_outlineMaterial);
        // Start with default material
        _spriteRenderer.material = _defaultMaterialInstance;
    }

    public virtual void Initialize()
    {
        string aName = string.Empty;
        if (_actorData.ActorName == "" ||
            _actorData.ActorName == string.Empty)
            _actorName = UConstants.LIST_OF_NAMES[UnityEngine.Random.Range(0, UConstants.LIST_OF_NAMES.Count)];
        else
            _actorName = _actorData.ActorName;

        gameObject.name = _actorName;
        _actorAnimator.runtimeAnimatorController = _actorData.ActorAnimatorParameter;

        _initiativeBonus = _actorData.InitiativeBonus;
    }
    #endregion

    // ========================================================================

    #region Turn Methods
    public void RollInitiative()
    {
        int roll = UnityEngine.Random.Range(1, 9);
        int init = roll + _initiativeBonus;
        _currentInitiative = init;
        Console.Log($"{name} rolled {roll} + {_initiativeBonus} = {init}");
    }

    public virtual void OnTurnStart()
    {
        IsMyTurn = true;
        TurnOutlineEffect(true);
        _myStats.OnNewTurn();
        _myUi.UpdateTurnMarker(false);

       _hand.DrawCards(_myStats.CardBuy);
    }

    public virtual void OnTurnEnd()
    {
        IsMyTurn = false;
        TurnOutlineEffect(false);
        TargetingManager.Instance.ClearHightLights();

        _myStats.OnEndTurn();

        TurnEndEvent?.Invoke();
    }

    protected void RaiseTurnStartEvent()
    {
        TurnStartEvent?.Invoke();
    }
    #endregion

    // ========================================================================

    #region Animations and Effects Methods
    public void HightLightActor(UEnums.Target TargetFaction)
    {
        switch (TargetFaction)
        {
            case UEnums.Target.Enemy:
                _hostilePositionEffect.SetActive(true);
                break;
            default:
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
            case UEnums.Target.Enemy:
                _hostileTargetEffect.SetActive(true);
                break;
            default:
                _allyTargetEffect.SetActive(true);
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

    public void TurnOutlineEffect(bool on)
    {
        // Swap between default and outline materials
        _spriteRenderer.material = on ? _outlineMaterialInstance : _defaultMaterialInstance;
    }
    #endregion

    // ========================================================================

    #region Pointer Handler Methods
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CombatManager.Instance.IsPlayerTurn || !_targetable || _highTarget)
            return;

        switch (_currentAttacker)
        {
            case UEnums.Target.Enemy:
                _hostileTargetEffect.SetActive(true);
                break;
            default:
                _allyTargetEffect.SetActive(true);
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!CombatManager.Instance.IsPlayerTurn || !_targetable || _highTarget)
            return;

        switch (_currentAttacker)
        {
            case UEnums.Target.Enemy:
                _hostileTargetEffect.SetActive(false);
                break;
            default:
                _allyTargetEffect.SetActive(false);
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CombatManager.Instance.IsPlayerTurn || !_targetable)
            return;

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
            case UEnums.Target.Enemy:
                _hostileTargetEffect.SetActive(true);
                break;
            default:
                _allyTargetEffect.SetActive(true);
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

    public void ChangeInitiative(int amount)
    {
        _initiativeBonus += (amount * UConstants.INITIATIVE_STATUS_MULTIPLIER);
    }

    public void ResetInitiative()
    {
        _initiativeBonus = _actorData.InitiativeBonus;
    }

    // ========================================================================
}
