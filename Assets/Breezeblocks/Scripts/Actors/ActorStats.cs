using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(ActorManager))]
[RequireComponent(typeof(ActorUI))]
public class ActorStats : MonoBehaviour
{
    private ActorManager _actor = null;
    private ActorUI _ui = null;

    // Health stats
    private int _maxHealth = 0;
    private int _currentHealth = 0;
    private float HealthPercentage { get { return  (float)_currentHealth / _maxHealth; } }

    // Block stats
    private int _currentBlock = 0;

    // Actions stats
    private int _actionsPerTurn = 0;
    private int _currentActions = 0;
    
    // Card buy stats
    private int _cardBuy = 0;
    public int CardBuy => _cardBuy;
    
    // Damage stats
    private int _minDamage = 0;
    private int _maxDamage = 0;
    public int Damage { get { return Random.Range(_minDamage, _maxDamage); } }

    // Status
    private List<StatusEffectInstance> _activeEffects = new List<StatusEffectInstance>();

    // ========================================================================

    #region Initialization
    private void Start()
    {
        _actor = GetComponent<ActorManager>();
        _ui = GetComponent<ActorUI>();

        Initialize();
    }


    private void Initialize()
    {
        _maxHealth = _actor.ActorData.MaxHealth;
        _currentHealth = _maxHealth;
        _actionsPerTurn = _actor.ActorData.ActionsPerTurn;
        _minDamage = _actor.ActorData.MinDamage;
        _maxDamage = _actor.ActorData.MaxDamage;

        _ui.UpdateUI(HealthPercentage, _currentHealth, _maxHealth);
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
    public void AddStatusEffect(UEnums.StatusEffects statusEffect, int amount, int duration)
    {
        StatusEffectInstance newEffect = new StatusEffectInstance(statusEffect, amount, duration);
        _activeEffects.Add(newEffect);
    }

    /// <summary>
    /// Update the status effects each round.
    /// </summary>
    private void UpdateStatusEffects()
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
        int damageAfterBlock = damage - _currentBlock;
        damageAfterBlock = Mathf.Max(0, damageAfterBlock);

        _currentBlock = Mathf.Max(0, _currentBlock - damage);
        _currentHealth -= damageAfterBlock;

        _ui.UpdateUI(HealthPercentage, _currentHealth, _maxHealth);
        Debug.Log("Percentage: " + HealthPercentage);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log($"{_actor.gameObject.name} has died.");
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
}
