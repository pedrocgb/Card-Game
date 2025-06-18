using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class CardUIAnimations : MonoBehaviour
{
    #region Variables and Properties
    // Card animation settings
    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField]
    private float _moveDuration = 0.6f;
    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField]
    private float _flipDuration = 0.3f;

    // Components
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private GameObject _frontImage;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private GameObject _backImage;
    #endregion

    // ========================================================================

    #region Animation Methods
    /// <summary>
    /// Animate the card being drawn from the deck to the hand.
    /// </summary>
    /// <param name="deckPosition"></param>
    /// <param name="handPosition"></param>
    public void PlayDrawAnimation(Vector3 deckPosition, Vector3 handPosition)
    {
        RectTransform rect = GetComponent<RectTransform>();

        // Set start position and reset rotation
        rect.anchoredPosition = deckPosition;
        rect.localEulerAngles = new Vector3(0, 0, 0);

        // Show only back at start
        _frontImage.SetActive(false);
        _backImage.SetActive(true);

        // Move and flip animation sequence
        Sequence drawSequence = DOTween.Sequence();

        // Move
        drawSequence.Append(rect.DOAnchorPos(handPosition, _moveDuration).SetEase(Ease.OutCubic));

        // Flip halfway (hide back, show front)
        drawSequence.Join(rect.DORotate(new Vector3(0, 90, 0), _flipDuration)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() =>
            {
                _frontImage.SetActive(true);
                _backImage.SetActive(false);
            }));

        // Finish flip to face
        drawSequence.Append(rect.DORotate(new Vector3(0, 0, 0), _flipDuration)
            .SetEase(Ease.InOutCubic));
    }
    #endregion

    // ========================================================================

}
