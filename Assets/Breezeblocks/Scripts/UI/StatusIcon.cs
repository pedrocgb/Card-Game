using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusIcon : MonoBehaviour
{
    #region Variables and Properties
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Image _iconImage;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _amountText;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _durationText;
    #endregion

    // ========================================================================

    #region Icons Methods
    public void SetIcon(Sprite NewSprite)
    {
        _iconImage.sprite = NewSprite;
    }

    public void SetValues(int Amount, int Duration, UEnums.StatusEffects Effect)
    {
        string _hex = GetColorHex(Effect);

        _amountText.text = $"<color={_hex}>{Amount}</color>";
        _durationText.text = Duration.ToString();
    }

    private string GetColorHex(UEnums.StatusEffects effect)
    {
        switch (effect)
        {
            default:
                return "white";

            case UEnums.StatusEffects.Vulnerability:
            case UEnums.StatusEffects.Weakness:
            case UEnums.StatusEffects.Slow:
            case UEnums.StatusEffects.Stun:
                return "#FF5555";

            case UEnums.StatusEffects.Block:
            case UEnums.StatusEffects.Haste:
            case UEnums.StatusEffects.Regen:
                return "#55AAFF";
        }

    }
    #endregion

    // ========================================================================
}
