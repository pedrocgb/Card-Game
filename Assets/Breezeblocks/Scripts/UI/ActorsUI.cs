using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActorsUI : MonoBehaviour
{
    #region Variables and Properties
    public static ActorsUI Instance = null;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _nameText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _raceAndSpecText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _hpText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _actionsText = null;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Image _portrait = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Image _healthBar = null;
    #endregion

    // ========================================================================

    private void Awake()
    {
        Instance = this;
    }

    // ========================================================================

    #region Static Methods
    /// <summary>
    /// Updates the user interface with the provided actor information.
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Race"></param>
    /// <param name="Specialization"></param>
    /// <param name="CurrentHealth"></param>
    /// <param name="MaxHealth"></param>
    /// <param name="CurrentActions"></param>
    /// <param name="MaxActions"></param>
    /// <param name="HealthPercentage"></param>
    public static void UpdateUserInterface(string Name, string Race, string Specialization, int CurrentHealth, int MaxHealth, int CurrentActions, int MaxActions, float HealthPercentage, Sprite Portrait)
    {
        if (Instance == null)
            return;

        Instance.updateUserInterface(Name, Race, Specialization, CurrentHealth, MaxHealth, CurrentActions, MaxActions, HealthPercentage, Portrait);
    }

    /// <summary>
    /// Updates the user interface with the provided health information.
    /// </summary>
    /// <param name="CurrentHealth"></param>
    /// <param name="MaxHealth"></param>
    /// <param name="HealthPercentage"></param>
    public static void UpdateUserInterface(int CurrentHealth, int MaxHealth, float HealthPercentage)
    {
        if (Instance == null)
            return;
        Instance.updateUserInterface(CurrentHealth, MaxHealth, HealthPercentage);
    }

    /// <summary>
    /// Updates the user interface with the provided action information.
    /// </summary>
    /// <param name="CurrentActions"></param>
    /// <param name="MaxActions"></param>
    public static void UpdateUserInterface(int CurrentActions, int MaxActions)
    {
        if (Instance == null)
            return;
        Instance.updateUserInterface(CurrentActions, MaxActions);
    }

    #endregion

    // ========================================================================

    #region Local Methods
    private void updateUserInterface(string name, string race, string specialization, int currentHealth, int maxHealth, int currentActions, int maxActions, float healthPercentage, Sprite portrait)
    {
        _nameText.text = name;
        _raceAndSpecText.text = $"{race} ({specialization})";
        _hpText.text = $"{currentHealth}/{maxHealth}";
        _actionsText.text = $"{currentActions}/{maxActions}";
        _healthBar.fillAmount = healthPercentage;
        _portrait.sprite = portrait;
    }

    private void updateUserInterface(int currentHealth, int maxHealth, float healthPercentage)
    {
        _hpText.text = $"{currentHealth}/{maxHealth}";
        _healthBar.fillAmount = healthPercentage;
    }

    private void updateUserInterface(int currentActions, int maxActions)
    {
        _actionsText.text = $"{currentActions}/{maxActions}";
    }
    #endregion

    // ========================================================================
}
