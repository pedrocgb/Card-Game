using Breezeblocks.Managers;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ActorManager))]
public class ActorUI : MonoBehaviour
{
    #region Variables and Properties
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Canvas _worldCanvas = null;
    public Canvas WorldCanvas => _worldCanvas;

    [FoldoutGroup("Components/Health", expanded: true)]
    [SerializeField]
    private Image _healthBar = null;
    [FoldoutGroup("Components/Health", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _healthText = null;

    [FoldoutGroup("Components/Actions", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _actionsText = null;

    // Status Icons
    [FoldoutGroup("Components/Status", expanded: true)]
    [SerializeField]
    private Transform _statusPanel = null;

    private string _statusPrefab = "Status Icon";
    private Dictionary<UEnums.StatusEffects, StatusIcon> _activeUI = new();
    #endregion

    // ========================================================================

    #region Initialization
    private void Start()
    {
        _worldCanvas.worldCamera = Camera.main;
    }
    #endregion

    // ========================================================================

    #region UI Methods
    public void UpdateHealthUI(float healthPercentage, int currentHealth, int maxHealth)
    {
        _healthBar.fillAmount = healthPercentage;
        _healthText.text = $"{currentHealth}/{maxHealth}";
    }

    public void UpdateActionsUI(int currentActions, int maxActions)
    {
        _actionsText.text = $"{currentActions}/{maxActions}";
    }

    public void UpdateStatusUI(List<StatusEffectInstance> ActiveEffect)
    { 
        // Group by type
        Dictionary<UEnums.StatusEffects, (int amount, int maxDuration)> totals = new();

        foreach (var effect in ActiveEffect)
        {
            if (!totals.ContainsKey(effect.StatusEffect))
                totals[effect.StatusEffect] = (0, 0);

            var current = totals[effect.StatusEffect];
            int newAmount = current.amount + effect.Amount;
            int newDuration = Mathf.Max(current.maxDuration, effect.DurationRemaining);

            totals[effect.StatusEffect] = (newAmount, newDuration);
        }

        // Update or create icons
        foreach (var pair in totals)
        {
            if (!_activeUI.ContainsKey(pair.Key))
            {
                StatusIcon icon = ObjectPooler.SpawnFromPool(_statusPrefab, _statusPanel.position, Quaternion.identity).GetComponent<StatusIcon>();
                icon.transform.SetParent(_statusPanel, false);
                icon.SetIcon(UIconsDatabase.GetIcon(pair.Key));
                _activeUI[pair.Key] = icon;
            }

            _activeUI[pair.Key].SetValues(pair.Value.amount, pair.Value.maxDuration, pair.Key);
        }

        // Remove icons for expired effects
        List<UEnums.StatusEffects> toRemove = new();
        foreach (var existing in _activeUI.Keys)
        {
            if (!totals.ContainsKey(existing))
            {
                _activeUI[existing].gameObject.SetActive(false);
                toRemove.Add(existing);
            }
        }

        foreach (var key in toRemove)
            _activeUI.Remove(key);

    }
    #endregion

    // ========================================================================
}
