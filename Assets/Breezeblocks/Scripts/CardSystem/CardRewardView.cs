using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CardRewardView : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Variables and Properties
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private GameObject _cardContent;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private GameObject _cardBack;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _cardNameText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image _cardBackBg = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image _cardImage = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _cardDescriptionText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _cardCostText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image _positionIcon = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image _targetIcon = null;

    [FoldoutGroup("Components/Materials", expanded: true)]
    [SerializeField] private Material _defaultMaterial = null;
    [FoldoutGroup("Components/Materials", expanded: true)]
    [SerializeField] private Material _selectedMaterial = null;

    // ========================================================================

    [FoldoutGroup("Animation", expanded: true)]
    [FoldoutGroup("Animation/Flip", expanded: true)]
    [SerializeField] private float _flipRotationAngle = 90f;
    [FoldoutGroup("Animation/Flip", expanded: true)]
    [SerializeField] private float _flipDuration = 0.25f;

    [FoldoutGroup("Animation/FlipScale", expanded: true)]
    [SerializeField] private float _flipScaleAmount = 1.1f;
    [FoldoutGroup("Animation/FlipScale", expanded: true)]
    [SerializeField] private float _flipScaleDuration = 0.5f;
    [FoldoutGroup("Animation/FlipScale", expanded: true)]
    [SerializeField] private int _flipScaleLoops = 2;
    [FoldoutGroup("Animation/FlipScale", expanded: true)]
    [SerializeField] private LoopType _flipScaleLoopType = LoopType.Yoyo;
    [FoldoutGroup("Animation/FlipScale", expanded: true)]
    [SerializeField] private Ease _flipScaleEase = Ease.OutBack;

    [FoldoutGroup("Animation/BackFlip", expanded: true)]
    [SerializeField] private float _backFlipDuration = 0.25f;
    [FoldoutGroup("Animation/BackFlip", expanded: true)]
    [SerializeField] private float _backFadeDuration = 0.5f;

    [FoldoutGroup("Animation/Shrink", expanded: true)]
    [SerializeField] private float _shrinkScaleAmount = 0.1f;
    [FoldoutGroup("Animation/Shrink", expanded: true)]
    [SerializeField] private float _shrinkDuration = 0.2f;
    [FoldoutGroup("Animation/Shrink", expanded: true)]
    [SerializeField] private Ease _shrinkEase = Ease.InCubic;

    [FoldoutGroup("Animation/ShrinkFade", expanded: true)]
    [SerializeField] private float _shrinkFadeDuration = 0.2f;

    [FoldoutGroup("Animation/Hover", expanded: true)]
    [SerializeField] private float _hoverScaleAmount = 1.1f;
    [FoldoutGroup("Animation/Hover", expanded: true)]
    [SerializeField] private float _hoverDuration = 0.2f;
    [FoldoutGroup("Animation/Hover", expanded: true)]
    [SerializeField] private Ease _hoverEase = Ease.OutBack;

    private CanvasGroup _canvasGroup;
    private bool _isFlipped = false;
    private CardData _data;
    private System.Action<CardRewardView> _onSelected;
    public CardData CardData => _data;
    #endregion

    // ========================================================================

    public void Initialize(CardData data, System.Action<CardRewardView> onSelected)
    {
        _data = data;
        _onSelected = onSelected;

        _cardNameText.text = data.CardName;
        _cardImage.sprite = data.CardImage;
        _cardDescriptionText.text = data.CardDescription;
        _cardCostText.text = data.ActionCost.ToString();
        _positionIcon.sprite = data.PositionIcon;
        _targetIcon.sprite = data.TargetIcon;

        // start hidden/back-side up
        transform.localScale = Vector3.one;
        _cardBackBg.material = _defaultMaterial;
        _cardContent.SetActive(false);
        _cardBack.SetActive(true);
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 1f;

        // then flip in
        FlipToFrontInstant();
    }

    // ========================================================================

    #region Pointer Methods
    public void OnPointerEnter(PointerEventData e)
    {
        if (!_isFlipped) return;
        transform.DOScale(_hoverScaleAmount, _hoverDuration)
                 .SetEase(_hoverEase);
        _cardBackBg.material = _selectedMaterial;
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (!_isFlipped) return;
        transform.DOScale(1f, _hoverDuration)
                 .SetEase(_hoverEase);
        _cardBackBg.material = _defaultMaterial;
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (!_isFlipped) return;
        _onSelected?.Invoke(this);
    }
    #endregion

    // ========================================================================

    #region Animations Methods
    /// <summary>
    /// Flip from back → front by activating content.
    /// </summary>
    public void FlipToFrontInstant()
    {
        _isFlipped = true;
        var seq = DOTween.Sequence();
        seq.Append(transform.DORotate(new Vector3(0, _flipRotationAngle, 0), _flipDuration));
        seq.AppendCallback(() =>
        {
            _cardBack.SetActive(false);
            _cardContent.SetActive(true);
        });
        seq.Append(transform.DORotate(Vector3.zero, _flipDuration));
        seq.Join(transform.DOScale(_flipScaleAmount, _flipScaleDuration)
                       .SetEase(_flipScaleEase)
                       .SetLoops(_flipScaleLoops, _flipScaleLoopType));
    }

    /// <summary>
    /// Flip back → hide by deactivating content and fading out.
    /// </summary>
    public void FlipToBackAndFade()
    {
        if (!_isFlipped) return;
        _isFlipped = false;

        var seq = DOTween.Sequence();
        seq.Append(transform.DORotate(new Vector3(0, _flipRotationAngle, 0), _backFlipDuration));
        seq.AppendCallback(() =>
        {
            _cardContent.SetActive(false);
            _cardBack.SetActive(true);
        });
        seq.Append(transform.DORotate(Vector3.zero, _backFlipDuration));
        seq.Join(_canvasGroup.DOFade(0f, _backFadeDuration));
        seq.OnComplete(() => gameObject.SetActive(false));
    }

    /// <summary>
    /// Shrinks quickly and fades out, then deactivates.
    /// </summary>
    public void ShrinkAndFade()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(_shrinkScaleAmount, _shrinkDuration)
                       .SetEase(_shrinkEase));
        seq.Join(_canvasGroup.DOFade(0f, _shrinkFadeDuration));
        seq.OnComplete(() => gameObject.SetActive(false));
    }
    #endregion

    // ========================================================================

    public CardInstance GetCardInstance()
    {
        return new CardInstance(_data);
    }

    // ========================================================================
}
