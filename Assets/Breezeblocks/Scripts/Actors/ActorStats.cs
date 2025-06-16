using System.Collections.Generic;
using System.Linq;
using Breezeblocks.Managers;
using UnityEngine;
using static UEnums;

[RequireComponent(typeof(ActorManager))]
[RequireComponent(typeof(ActorWorldUI))]
public class ActorStats : MonoBehaviour
{
    #region Variables and Properties
    private ActorManager _actor = null;
    private ActorWorldUI _ui = null;
    private bool _isPlayerActor = false;

    // Health stats
    private int _maxHealth = 0;
    public int MaxHealth => _maxHealth;
    private int _currentHealth = 0;
    public int CurrentHealth => _currentHealth;
    public float HealthPercentage { get { return  (float)_currentHealth / _maxHealth; } }
    private bool _isDead = false;
    public bool IsDead => _isDead;

    // Buffs stats

    // Debuffs stats
    private bool _isStunned = false;
    public bool IsStunned => _isStunned;
    public bool IsRestrained
    {
        get
        {
           return GetTotalEffectAmount(StatusEffects.Restrained) > 0;
        }
    }

    // Actions stats
    private int _unmodifiedActionsPerTurn = 0;
    private int _actionsPerTurn = 0;
    public int ActionsPerTurn => _actionsPerTurn;
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
    private void Awake()
    {
        _actor = GetComponent<ActorManager>();
        _ui = GetComponent<ActorWorldUI>();
    }
    private void Start()
    {        
        _isPlayerActor = _actor is PlayerActor;
    }

    public void Initialize()
    {
        _isDead = false;
        _maxHealth = _actor.Data.MaxHealth;
        _currentHealth = _maxHealth;
        _unmodifiedActionsPerTurn = _actor.Data.ActionsPerTurn;
        _actionsPerTurn = _unmodifiedActionsPerTurn;
        _currentActions = _actionsPerTurn;
        _cardBuy = _actor.Data.CardBuy;

        _ui.UpdateHealthUI();
        if (_actor.IsMyTurn)
        {
            ActorsUI.UpdateActionsInterface(_actor.Stats.CurrentActions, _actor.Stats.ActionsPerTurn);
        }
    }

    public void OnNewTurn()
    {
        CalculateActions();

        ResolveStartTurnEffects();
        _actor.UI.UpdateStatusUI(_activeEffects);

        if (_actor.Data.HasSpecialization)
        {
            ActorsUI.UpdateUserInterface(_actor.ActorName, _actor.Data.ActorRace.RaceName, _actor.Data.ActorSpecialization.SpecializationName,
                _currentHealth, _maxHealth, _currentActions, _actionsPerTurn, HealthPercentage,
                _actor.Data.Portrait, _actor.Deck.CurrentDeck.Count, _actor.Deck.DiscardPile.Count);
        }
        else
        {
            ActorsUI.UpdateUserInterface(_actor.ActorName, _actor.Data.ActorRace.RaceName, string.Empty,
               _currentHealth, _maxHealth, _currentActions, _actionsPerTurn, HealthPercentage,
               _actor.Data.Portrait, _actor.Deck.CurrentDeck.Count, _actor.Deck.DiscardPile.Count);
        }
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

        if (_actor.IsMyTurn)
            ActorsUI.UpdateActionsInterface(_currentActions, _actionsPerTurn);
    }

