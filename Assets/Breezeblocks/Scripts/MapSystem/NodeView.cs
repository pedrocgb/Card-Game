using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using static UEnums;

/// <summary>
/// Visual representation of a single MapNode.  
/// Handles its own animations and can be toggled between:
///   • Hidden (fully inaccessible, shows hiddenSprite)  
///   • Visible but locked (dimmed, no clicks)  
///   • Visible and unlocked (full color, clickable)  
/// </summary>
public class NodeView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Node Image (assign a UI Image with a sprite)")]
    public Image nodeImage;

    [Header("Sprites for Each Node Type")]
    public Sprite spriteCombat;
    public Sprite spriteShop;
    public Sprite spriteRest;
    public Sprite spriteElite;
    public Sprite spriteTreasure;
    public Sprite spriteBoss;

    [Header("Hidden State Sprite")]
    [Tooltip("Sprite used when this node is 'out of vision' and must be hidden.")]
    public Sprite hiddenSprite;

    [Header("Hover Settings")]
    [Tooltip("Scale factor when hovered.")]
    public float hoverScale = 1.2f;
    [Tooltip("Duration (seconds) to tween in/out of hover.")]
    public float hoverDuration = 0.15f;

    private RectTransform _rectTransform;
    private Vector3 _originalScale;
    private bool _initialized = false;
    private bool _isHidden = false;

    // Colors for enabled vs locked (visible) states
    private readonly Color _enabledColor = Color.white;
    private readonly Color _lockedColor = new Color(1f, 1f, 1f, 0.5f);

    // The MapNode data this NodeView represents
    private MapNode _mapNode;

    // Callback invoked when the node is clicked
    private Action<MapNode> _onClickCallback;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _originalScale = _rectTransform.localScale;
    }

    /// <summary>
    /// Initializes this NodeView with its MapNode data and click callback,
    /// then plays a “scale‐in” animation.
    /// </summary>
    public void Initialize(MapNode mapNode, Action<MapNode> onClick)
    {
        _mapNode = mapNode;
        _onClickCallback = onClick;
        _initialized = true;
        _isHidden = false;

        // 1) Assign the correct sprite based on the node’s type:
        switch (_mapNode.Type)
        {
            case MapNodeType.Combat:
                nodeImage.sprite = spriteCombat;
                break;
            case MapNodeType.Shop:
                nodeImage.sprite = spriteShop;
                break;
            case MapNodeType.Elite:
                nodeImage.sprite = spriteElite;
                break;
            case MapNodeType.Treasure:
                nodeImage.sprite = spriteTreasure;
                break;
            case MapNodeType.Boss:
                nodeImage.sprite = spriteBoss;
                break;
            default:
                // Leave it as whatever default is on the prefab
                break;
        }

        // 2) Start fully opaque and raycast‐enabled (will be overridden by SetInteractable/SetHidden)
        nodeImage.color = _enabledColor;
        nodeImage.raycastTarget = true;

        // 3) Pop‐in animation: scale from 0→original
        _rectTransform.localScale = Vector3.zero;
        _rectTransform
            .DOScale(_originalScale, 0.5f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    /// <summary>
    /// Toggles whether this node is clickable/hoverable.  
    /// When locked, we dim the sprite and disable its raycast target.
    /// </summary>
    public void SetInteractable(bool enabled)
    {
        if (!_initialized || _isHidden) return;

        nodeImage.raycastTarget = enabled;
        nodeImage.color = enabled ? _enabledColor : _lockedColor;
    }

    /// <summary>
    /// Toggles whether this node is “out of vision” and should be hidden.  
    /// When hidden, we show hiddenSprite and disable all interactions.
    /// When revealed, we restore the correct type‐sprite but do not enable clicks
    /// until SetInteractable(true) is called.
    /// </summary>
    public void SetHidden(bool hidden)
    {
        if (!_initialized) return;

        _isHidden = hidden;

        if (hidden)
        {
            // 1) Show the ‘hidden’ placeholder sprite
            nodeImage.sprite = hiddenSprite;
            // 2) Disable all interaction
            nodeImage.raycastTarget = false;
            // 3) Ensure full alpha so the hidden sprite is fully visible
            nodeImage.color = _enabledColor;
        }
        else
        {
            // 1) Assign the correct sprite based on the node’s type:
            switch (_mapNode.Type)
            {
                case MapNodeType.Combat:
                    nodeImage.sprite = spriteCombat;
                    break;
                case MapNodeType.Shop:
                    nodeImage.sprite = spriteShop;
                    break;
                case MapNodeType.Elite:
                    nodeImage.sprite = spriteElite;
                    break;
                case MapNodeType.Treasure:
                    nodeImage.sprite = spriteTreasure;
                    break;
                case MapNodeType.Boss:
                    nodeImage.sprite = spriteBoss;
                    break;
                default:
                    // Leave it as whatever default is on the prefab
                    break;
            }

            // After revealing, “locked vs unlocked” is controlled by SetInteractable(…).
            // So we do not change raycastTarget or color here—UpdateVisibilityAndInteractability
            // in MapVisualizer will call SetInteractable(...) if this node is within vision.
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_initialized || _isHidden) return;
        _onClickCallback?.Invoke(_mapNode);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_initialized || _isHidden) return;
        // Hover tween: scale up
        _rectTransform
            .DOScale(_originalScale * hoverScale, hoverDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_initialized || _isHidden) return;
        // Hover‐out tween: scale back to normal
        _rectTransform
            .DOScale(_originalScale, hoverDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    /// <summary>
    /// Called by the pool when this node is released.  
    /// Resets state and disables the GameObject.
    /// </summary>
    public void Deactivate()
    {
        _initialized = false;
        _mapNode = null;
        _onClickCallback = null;
        _isHidden = false;
        _rectTransform.localScale = _originalScale;
        nodeImage.color = _enabledColor;
        nodeImage.raycastTarget = false;
        gameObject.SetActive(false);
    }
}
