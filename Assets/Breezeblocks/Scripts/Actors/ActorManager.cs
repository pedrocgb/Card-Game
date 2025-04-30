using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(DeckManager))]
public abstract class ActorManager : MonoBehaviour
{
    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField]
    private ActorData _actorData = null;

    private DeckManager _deck = null;
    public DeckManager Deck => _deck;

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

    protected bool _isMyTurn = false;

    private void Awake()
    {
        _deck = GetComponent<DeckManager>();
    }

    private void Start()
    {
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

    public void StartNewTurn()
    {
        _currentActions = _actionsPerTurn;
    }

    public void EndTurn()
    {

    }

    public virtual void StartTurn()
    {

    }
}
