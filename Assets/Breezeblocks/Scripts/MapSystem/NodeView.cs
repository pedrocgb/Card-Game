using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using static UEnums;
using Sirenix.OdinInspector;

/// <summary>
/// Visual representation of a single MapNode.  
/// Handles its own animations and can be toggled between:
///   • Hidden (fully inaccessible, shows hiddenSprite)  
///   • Visible but locked (dimmed, no clicks)  
///   • Visible and unlocked (full color, clickable)  
/// </summary>
public class NodeView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region Variables and Properties
    private Image _nodeImage;

    [FoldoutGroup("Node Sprites", expanded: true)]
    [SerializeField]
    private Sprite _spriteCombat;
    [FoldoutGroup("Node Sprites", expanded: true)]
    [SerializeField]
    private Sprite _spriteShop;
    [FoldoutGroup("Node Sprites", expanded: true)]
    [SerializeField]
    private Sprite _spriteElite;
    [FoldoutGroup("Node Sprites", expanded: true)]
    [SerializeField]
    private Sprite _spriteTreasure;
    [FoldoutGroup("Node Sprites", expanded: true)]
    [SerializeField]
    private Sprite _spriteBoss;

    [FoldoutGroup("Node Sprites/Hidden", expanded: true)]
    [SerializeField]
    [InfoBox("Sprite used when this node is 'out of vision' and must be hidden.", InfoMessageType.None)]
    public Sprite _hiddenSprite;

    [FoldoutGroup("Hover Settings", expanded: true)]
    [SerializeField]
    public float _hoverScale = 1.2f;
    [FoldoutGroup("Hover Settings", expanded: true)]
    [SerializeField]
    [InfoBox("Duration (seconds) to tween in/out of hover.", InfoMessageType.None)]
    public float _hoverDuration = 0.15f;

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
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        _nodeImage = GetComponent<Image>();
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
                _nodeImage.sprite = _spriteCombat;
                break;
            case MapNodeType.Shop:
                _nodeImage.sprite = _spriteShop;
                break;
            case MapNodeType.Elite:
                _nodeImage.sprite = _spriteElite;
                break;
            case MapNodeType.Treasure:
                _nodeImage.sprite = _spriteTreasure;
                break;
            case MapNodeType.Boss:
                _nodeImage.sprite = _spriteBoss;
                break;
            default:
                // Leave it as whatever default is on the prefab
                break;
        }

        // 2) Start fully opaque and raycast‐enabled (will be overridden by SetInteractable/SetHidden)
        _nodeImage.color = _enabledColor;
        _nodeImage.raycastTarget = true;

        // 3) Pop‐in animation: scale from 0→original
        _rectTransform.localScale = Vector3.zero;
        _rectTransform
            .DOScale(_originalScale, 0.5f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }
    #endregion

    // ========================================================================

    #region Nodes Methods
    /// <summary>
    /// Toggles whether this node is clickable/hoverable.  
    /// When locked, we dim the sprite and disable its raycast target.
    /// </summary>
    public void SetInteractable(bool enabled)
    {
        if (!_initialized || _isHidden) return;

        _nodeImage.raycastTarget = enabled;
        _nodeImage.color = enabled ? _enabledColor : _lockedColor;
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
            _nodeImage.sprite = _hiddenSprite;
            // 2) Disable all interaction
            _nodeImage.raycastTarget = false;
            // 3) Ensure full alpha so the hidden sprite is fully visible
            _nodeImage.color = _enabledColor;
        }
        else
        {
            // 1) Assign the correct sprite based on the node’s type:
            switch (_mapNode.Type)
            {
                case MapNodeType.Combat:
                    _nodeImage.sprite = _spriteCombat;
                    break;
                case MapNodeType.Shop:
                    _nodeImage.sprite = _spriteShop;
                    break;
                case MapNodeType.Elite:
                    _nodeImage.sprite = _spriteElite;
                    break;
                case MapNodeType.Treasure:
                    _nodeImage.sprite = _spriteTreasure;
                    break;
                case MapNodeType.Boss:
                    _nodeImage.sprite = _spriteBoss;
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
        _nodeImage.color = _enabledColor;
        _nodeImage.raycastTarget = false;
        gameObject.SetActive(false);
    }
    #endregion

    // ========================================================================

    #region Hover Methods
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
            .DOScale(_originalScale * _hoverScale, _hoverDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_initialized || _isHidden) return;
        // Hover‐out tween: scale back to normal
        _rectTransform
            .DOScale(_originalScale, _hoverDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }
    #endregion

    // ========================================================================
}
