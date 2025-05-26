using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;

[RequireComponent(typeof(CanvasGroup))]
public class TurnIcon : MonoBehaviour
{
    #region Variables and Properties
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] 
    private Image _portraitImage;

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private ActorManager _linkedActor = null;
    public ActorManager LinkedActor => _linkedActor;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(ActorManager actor)
    {
        _canvasGroup.alpha = 1f;
        _portraitImage.sprite = actor.Data.Portrait;
        _linkedActor = actor;
    }
    #endregion

    // ========================================================================

    #region DOTWeen animations
    public void AnimateResize(Vector2 targetSize)
    {
        _rectTransform.DOSizeDelta(targetSize, 0.25f).SetEase(Ease.OutBack);
    }

    public void Expand(Vector2 size)
    {
        _rectTransform.sizeDelta = size;
    }

    public void FadeOutAndDestroy()
    {
        _canvasGroup.DOFade(0f, 0.25f).OnComplete(() => gameObject.SetActive(false));
    }
    #endregion

    // ========================================================================
}
