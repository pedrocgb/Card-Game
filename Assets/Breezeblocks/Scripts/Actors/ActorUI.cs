using Sirenix.OdinInspector;
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

    [FoldoutGroup("Components/Health", expanded: true)]
    [SerializeField]
    private Image _healthBar = null;
    [FoldoutGroup("Components/Health", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _healthText = null;

    [FoldoutGroup("Components/Actions", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _actionsText = null;
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
    #endregion

    // ========================================================================
}