    private void CalculateActions()
    {
        _currentActions = _actionsPerTurn;

        _currentActions += GetTotalEffectAmount(StatusEffects.Haste);
        _currentActions -= GetTotalEffectAmount(StatusEffects.Slow);
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

        if (existingEffects.Count == 0)
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
    public void UpdateAllStatusDuration()
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

    public void RemoveAllStatusEffects()
    {
        for (int i = 0; i < _activeEffects.Count; i++)
        {
            _activeEffects[i].DurationRemaining = 0;
            _activeEffects[i].Amount = 0;
            _activeEffects.RemoveAt(i);
        }

        _actor.UI.UpdateStatusUI(_activeEffects);
    }

    /// <summary>
    /// Removes 1 (equivalent) turn duration from a status effect. 
    /// When the duration reaches 0 the status is removed.
    /// </summary>
    /// <param name="Effect"></param>
    private void UpdateStatusDuration(StatusEffectInstance Effect)
    {
        Effect.DurationRemaining--;
        if (Effect.DurationRemaining <= 0)
        {
            _activeEffects.Remove(Effect);
        }
    }

    /// <summary>
    /// Resolve the end turn effects. Effects that will lose 1 durability as soon as the turn ends.
    /// </summary>
    private void ResolveEndTurnEffects()
    {
        foreach (StatusEffectInstance effect in _activeEffects.ToList())
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
                case StatusEffects.Lock:
                    UpdateStatusDuration(effect);
                    _actor.Hand.UnlockAllCards();
                    Console.Log($"{_actor.Data.ActorName} resolving locked cards: unlocking cards.");
                    break;

                case StatusEffects.Haste:
                    UpdateStatusDuration(effect);
                    break;
                case StatusEffects.Toughness:
                    UpdateStatusDuration(effect);
                    break;
            }
        }
    }

    /// <summary>
    /// Resolver start turn effects, effects that will resolve and lose 1 durability as soon as the turn starts.
    /// </summary>
    private void ResolveStartTurnEffects()
    {
        foreach (StatusEffectInstance effect in _activeEffects.ToList())
        {
            switch (effect.StatusEffect)
            {
                case StatusEffects.Vulnerability:
                    UpdateStatusDuration(effect);
                    break;
                case StatusEffects.Lock:
                    Console.Log($"{_actor.Data.ActorName} locking cards.");
                    _actor.Hand.LockRandomCards(effect.Amount);
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
                case StatusEffects.Riposte:
                    UpdateStatusDuration(effect);
                    break;
                case StatusEffects.Haste:
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
        // Blind check
        if (GetTotalEffectAmount(StatusEffects.Blind) > 0)
        {
            var blind = _activeEffects.FirstOrDefault(e => e.StatusEffect == StatusEffects.Blind);
            if (blind != null)
                UpdateStatusDuration(blind);
            _actor.UI.UpdateStatusUI(_activeEffects);

            FloatingTextManager.SpawnText(transform.position, "MISS!", HealthModColors.Dodge);
            Console.Log($"{_actor.ActorName} misses the attack!");
            return;
        }

        // Calculate damage dealt with weakness
        int initialWeakness = GetTotalEffectAmount(StatusEffects.Weakness);
        int damageAfterWeakness = Mathf.Max(0, Damage - initialWeakness);

        // Calculate damage dealth with bonus damage from Toughness
        int initialToughness = GetTotalEffectAmount(StatusEffects.Toughness);

        int finalDamage = Mathf.Max(0, damageAfterWeakness + initialToughness);

        // Deal damage
        Target.Stats.TakeDamage(finalDamage, _actor);

        // Log
        string log = $"{_actor.ActorName} deals {finalDamage} damage to {Target.Data.ActorName}. " +
            $"Weakness: {initialWeakness}, Toughness: {initialToughness}, Damage dealt: {finalDamage}";
        Console.Log(log);
    }

    public void DealBlockDamage(ActorManager Target)
    {
        // Blind check
        if (GetTotalEffectAmount(StatusEffects.Blind) > 0)
        {
            var blind = _activeEffects.FirstOrDefault(e => e.StatusEffect == StatusEffects.Blind);
            if (blind != null)
                UpdateStatusDuration(blind);
            _actor.UI.UpdateStatusUI(_activeEffects);

            FloatingTextManager.SpawnText(transform.position, "MISS!", HealthModColors.Dodge);
            Console.Log($"{_actor.ActorName} misses the attack!");
            return;
        }

        // Calculate damage dealt with weakness
        int initialWeakness = GetTotalEffectAmount(StatusEffects.Weakness);
        int damageAfterWeakness = Mathf.Max(0, GetTotalEffectAmount(StatusEffects.Block) - initialWeakness);

        // Calculate damage dealth with bonus damage from Toughness
        int initialToughness = GetTotalEffectAmount(StatusEffects.Toughness);

        int finalDamage = Mathf.Max(0, damageAfterWeakness + initialToughness);

        // Deal damage
        Target.Stats.TakeDamage(finalDamage, _actor);

        // Log
        string log = $"{_actor.ActorName} deals {finalDamage} damage to {Target.Data.ActorName}. " +
            $"Weakness: {initialWeakness}, Toughness: {initialToughness}, Damage dealt: {finalDamage}";
        Console.Log(log);
    }

    public void TakeDamage(int damage, ActorManager source)
    {
        // Dodge check
        if (GetTotalEffectAmount(StatusEffects.Dodge) > 0)
        {
            var dodge = _activeEffects.FirstOrDefault(e => e.StatusEffect == StatusEffects.Dodge);
            if (dodge != null)
                UpdateStatusDuration(dodge);
            _actor.UI.UpdateStatusUI(_activeEffects);

            FloatingTextManager.SpawnText(transform.position, "DODGE!", HealthModColors.Dodge);
            Console.Log($"{_actor.ActorName} dodges the attack!");
            return;
        }

        // Calculate extra damage taken from vulnerability
        if (GetTotalEffectAmount(StatusEffects.Vulnerability) > 0)
        {
            damage = Mathf.Max(0, damage + GetTotalEffectAmount(StatusEffects.Vulnerability));
        }

        // Calculate damage taken with block
        // Remove from block than remove from health
        int initialBlock = GetTotalEffectAmount(StatusEffects.Block);
        int blockedAmount = Mathf.Min(damage, initialBlock);
        int damageAfterBlock = Mathf.Max(0, damage - initialBlock);
        SubtractBlock(blockedAmount);

        initialBlock = Mathf.Max(0, initialBlock - damage);
        _currentHealth -= damageAfterBlock;

        // Update Actor healthbar
        _actor.UI.UpdateHealthUI();

        // Play taking damage animations
        FloatingTextManager.SpawnText(transform.position, damageAfterBlock.ToString(), HealthModColors.BasicDamage);

        // Resolve Riposte
        if (GetTotalEffectAmount(StatusEffects.Riposte) > 0)
        {
            source.Stats.TakeDamage(GetTotalEffectAmount(StatusEffects.Riposte), _actor);
            Console.Log($"{_actor.ActorName} ripostes {source.ActorName} for {GetTotalEffectAmount(StatusEffects.Riposte)} damage!");
        }

        // Log
        string log = $"{_actor.ActorName} takes {damage} damage: " +
            $"{blockedAmount} blocked, {damageAfterBlock} dealt. " +
            $"Block remaining: {initialBlock}, Health: {_currentHealth}";
        Console.Log(log);  

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Take burn damage at the start of the turn.
    /// Burn damage is multiplied by a fixed value definied in UConstants.BURN_MULTIPLIER.
    /// </summary>
    private void TakeBurnDamage()
    {
        int burnDamage = GetTotalEffectAmount(StatusEffects.Burn) * UConstants.BURN_MULTIPLIER;
        if (burnDamage > 0)
        {
            burnDamage = Mathf.Max(0, burnDamage);
            _currentHealth -= burnDamage;

            // Update Actor healthbar
            _actor.UI.UpdateHealthUI();

            // Play taking damage animations
            FloatingTextManager.SpawnText(transform.position, burnDamage.ToString(), HealthModColors.BurnDamage);

            // Log
            string log = $"{_actor.Data.ActorName} takes {burnDamage} burning damage: " +
              $"Health: {_currentHealth}";
            Console.Log(log);

            if (_currentHealth <= 0 )
            {
                Die();
            }
        }
    }

    /// <summary>
    /// Take poison damage at the start of the turn.
    /// Poison damage is cumulative.
    /// </summary>
    private void TakePoisonDamage()
    {
        int poisonDamage = GetTotalEffectAmount(StatusEffects.Poison);
        if (poisonDamage > 0)
        {
            poisonDamage = Mathf.Max(0, poisonDamage);
            _currentHealth -= poisonDamage;

            // Play taking damage animations
            FloatingTextManager.SpawnText(transform.position, poisonDamage.ToString(), HealthModColors.PoisonDamage);

            // Log
            string log = $"{_actor.Data.ActorName} takes {poisonDamage} poison damage: " +
              $"Health: {_currentHealth}";

            if (_currentHealth <= 0)
            {
                Die();
            }
        }
    }

    /// <summary>
    /// Takes bleed damage at the end of the turn.
    /// Bleed damage is fixed at 1 point of damage. But if the actor is moved in any way, it will take 1 * UConstants.BLEED_MULTIPLIER damage instead.
    /// </summary>
    /// <param name="MoveDamage"></param>
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
            FloatingTextManager.SpawnText(transform.position, bleedDamage.ToString(), HealthModColors.BasicDamage);

            // Log
            string log = $"{_actor.Data.ActorName} takes {bleedDamage} poison damage: " +
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
        FloatingTextManager.SpawnText(transform.position, healAmount.ToString(), HealthModColors.Heal);
        if (_currentHealth > _maxHealth)
            _currentHealth = _maxHealth;

        _ui.UpdateHealthUI();
    }

    private void RegenHealing()
    {
        int regenAmount = GetTotalEffectAmount(StatusEffects.Regen);
        if (regenAmount > 0)
        {
            regenAmount = Mathf.Max(0, regenAmount);
            _currentHealth += regenAmount;
            // Play taking damage animations
            FloatingTextManager.SpawnText(transform.position, regenAmount.ToString(), HealthModColors.Heal);
            if (_currentHealth > _maxHealth)
                _currentHealth = _maxHealth;

        }

        _ui.UpdateHealthUI();
    }
    #endregion

    // ========================================================================

    #region Relic Methods
    public void IncreaseMaxHealth(int flatAmount, bool equip)
    {
        if (equip)
        {
            // On equip we increase the max health and current health by the flat amount
            _maxHealth += flatAmount;
            _currentHealth += flatAmount;
            _ui.UpdateHealthUI();
        }
        else
        {
            // On unequip we reduce the max health and current health by the flat amount
            _maxHealth -= flatAmount;
            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
            _ui.UpdateHealthUI();
        }
    }

    public void IncreaseMaxHealth(float percentageAmount, bool equip)
    {
        if (equip)
        {
            int increaseAmount = Mathf.RoundToInt(_maxHealth * percentageAmount);
            _maxHealth += increaseAmount;
            _currentHealth += increaseAmount;
            _ui.UpdateHealthUI();
        }
        else
        {
            int decreaseAmount = Mathf.RoundToInt(_maxHealth * percentageAmount);
            _maxHealth -= decreaseAmount;
            _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
            _ui.UpdateHealthUI();
        }
    }

    public void IncreaseAction(int amount)
    {
        _currentActions += amount;
        if (_actor.IsMyTurn)
            ActorsUI.UpdateActionsInterface(_currentActions, _actionsPerTurn);
    }

    public void IncreaseCardBuy(int amount, bool equip)
    {
        if (equip)
            _cardBuy += amount;
        else
            _cardBuy = Mathf.Max(0, _cardBuy - amount);
    }
    #endregion

    // ========================================================================

    #region Buff Effects
    public void GainBlock(int blockAmount, int duration)
    {
        AddStatusEffect(StatusEffects.Block, blockAmount, duration);
    }
    private void SubtractBlock(int amount)
    {
        int remaining = amount;

        foreach (var effect in _activeEffects.Where(e => e.StatusEffect == StatusEffects.Block).ToList())
        {
            if (remaining <= 0) break;

            int subtract = Mathf.Min(effect.Amount, remaining);
            effect.Amount -= subtract;
            remaining -= subtract;

            if (effect.Amount <= 0)
                _activeEffects.Remove(effect);
        }

        _actor.UI.UpdateStatusUI(_activeEffects);
    }

    public void GainRegen(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Regen, amount, duration);
    }

    public void GainToughness(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Toughness, amount, duration);
    }

    public void GainDodge(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Dodge, amount, duration);
    }

    public void GainRiposte(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Riposte, amount, duration);
    }

    public void GainHaste(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Haste, amount, duration);
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

    public void SufferLockDebuff(int amount, int duration)
    {
        AddStatusEffect(StatusEffects.Lock, amount, duration);
    }
    #endregion


    // ========================================================================
}
