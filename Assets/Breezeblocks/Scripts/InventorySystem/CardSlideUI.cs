using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSlideUI : MonoBehaviour
{
    #region Variables and Properties
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _cardNameText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image _cardImage = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _cardActionCostText = null;

    private CardInstance _cardInstance;
    public CardInstance Card => _cardInstance;
    #endregion

    // ========================================================================

    public void Initialize(CardInstance card)
    {
        _cardInstance = card;
        _cardNameText.text = card.CardName;
        _cardImage.sprite = card.CardImage;
        _cardActionCostText.text = card.ActionCost.ToString();
    }

    // ========================================================================

    public void OnCardSelection()
    {
        InventoryManager.UpdateCardPreview(_cardInstance);
    }

    // ========================================================================
}
