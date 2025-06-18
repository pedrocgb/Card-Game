using Breezeblocks.Managers;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ActorManager))]
public class ActorWorldUI : MonoBehaviour
{
    #region Variables and Properties
    private ActorManager _actor = null;

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

    [FoldoutGroup("Components/Status", expanded: true)]
    [SerializeField]
    private Transform _statusPanel = null;

    [FoldoutGroup("Components/Health", expanded: true)]
    [SerializeField]
    private Image _turnMarkerIcon = null;
    private CanvasGroup _turnMarkerCanvasGroup = null;
    private RectTransform _turnMarkerRectTrasnform = null;

    private string _statusPrefab = "Status Icon";
    private Dictionary<UEnums.StatusEffects, StatusIcon> _activeUI = new();
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        _actor = GetComponent<ActorManager>();  
        _turnMarkerCanvasGroup = _turnMarkerIcon.GetComponent<CanvasGroup>();
        _turnMarkerRectTrasnform = _turnMarkerIcon.GetComponent<RectTransform>();
    }

    private void Start()
    {
        _worldCanvas.worldCamera = Camera.main;
    }
    #endregion

    // ========================================================================

    #region UI Methods
    public void UpdateHealthUI()
    {
        _healthBar.fillAmount = _actor.Stats.HealthPercentage;
        _healthText.text = $"{_actor.Stats.CurrentHealth}/{_actor.Stats.MaxHealth}";

        if (_actor.IsMyTurn)
        {
            ActorsUI.UpdateHealthInterface(_actor.Stats.CurrentHealth, _actor.Stats.MaxHealth, _actor.Stats.HealthPercentage);
        }
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

    public void UpdateTurnMarker(bool Show)
    {
        if (Show)
        {
            _turnMarkerIcon.gameObject.SetActive(true);
            _turnMarkerRectTrasnform.localScale = Vector3.zero;
            _turnMarkerCanvasGroup.alpha = 1f;

            _turnMarkerRectTrasnform.DOScale(new Vector3(1.2f,1.2f,1f), 0.5f).SetEase(Ease.OutBack);
        }
        else
        {
            Sequence s = DOTween.Sequence();

            s.Append(_turnMarkerRectTrasnform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));
            s.Join(_turnMarkerCanvasGroup.DOFade(0f, 0.5f));
            s.OnComplete(() => _turnMarkerIcon.gameObject.SetActive(false));
        }
    }
    #endregion

    // ========================================================================
}
