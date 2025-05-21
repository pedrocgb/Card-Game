using System.Collections.Generic;
using System.Linq;
using Breezeblocks.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using static UEnums;

[RequireComponent(typeof(ActorManager))]
[RequireComponent(typeof(ActorUI))]
public class ActorStats : MonoBehaviour
{
    #region Variables and Properties
    private ActorManager _actor = null;
    private bool _isPlayerActor = false;

    // Health stats
    private int _maxHealth = 0;
    private int _currentHealth = 0;
    private float HealthPercentage { get { return  (float)_currentHealth / _maxHealth; } }
    private bool _isDead = false;
    public bool IsDead => _isDead;

    // Block stats
    private int _currentBlock = 0;

    // Actions stats
    private int _actionsPerTurn = 0;
    private int _currentActions = 0;
    public int CurrentActions => _currentActions;

    // Card buy stats
    private int _cardBuy = 0;
    public int CardBuy => _cardBuy;
    
    // Damage stats
    private int _minDamage = 0;
    private int _maxDamage = 0;
    public int Damage { get { return Random.Range(_minDamage, _maxDamage); } }

    // Status
    private List<StatusEffectInstance> _activeEffects = new List<StatusEffectInstance>();

    #endregion

    // ========================================================================

    #region Initialization
    private void Start()
    {
        _actor = GetComponent<ActorManager>();
        _isPlayerActor = _actor is PlayerActor;

        Initialize();
    }


    protected virtual void Initialize()
    {
        _maxHealth = _actor.Data.MaxHealth;
        _currentHealth = _maxHealth;
        _actionsPerTurn = _actor.Data.ActionsPerTurn;
        _currentActions = _actionsPerTurn;
        _minDamage = _actor.Data.MinDamage;
        _maxDamage = _actor.Data.MaxDamage;

        UpdateAllUI();
    }

    public void OnNewTurn()
    {
        _currentActions = _actionsPerTurn;

        ResolveAllEffects();
        UpdateStatusDuration();
        _actor.UI.UpdateStatusUI(_activeEffects);
    }
    #endregion

    // ========================================================================

    #region Actions
    public void SpendAction(int Amount)
    {
        _currentActions -= Amount;

        if (_isPlayerActor)
            _actor.UI.UpdateActionsUI(_currentActions, _actionsPerTurn);    
    }
    #endregion

    // ========================================================================

    #region Status Effects
    /// <summary>
    /// Adds a new Status to the actor.
    /// </summary>
    /// <param name="statusEffect"></param>
    /// <param name="amount"></param>
    /// <param name="duration"></param>
    public void AddStatusEffect(StatusEffects statusEffect, int amount, int duration)
    {
        var stackingMode = UIconsDatabase.GetStackingMode(statusEffect);

        List<StatusEffectInstance> existingEffects = _activeEffects.Where(e => e.StatusEffect == statusEffect).ToList();

        if (existingEffects.Count == 0 ||
            stackingMode == StatusStackingMode.None)
        {
            _activeEffects.Add(new StatusEffectInstance(statusEffect, amount, duration, stackingMode));
        }

        foreach (var e in existingEffects)
        {
            switch (stackingMode)
            {
                default:
                case StatusStackingMode.None:
                    // No stacking, do nothing
                    break;
                case StatusStackingMode.RefreshDurationOnly:
                    e.DurationRemaining += duration;
                    break;
                case StatusStackingMode.StackAmountOnly:
                    e.DurationRemaining = Mathf.Max(e.DurationRemaining, 1);
                    e.Amount += amount; 
                    break;
                case StatusStackingMode.StackBoth:
                    e.Amount += amount;
                    e.DurationRemaining += duration;
                    break;
            }
        }
    }

    /// <summary>
    /// Update the status effects each round.
    /// </summary>
    private void UpdateStatusDuration()
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            _activeEffects[i].DurationRemaining--;
            if (_activeEffects[i].DurationRemaining <= 0)
            {
                _activeEffects.RemoveAt(i);
            }
        }
    }

    private void ResolveAllEffects()
    {
        // Resolve all effects
    }

    /// <summary>
    /// Get a specific Status effect amount.
    /// </summary>
    /// <param name="statusEffect"></param>
    /// <returns></returns>
    private int GetTotalEffectAmount(UEnums.StatusEffects statusEffect)
    {
        int totalAmount = 0;
        foreach (var effect in _activeEffects)
        {
            if (effect.StatusEffect == statusEffect)
            {
                totalAmount += effect.Amount;
            }
        }
        return totalAmount;
    }
    #endregion

    // ========================================================================

    #region Damage Effects
    public void TakeDamage(int damage)
    {
        // Calculate damage taken with block
        // Remove from block than remove from health
        int initialBlock = _currentBlock;
        int blockedAmount = Mathf.Min(damage, _currentBlock);
        int damageAfterBlock = Mathf.Max(0, damage - _currentBlock);

        _currentBlock = Mathf.Max(0, _currentBlock - damage);
        _currentHealth -= damageAfterBlock;

        // Log
        string log = $"{_actor.Data.ClassName} takes {damage} damage: " +
            $"{blockedAmount} blocked, {damageAfterBlock} dealt. " +
            $"Block remaining: {_currentBlock}, Health: {_currentHealth}";
        UConsole.Log(log);  

        // Update Actor healthbar
        _actor.UI.UpdateHealthUI(HealthPercentage, _currentHealth, _maxHealth);

        // Play taking damage animation
        FloatingDamage f = ObjectPooler.SpawnFromPool("Floating Damage Text", transform.position, Quaternion.identity).GetComponent<FloatingDamage>();
        f.transform.SetParent(_actor.UI.WorldCanvas.transform);
        f.UpdateText(damageAfterBlock.ToString(), 0.6f);



        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log($"{_actor.gameObject.name} has died.");

        PositionsManager.RemoveActor(_actor);
    }
    #endregion

    // ========================================================================

    #region Healing Effects
    public void Heal(int healAmount)
    {
        _currentHealth += healAmount;
        if (_currentHealth > _maxHealth)
            _currentHealth = _maxHealth;
    }
    #endregion

    // ========================================================================

    public void GainBlock(int blockAmount, int duration)
    {
        AddStatusEffect(UEnums.StatusEffects.Block, blockAmount, duration);
    }

    // ========================================================================

    public void UpdateAllUI()
    {
        _actor.UI.UpdateHealthUI(HealthPercentage, _currentHealth, _maxHealth);
        if (_isPlayerActor)
            _actor.UI.UpdateActionsUI(_currentActions, _actionsPerTurn);
    }

    // ========================================================================
}
