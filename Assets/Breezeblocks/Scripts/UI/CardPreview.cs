using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardPreview : MonoBehaviour
{
    #region Variables and Properties
    private CardInstance _cardInstance = null;
    public CardInstance CardInstance => _cardInstance;

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
    private GameObject _cardLockEffect = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private CanvasGroup _canvasGroup = null;
    public CanvasGroup CanvasGroup => _canvasGroup;

    private RectTransform _myRectTransform = null;
    public RectTransform MyRectTransform => _myRectTransform;
    #endregion

    // ========================================================================
    private void Awake()
    {
        _myRectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(CardInstance Instance)
    {
        _cardLockEffect.SetActive(false);
        _cardInstance = Instance;

        _cardNameText.text = Instance.CardName;
        _cardDescriptionText.text = Instance.CardDescription;
        _cardCostText.text = Instance.ActionCost.ToString();
        _cardImage.sprite = Instance.CardImage;
        _positionIcon.sprite = Instance.PositionIcon;
        _targetIcon.sprite = Instance.TargetIcon;
        _canvasGroup.alpha = 1f;

        transform.localScale = Vector3.one;
    }

    public void EnableCardLockEffect(bool enable)
    {
        _cardLockEffect.SetActive(enable);
    }

    // ========================================================================
}
