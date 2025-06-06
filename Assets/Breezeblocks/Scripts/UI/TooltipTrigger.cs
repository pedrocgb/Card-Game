using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(NodeView))]
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // If your NodeView stores its MapNode, we grab it to know the node type.
    private NodeView _nodeView;

    // Cached reference to the singleton TooltipManager in the scene:
    private Tooltip _tooltipManager;

    private void Awake()
    {
        _nodeView = GetComponent<NodeView>();
        // We assume there's exactly one TooltipManager in the Canvas.
        _tooltipManager = FindAnyObjectByType<Tooltip>();
    }

    /// <summary>
    /// Called by Unity when the mouse pointer enters this UI element's RectTransform.
    /// We fetch the MapNodeType from NodeView and tell TooltipManager to show at the mouse position.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_tooltipManager == null || _nodeView == null)
            return;

        var mapNode = _nodeView.Node; // <-- assume NodeView has a getter method for its underlying MapNode
        if (mapNode == null)
            return;

        // Get the node’s type enum value:
        UEnums.MapNodeType nodeType = mapNode.Type;

        // Use the eventData.pointerCurrentRaycast.screenPosition OR Input.mousePosition:
        Vector2 uiPos = Input.mousePosition;

        _tooltipManager.ShowTooltip(nodeType, uiPos);
    }

    /// <summary>
    /// Called by Unity when the mouse pointer exits this UI element's RectTransform.
    /// We simply hide the tooltip.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_tooltipManager == null)
            return;

        _tooltipManager.HideTooltip();
    }
}
