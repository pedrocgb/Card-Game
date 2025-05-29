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

    // Card data
    private CardData _cardData = null;
    public CardData CardData => _cardData;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(OnCardClicked);
    }

    public void Initialize(CardData cardData)
    {
        _cardData = cardData;
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
        _cardNameText.text = _cardData.CardName;
        _cardDescriptionText.text = _cardData.CardDescription;
        _cardCostText.text = _cardData.ActionCost.ToString();
        _cardImage.sprite = _cardData.CardImage;
        _positionIcon.sprite = _cardData.PositionIcon;
        _targetIcon.sprite = _cardData.TargetIcon;
        _canvasGroup.alpha = 1f;
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
