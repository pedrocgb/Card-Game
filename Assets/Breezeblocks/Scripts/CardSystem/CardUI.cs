using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IPooledObjects
{
    #region Variables and Properties
    [FoldoutGroup("Animation Settings", expanded: true)]
    [SerializeField]
    private float _selectScale = 1.2f;
    [FoldoutGroup("Animation Settings", expanded: true)]
    [SerializeField]
    private float _deselectScale = 1f;


    // Components
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _cardNameText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Image _cardBackBg = null;
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
    public CanvasGroup CanvasGroup => _canvasGroup;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private CardUIAnimations _cardAnimations = null;
    public CardUIAnimations Animations => _cardAnimations;
    private RectTransform _rectTransform = null;
    public RectTransform MyRectTransform => _rectTransform;

    [FoldoutGroup("Components/Materials", expanded: true)]
    [SerializeField]
    private Material _defaultMaterial = null;
    [FoldoutGroup("Components/Materials", expanded: true)]
    [SerializeField]
    private Material _selectedMaterial = null;

    private Button _btn = null;
    private CardHoverHandler _hover = null;
    public CardHoverHandler HoverHandler => _hover;

    [FoldoutGroup("Effects", expanded: true)]
    [SerializeField]
    private GameObject _cardLockEffect = null;

    // Card data
    private CardInstance _cardInstance = null;
    public CardInstance CardInstance => _cardInstance;
    public int SlotIndex { get; set; }
    public bool IsInteractable { get; private set; }
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        _btn = GetComponent<Button>();
        _hover = GetComponent<CardHoverHandler>();
        _rectTransform = GetComponent<RectTransform>();
        _btn.onClick.AddListener(() => PlayerTurnManager.Instance.OnCardClicked(this));
    }

    public void Initialize(CardInstance cardData)
    {
        _cardInstance = cardData;
        _cardImage.material = _defaultMaterial;
        transform.localScale = Vector3.one * _deselectScale;
        IsInteractable = true;

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

    public void SetInteractable(bool on)
    {
        IsInteractable = on;
    }

    public void OnSelection()
    {
        _cardBackBg.material = _selectedMaterial;

        _cardBackBg.transform.DOKill();
        _cardBackBg.transform.DOScale(_selectScale, 0.25f).SetEase(Ease.OutBack);
        _hover.enabled = false;
    }

    public void OnDeselection()
    {
        _cardBackBg.material = _defaultMaterial;

        _cardBackBg.transform.DOKill();
        _cardBackBg.transform.DOScale(_deselectScale, 0.25f).SetEase(Ease.OutBack);
        _hover.enabled = true;
    }
    #endregion

    // ========================================================================
}
