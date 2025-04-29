using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
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

    private CardData _cardData = null;

    public void Initialize(CardData cardData)
    {
        _cardData = cardData;
        UpdateCardUI();
    }

    private void UpdateCardUI()
    {
        _cardNameText.text = _cardData.CardName;
        _cardDescriptionText.text = _cardData.CardDescription;
        _cardCostText.text = _cardData.CardCost.ToString();
        _cardImage.sprite = _cardData.CardImage;
        _positionIcon.sprite = _cardData.PositionIcon;
        _targetIcon.sprite = _cardData.TargetIcon;
    }
}
