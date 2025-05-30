using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;

[RequireComponent(typeof(RectTransform))]
public class CardHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Variables and Properties
    private static CardHoverHandler _currentHovered;

    private RectTransform _rect;
    private Vector2 _originalAnchoredPos;
    private Vector3 _originalScale;
    private int _originalSiblingIndex;
    private bool _hoverEnabled = false;

    [FoldoutGroup("Animation Settings", expanded: true)]
    [SerializeField] 
    private float _hoverMoveY = 20f;
    [FoldoutGroup("Animation Settings", expanded: true)]
    [SerializeField] 
    private float _hoverScale = 1.1f;
    [FoldoutGroup("Animation Settings", expanded: true)]
    [SerializeField] 
    private float _tweenDuration = 0.2f;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _originalAnchoredPos = _rect.anchoredPosition;
        _originalScale = transform.localScale;
    }
    #endregion

    // ========================================================================

    #region Interface Methods
    public void EnableHover(bool enable)
    {
        _hoverEnabled = enable;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // If not player's turn, ignore hover events
        if (!CombatManager.Instance.IsPlayerTurn ||
            !_hoverEnabled)
            return;

        // Only allow one card hovered at a time
        if (_currentHovered != null && _currentHovered != this)
        {
            _currentHovered.Restore();
        }

        // Mark this as current
        _currentHovered = this;

        // Capture and move to front of hierarchy
        _originalSiblingIndex = _rect.GetSiblingIndex();
        _rect.SetAsLastSibling();

        // Kill any existing tweens on this transform
        _rect.DOKill();
        transform.DOKill();

        // Animate up and grow
        _rect.DOAnchorPosY(_originalAnchoredPos.y + _hoverMoveY, _tweenDuration).SetEase(Ease.OutCubic);
        transform.DOScale(_originalScale * _hoverScale, _tweenDuration).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!CombatManager.Instance.IsPlayerTurn ||
            !_hoverEnabled)
            return;

        if (_currentHovered == this)
        {
            Restore();
            _currentHovered = null;
        }
    }
    #endregion

    // ========================================================================

    public void CaptureOriginalTransform()
    {
        _originalAnchoredPos = _rect.anchoredPosition;
        _originalScale = transform.localScale;
    }

    /// <summary>
    /// Revert the card back to its original position and scale.
    /// </summary>
    public void Restore()
    {
        _rect.DOKill();
        transform.DOKill();

        _rect.DOAnchorPos(_originalAnchoredPos, _tweenDuration).SetEase(Ease.OutCubic);
        transform.DOScale(_originalScale, _tweenDuration).SetEase(Ease.OutBack);

        // Restore original hierarchy position
        _rect.SetSiblingIndex(_originalSiblingIndex);
    }

    // ========================================================================
}
