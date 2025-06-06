using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class Tooltip : MonoBehaviour
{
    [System.Serializable]
    private struct NodeTypeInfo
    {
        public UEnums.MapNodeType nodeType;   // the enum type of the MapNode
        [TextArea] public string description;  // the text you want to show in the tooltip
    }

    [Header("References")]
    [SerializeField]
    [Tooltip("The panel (with an Image component) that will serve as the tooltip background")]
    private RectTransform _tooltipPanel;

    [SerializeField]
    [Tooltip("The UI.Text component inside the tooltip panel where the description is shown")]
    private TextMeshProUGUI _tooltipText;

    [Header("Typing Animation Settings")]
    [SerializeField]
    [Tooltip("Time (in seconds) per character when typing")]
    private float _typingSpeed = 0.05f;

    [Header("Node-Type → Description Mapping")]
    [SerializeField]
    [Tooltip("List of (MapNodeType → description) pairs.  Fill each entry in the Inspector.")]
    private NodeTypeInfo[] _nodeTypeInfos;

    [Header("Follow & Sizing Settings")]
    [SerializeField]
    [Tooltip("How far, in pixels, the panel should be offset from the mouse position")]
    private Vector2 _offset = new Vector2(10f, -10f);

    [SerializeField]
    [Tooltip("Padding (in pixels) around the text inside the background panel")]
    private Vector2 _padding = new Vector2(8f, 8f);

    // Runtime dictionary for fast lookup:
    private Dictionary<UEnums.MapNodeType, string> _descriptionByType;

    // Tween that’s currently animating the text.  We keep a reference so we can Kill() if needed.
    private Tween _typingTween;

    private void Awake()
    {
        // Build dictionary from the serialized array:
        _descriptionByType = new Dictionary<UEnums.MapNodeType, string>();
        foreach (var pair in _nodeTypeInfos)
        {
            if (!_descriptionByType.ContainsKey(pair.nodeType))
                _descriptionByType.Add(pair.nodeType, pair.description);
        }

        // Initially, tooltip should be hidden:
        if (_tooltipPanel != null)
            _tooltipPanel.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        // While the tooltip is visible, keep it at (mousePosition + offset)
        if (_tooltipPanel != null && _tooltipPanel.gameObject.activeSelf)
        {
            _tooltipPanel.position = (Vector2)Input.mousePosition + _offset;
        }
    }

    /// <summary>
    /// Call this to show a tooltip for the given nodeType at screenPosition.
    /// It types out the text (character by character) using DOTween.
    /// </summary>
    public void ShowTooltip(UEnums.MapNodeType nodeType, Vector2 screenPosition)
    {
        if (_tooltipPanel == null || _tooltipText == null)
            return;

        // 1) Look up the full text for this nodeType:
        if (!_descriptionByType.TryGetValue(nodeType, out string fullText))
            fullText = "";

        // 2) Temporarily set the full text to measure its final size
        _tooltipText.text = fullText;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipText.rectTransform);

        // 3) Read preferred dimensions
        float textW = _tooltipText.preferredWidth;
        float textH = _tooltipText.preferredHeight;

        // 4) Resize the Text rect and background panel immediately to the final size
        _tooltipText.rectTransform.sizeDelta = new Vector2(textW, textH);
        _tooltipPanel.sizeDelta = new Vector2(textW + _padding.x * 2f, textH + _padding.y * 2f);

        // 5) Now clear the text so we can type it out
        _tooltipText.text = "";

        // 6) Enable & position the panel (mouse + offset)
        _tooltipPanel.gameObject.SetActive(true);
        _tooltipPanel.position = screenPosition + _offset;

        // 7) Kill any prior typing tween
        _typingTween?.Kill();
        _typingTween = null;

        // 8) Animate “typing” from 0→fullText.Length, but background stays fixed
        int totalChars = fullText.Length;
        int current = 0;
        float duration = totalChars * _typingSpeed;

        _typingTween = DOTween.To(
            () => current,
            x =>
            {
                current = x;
                _tooltipText.text = fullText.Substring(0, Mathf.Clamp(current, 0, totalChars));
            },
            totalChars,
            duration
        )
        .SetEase(Ease.Linear)
        .SetUpdate(true);
    }


    public void HideTooltip()
    {
        if (_tooltipPanel == null)
            return;

        _tooltipPanel.gameObject.SetActive(false);
        _typingTween?.Kill();
        _typingTween = null;
    }

    /// <summary>
    /// Adjusts the panel’s size to fit the current text plus padding.
    /// </summary>
    private void ResizeBackground()
    {
        // Force layout rebuild so preferredWidth/preferredHeight are up‐to‐date
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipText.rectTransform);

        // Measure the text’s preferred dimensions
        float textW = _tooltipText.preferredWidth;
        float textH = _tooltipText.preferredHeight;

        // Resize the Text’s RectTransform (so the background can match)
        _tooltipText.rectTransform.sizeDelta = new Vector2(textW, textH);

        // Now set the panel’s size to text size + padding on each side
        _tooltipPanel.sizeDelta = new Vector2(textW + _padding.x * 2f, textH + _padding.y * 2f);
    }
}
