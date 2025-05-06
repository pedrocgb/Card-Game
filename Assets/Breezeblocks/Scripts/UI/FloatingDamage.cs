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

    public void UpdateText(string NewText, float Timer)
    {
        _text.text = NewText;
        _deactivateTimeStamp = Time.time + Timer;
    }

    // ========================================================================
}
