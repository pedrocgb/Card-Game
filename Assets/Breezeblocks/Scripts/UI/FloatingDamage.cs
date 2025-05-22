using TMPro;
using UnityEngine;

public class FloatingDamage : MonoBehaviour
{
    private TextMeshProUGUI _text = null;
    private float _deactivateTimeStamp = 0f;

    // ========================================================================

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    // ========================================================================

    private void Update()
    {
        if (_deactivateTimeStamp <= Time.time)
            gameObject.SetActive(false);
    }

    // ========================================================================

    public void UpdateText(string NewText, float Timer, UEnums.HealthModColors DamageMod)
    {
        switch (DamageMod)
        {
            default:
            case UEnums.HealthModColors.BasicDamage:
                _text.color = UConstants.BASIC_DAMAGE_COLOR;
                break;
            case UEnums.HealthModColors.BurnDamage:
                _text.color = UConstants.BURN_DAMAGE_COLOR;
                break;
            case UEnums.HealthModColors.PoisonDamage:
                _text.color = UConstants.POISON_DAMAGE_COLOR;
                break;
            case UEnums.HealthModColors.Heal:
                _text.color = UConstants.HEAL_COLOR;
                break;
        }

        _text.text = NewText;
        _deactivateTimeStamp = Time.time + Timer;
    }

    // ========================================================================
}
