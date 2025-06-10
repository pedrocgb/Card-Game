using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardPreview : MonoBehaviour
{
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


    public void Initialize(CardInstance Instance)
    {
        _cardNameText.text = Instance.CardName;
        _cardDescriptionText.text = Instance.CardDescription;
        _cardCostText.text = Instance.ActionCost.ToString();
        _cardImage.sprite = Instance.CardImage;
        _positionIcon.sprite = Instance.PositionIcon;
        _targetIcon.sprite = Instance.TargetIcon;
        _canvasGroup.alpha = 1f;

        transform.localScale = Vector3.one;
    }
}
