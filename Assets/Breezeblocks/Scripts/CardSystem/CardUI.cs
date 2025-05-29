using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IPooledObjects
{
    #region Variables and Properties
    // Components
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _cardNameText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Image _cardImage = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _cardDescriptionText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _cardCostText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Image _positionIcon = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Image _targetIcon = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private CanvasGroup _canvasGroup = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private CardUIAnimations _cardAnimations = null;
    public CardUIAnimations Animations => _cardAnimations;
    private Button _btn = null;

    [FoldoutGroup("Effects", expanded: true)]
    [SerializeField]
    private GameObject _cardLockEffect = null;

    // Card data
    private CardInstance _cardInstance = null;
    public CardInstance CardInstance => _cardInstance;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(OnCardClicked);
    }

    public void Initialize(CardInstance cardData)
    {
        _cardInstance = cardData;
        UpdateCardUI();
    }

    public void OnSpawn()
    {
      
    }
    #endregion

    // ========================================================================

    #region UI Management Methods
    private void UpdateCardUI()
    {
        _cardNameText.text = _cardInstance.CardName;
        _cardDescriptionText.text = _cardInstance.CardDescription;
        _cardCostText.text = _cardInstance.ActionCost.ToString();
        _cardImage.sprite = _cardInstance.CardImage;
        _positionIcon.sprite = _cardInstance.PositionIcon;
        _targetIcon.sprite = _cardInstance.TargetIcon;
        _canvasGroup.alpha = 1f;

        _cardLockEffect.SetActive(false);
    }

    public void EnableCardLockEffect(bool enable)
    {
        _cardLockEffect.SetActive(enable);
        _btn.interactable = !enable;
    }
    #endregion

    // ========================================================================

    #region Callbacks
    public void Validate(bool Playable)
    {
        if (Playable)
            _btn.interactable = true;
        else
            _btn.interactable = false;
    }

    private void OnCardClicked()
    {
        PlayerTurnManager.Instance.SelectCard(this);
    }   
    #endregion

    // ========================================================================
}
