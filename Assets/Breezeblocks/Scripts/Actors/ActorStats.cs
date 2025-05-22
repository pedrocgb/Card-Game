using System.Collections.Generic;
using System.Linq;
using Breezeblocks.Managers;
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

    // Buffs stats

    // Debuffs stats
    private bool _isStunned = false;

    // Actions stats
    private int _unmodifiedActionsPerTurn = 0;
    private int _actionsPerTurn = 0;
    private int _currentActions = 0;
    public int CurrentActions => _currentActions;

    // Card buy stats
    private int _cardBuy = 0;
    public int CardBuy => _cardBuy;

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
        _unmodifiedActionsPerTurn = _actor.Data.ActionsPerTurn;
        _actionsPerTurn = _unmodifiedActionsPerTurn;
        _currentActions = _actionsPerTurn;

        UpdateAllUI();
    }

    public void OnNewTurn()
    {
        if (_isStunned)
            OnEndTurn();

        _currentActions = _actionsPerTurn;

        ResolveStartTurnEffects();
        _actor.UI.UpdateStatusUI(_activeEffects);
    }

    public void OnEndTurn()
    {

        ResolveEndTurnEffects();
        _actor.UI.UpdateStatusUI(_activeEffects);
    }
    #endregion

    // ========================================================================

    #region Actions
    public void SpendAction(int Amount)
    {
        _currentActions -= Amount;

        if (_isPlayerActor)
            _actor.UI.UpdateActionsUI(_currentActions, _unmodifiedActionsPerTurn);    
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

        _actor.UI.UpdateStatusUI(_activeEffects);
    }

    /// <summary>
    /// Update the status effects each round.
    /// </summary>
    private void UpdateAllStatusDuration()
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
    private void UpdateStatusDuration(StatusEffectInstance Effect)
    {
        Effect.DurationRemaining--;
        if (Effect.DurationRemaining <= 0)
        {
            _activeEffects.Remove(Effect);
        }
    }

    private void ResolveEndTurnEffects()
    {
        foreach (StatusEffectInstance effect in _activeEffects)
        {
            switch (effect.StatusEffect)
            {
                case StatusEffects.Weakness:
                    UpdateStatusDuration(effect);
                    break;
                case StatusEffects.Slow:
                    UpdateStatusDuration(effect);
                    break;
                case StatusEffects.Stun:
                    _isStunned = false;
                    UpdateStatusDuration(effect);
                    break;
                case StatusEffects.Restrained:
                    UpdateStatusDuration(effect);
                    break;
                case StatusEffects.Blind:
                    UpdateStatusDuration(effect);
                    break;

                case StatusEffects.Haste:
                    UpdateStatusDuration(effect);
                    break;
            }
        }
    }

    private void ResolveStartTurnEffects()
    {
        foreach (StatusEffectInstance effect in _activeEffects)
        {
            switch (effect.StatusEffect)
            {
                case StatusEffects.Vulnerability:
                    UpdateStatusDuration(effect);
                    break;


                case StatusEffects.Burn:
                    TakeBurnDamage();
                    UpdateStatusDuration(effect);
                    break;
                case StatusEffects.Poison:
                    TakePoisonDamage();
                    UpdateStatusDuration(effect);
                    break;
                case StatusEffects.Bleed:
                    TakeBleedDamage(MoveDamage: false);
                    UpdateStatusDuration(effect);
                    break;


                case StatusEffects.Block:
                    UpdateStatusDuration(effect);
                    break;
                case StatusEffects.Regen:
                    RegenHealing();
                    UpdateStatusDuration(effect);
                    break;
                case StatusEffects.Dodge:
                    UpdateStatusDuration(effect);
                    break;
                case StatusEffects.Hide:
                    UpdateStatusDuration(effect);
                    break;
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
    public void DealDamage(int Damage, ActorManager Target)
    {
        // Calculate damage dealt with weakness
        // Remove from health
        int initialWeakness = GetTotalEffectAmount(StatusEffects.Weakness);
        int damageAfterWeakness = Mathf.Max(0, Damage - initialWeakness);

        // Deal damage
        Target.Stats.TakeDamage(damageAfterWeakness);

        // Log
        string log = $"{_actor.Data.ClassName} deals {damageAfterWeakness} damage to {Target.Data.ClassName}. " +
            $"Weakness: {initialWeakness}, Damage dealt: {damageAfterWeakness}";
        UConsole.Log(log);
    }

    public void TakeDamage(int damage)
    {
        // Calculate damage taken with block
        // Remove from block than remove from health
        int initialBlock = GetTotalEffectAmount(StatusEffects.Block);
        int blockedAmount = Mathf.Min(damage, initialBlock);
        int damageAfterBlock = Mathf.Max(0, damage - initialBlock);

        initialBlock = Mathf.Max(0, initialBlock - damage);
        _currentHealth -= damageAfterBlock;

        // Log
        string log = $"{_actor.Data.ClassName} takes {damage} damage: " +
            $"{blockedAmount} blocked, {damageAfterBlock} dealt. " +
            $"Block remaining: {initialBlock}, Health: {_currentHealth}";
        UConsole.Log(log);  

        // Update Actor healthbar
        _actor.UI.UpdateHealthUI(HealthPercentage, _currentHealth, _maxHealth);

        // Play taking damage animations
        FloatingTextAnimation(damageAfterBlock.ToString(), HealthModColors.BasicDamage);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void TakeBurnDamage()
    {
        int burnDamage = GetTotalEffectAmount(StatusEffects.Burn) * UConstants.BURN_MULTIPLIER;
        if (burnDamage > 0)
        {
            burnDamage = Mathf.Max(0, burnDamage);
            _currentHealth -= burnDamage;

            // Play taking damage animations
            FloatingTextAnimation(burnDamage.ToString(), HealthModColors.BurnDamage);

            // Log
            string log = $"{_actor.Data.ClassName} takes {burnDamage} burning damage: " +
              $"Health: {_currentHealth}";
            UConsole.Log(log);

            if (_currentHealth <= 0 )
            {
                Die();
            }
        }
    }

    private void TakePoisonDamage()
    {
        int poisonDamage = GetTotalEffectAmount(StatusEffects.Poison);
        if (poisonDamage > 0)
        {
            poisonDamage = Mathf.Max(0, poisonDamage);
            _currentHealth -= poisonDamage;

            // Play taking damage animations
            FloatingTextAnimation(poisonDamage.ToString(), HealthModColors.PoisonDamage);

            // Log
            string log = $"{_actor.Data.ClassName} takes {poisonDamage} poison damage: " +
              $"Health: {_currentHealth}";

            if (_currentHealth <= 0)
            {
                Die();
            }
        }
    }

    private void TakeBleedDamage(bool MoveDamage)
    {
        int bleedDamage = GetTotalEffectAmount(StatusEffects.Bleed);
        if (MoveDamage)
        {
            bleedDamage = bleedDamage * UConstants.BLEED_MULTIPLIER;
        }

        if (bleedDamage > 0)
        {
            bleedDamage = Mathf.Max(0, bleedDamage);
            _currentHealth -= bleedDamage;

            // Play taking damage animations
            FloatingTextAnimation(bleedDamage.ToString(), HealthModColors.BasicDamage);

            // Log
            string log = $"{_actor.Data.ClassName} takes {bleedDamage} poison damage: " +
              $"Health: {_currentHealth}";

            if (_currentHealth <= 0)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        _isDead = true;
        PositionsManager.RemoveActor(_actor);
        CombatManager.RemoveCombatent(_actor);

        gameObject.SetActive(false);
    }
    #endregion

    // ========================================================================

    #region Healing Effects
    public void Heal(int healAmount)
    {
        _currentHealth += healAmount;
        // Play taking damage animations
        FloatingTextAnimation(healAmount.ToString(), HealthModColors.Heal);
        if (_currentHealth > _maxHealth)
            _currentHealth = _maxHealth;
    }

    private void RegenHealing()
    {
        int regenAmount = GetTotalEffectAmount(StatusEffects.Regen);
        if (regenAmount > 0)
        {
            regenAmount = Mathf.Max(0, regenAmount);
            _currentHealth += regenAmount;
            // Play taking damage animations
            FloatingTextAnimation(regenAmount.ToString(), HealthModColors.Heal);
            if (_currentHealth > _maxHealth)
                _currentHealth = _maxHealth;
        }
    }
    #endregion

    // ========================================================================

    #region Buff Effects
    public void GainBlock(int blockAmount, int duration)
    {
        AddStatusEffect(UEnums.StatusEffects.Block, blockAmount, duration);
    }

    public void GainRegen(int amount, int duration)
    {
        AddStatusEffect(UEnums.StatusEffects.Regen, amount, duration);
    }
    #endregion

    // ========================================================================

    #region Debuff Effects
    public void SufferWeakness(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Weakness, amount, duration);
    }

    public void SufferVulnerability(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Vulnerability, amount, duration);
    }

    public void SufferSlow(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Slow, amount, duration);
    }

    public void SufferStun(int duration)
    {
        _isStunned = true;
        AddStatusEffect(StatusEffects.Stun, 1, duration);
    }

    public void SufferBurn(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Burn, amount, duration);
    }

    public void SufferPoison(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Poison, amount, duration);
    }

    public void SufferRestrained(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Restrained, amount, duration);
    }

    public void SufferBlind(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Blind, amount, duration);
    }

    public void SufferBleed(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Bleed, amount, duration);
    }
    #endregion

    // ========================================================================

    public void UpdateAllUI()
    {
        _actor.UI.UpdateHealthUI(HealthPercentage, _currentHealth, _maxHealth);
        if (_isPlayerActor)
            _actor.UI.UpdateActionsUI(_currentActions, _unmodifiedActionsPerTurn);
    }

    private void FloatingTextAnimation(string Text, HealthModColors DamageMod)
    {
        // Play taking damage animation
        FloatingDamage f = ObjectPooler.SpawnFromPool("Floating Damage Text", transform.position, Quaternion.identity).GetComponent<FloatingDamage>();
        f.transform.SetParent(_actor.UI.WorldCanvas.transform);
        f.UpdateText(Text, 0.6f, DamageMod);
    }

    // ========================================================================
}
