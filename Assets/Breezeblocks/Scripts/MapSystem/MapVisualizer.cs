using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using Breezeblocks.Managers;

[RequireComponent(typeof(MapGenerator))]
public class MapVisualizer : MonoBehaviour
{
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    [InfoBox("RectTransform under your Canvas that holds all node + line UI elements.", InfoMessageType.None)]
    private RectTransform _mapContainer;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    [InfoBox("Key for NodeView prefab in the pool (must contain NodeView script + an Image).", InfoMessageType.None)]
    private string _nodePrefab;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    [InfoBox("Key for Line prefab in the pool (UI Image with pivot 0.5,0.5) used to draw connections.", InfoMessageType.None)]
    private string _linePrefab;

    [FoldoutGroup("Layout", expanded: true)]
    [SerializeField]
    [InfoBox("Multiply every MapNode.Position by this before placing in the UI.", InfoMessageType.None)]
    public float positionMultiplier = 100f;

    [FoldoutGroup("Line Settings", expanded: true)]
    [SerializeField]
    [InfoBox("Thickness of each connection line, in UI units.", InfoMessageType.None)]
    public float lineThickness = 8f;

    [FoldoutGroup("Line Settings", expanded: true)]
    [SerializeField]
    [InfoBox("Duration (seconds) for line fade‐in animation.", InfoMessageType.None)]
    public float lineFadeDuration = 0.5f;

    [FoldoutGroup("Line Settings", expanded: true)]
    [SerializeField]
    [InfoBox("Default color of all lines (non‐highlighted).", InfoMessageType.None)]
    public Color defaultLineColor = Color.white;

    [FoldoutGroup("Line Settings", expanded: true)]
    [SerializeField]
    [InfoBox("Color for highlighting lines whose child node is the direct next of the last completed node.", InfoMessageType.None)]
    public Color highlightLineColor = Color.yellow;

    [FoldoutGroup("Vision", expanded: true)]
    [SerializeField]
    [InfoBox("How many floors ahead (from the completed/starting floor) are visible.", InfoMessageType.None)]
    [Range(0, 10)]
    public int vision = 2;

    private MapGenerator _mapGen;

    // Active instances so we can deactivate them
    private readonly List<GameObject> _activeNodes = new List<GameObject>();
    private readonly List<GameObject> _activeLines = new List<GameObject>();

    // We record each connection line with its parent/child nodes
    private struct ConnectionLine
    {
        public MapNode parent;
        public MapNode child;
        public GameObject lineGO;
    }
    private readonly List<ConnectionLine> _allConnectionLines = new List<ConnectionLine>();

    // Map each MapNode to its NodeView so we can toggle interactability & visibility
    private readonly Dictionary<MapNode, NodeView> _nodeViewMap = new Dictionary<MapNode, NodeView>();

    // Which nodes are currently “unlocked” (clickable).
    private readonly HashSet<MapNode> _unlockedNodes = new HashSet<MapNode>();

    // The node whose battle was most recently entered (clicked).
    private MapNode _currentNode = null;

    // The node that was last completed (or start node at initialization).
    private MapNode _lastCompletedNode = null;

    // The floor index of the last‐completed node (or 0 at start).
    private int _currentFloorIndex = 0;

    // ========================================================================

    private void Awake()
    {
        _mapGen = GetComponent<MapGenerator>();
    }

    private void Start()
    {
        _mapGen.GenerateMap();
        VisualizeMap();
    }

    /// <summary>
    /// Call this after MapGenerator.GenerateMap() has run.
    /// It will:
    ///  1. Release any previously drawn nodes & lines.
    ///  2. Instantiate and position all NodeViews & Lines (via pooler).
    ///  3. Initialize _currentFloorIndex = 0 (start floor).
    ///  4. Initially unlock only the start node (floor 0) and set it as _lastCompletedNode.
    ///  5. Apply vision, interactability, and highlighting logic.
    /// </summary>
    public void VisualizeMap()
    {
        // 1) Release all previously active nodes + lines
        foreach (var nodeGO in _activeNodes)
            nodeGO.SetActive(false);
        _activeNodes.Clear();

        foreach (var cl in _allConnectionLines)
            cl.lineGO.SetActive(false);
        _allConnectionLines.Clear();

        _nodeViewMap.Clear();
        _unlockedNodes.Clear();
        _currentNode = null;
        _lastCompletedNode = null;
        _currentFloorIndex = 0; // start at floor 0

        // 2) Fetch generated floors from MapGenerator
        List<List<MapNode>> floors = _mapGen.GetFloors();
        if (floors == null || floors.Count == 0)
        {
            Debug.LogWarning("[MapVisualizer] No floors found. Did you call GenerateMap()?");
            return;
        }

        // 3) Spawn & initialize NodeViews for every MapNode
        foreach (var floorList in floors)
        {
            foreach (var mapNode in floorList)
            {
                GameObject nodeGO = ObjectPooler.SpawnFromPool(_nodePrefab, Vector3.zero, Quaternion.identity);
                _activeNodes.Add(nodeGO);

                // Position it in UI:
                var rt = nodeGO.GetComponent<RectTransform>();
                rt.SetParent(_mapContainer, false);
                Vector2 uiPos = mapNode.Position * positionMultiplier;
                rt.anchoredPosition = uiPos;

                // Initialize NodeView:
                var nv = nodeGO.GetComponent<NodeView>();
                nv.Initialize(mapNode, OnNodeClicked);

                // Ensure this node is drawn on top of lines:
                nodeGO.transform.SetAsLastSibling();

                // Save the mapping so we can enable/disable later:
                _nodeViewMap[mapNode] = nv;
            }
        }

        // 4) Draw all connection lines BETWEEN floors:
        DrawAllConnections(floors);

        // 5) Initially, only the Start node (floor 0, index 0) is “unlocked.”
        var startNode = floors[0][0];
        _unlockedNodes.Add(startNode);
        _lastCompletedNode = startNode;

        // 6) Apply vision, interactability, and highlighting logic
        UpdateVisibilityAndInteractability();
        UpdateLineHighlighting();
    }

    /// <summary>
    /// Loops through every MapNode & its children, spawns a line UI Image
    /// between each pair, then fades it in. Also forces it behind nodes.
    /// Records each line in _allConnectionLines for later highlighting.
    /// </summary>
    private void DrawAllConnections(List<List<MapNode>> floors)
    {
        for (int f = 0; f < floors.Count - 1; f++)
        {
            foreach (var parent in floors[f])
            {
                Vector2 start = parent.Position * positionMultiplier;
                foreach (var child in parent.Connections)
                {
                    Vector2 end = child.Position * positionMultiplier;
                    GameObject lineGO = SpawnLineBetween(start, end);
                    _allConnectionLines.Add(new ConnectionLine
                    {
                        parent = parent,
                        child = child,
                        lineGO = lineGO
                    });
                }
            }
        }
    }

    /// <summary>
    /// Creates (or reuses) a UI‐Image line between two scaled UI points,
    /// stretches/rotates it, and fades it in. Also ensures it’s behind nodes.
    /// Returns the lineGO for later highlighting.
    /// </summary>
    private GameObject SpawnLineBetween(Vector2 start, Vector2 end)
    {
        GameObject lineGO = ObjectPooler.SpawnFromPool(_linePrefab, Vector3.zero, Quaternion.identity);
        _activeLines.Add(lineGO);

        var rt = lineGO.GetComponent<RectTransform>();
        rt.SetParent(_mapContainer, false);
        var img = lineGO.GetComponent<Image>();

        Vector2 delta = end - start;
        float length = delta.magnitude;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        Vector2 mid = (start + end) * 0.5f;
        rt.anchoredPosition = mid;
        rt.sizeDelta = new Vector2(length, lineThickness);
        rt.localEulerAngles = new Vector3(0f, 0f, angle);

        lineGO.transform.SetAsFirstSibling();

        if (img != null)
        {
            // Start invisible at defaultLineColor
            Color c = defaultLineColor;
            c.a = 0f;
            img.color = c;
            img
              .DOFade(defaultLineColor.a, lineFadeDuration)
              .SetEase(Ease.InOutSine)
              .SetUpdate(true);
        }

        return lineGO;
    }

    /// <summary>
    /// Called whenever a node is clicked.
    /// If it is unlocked, we mark it as the current node “in battle”
    /// (disabling all interactions). For testing, we hint pressing 'C' to complete.
    /// </summary>
    private void OnNodeClicked(MapNode node)
    {
        if (!_unlockedNodes.Contains(node))
            return;

        Debug.Log($"[MapVisualizer] Node clicked → enter battle at Floor {node.FloorIndex}, Type {node.Type}");
        Debug.Log("[MapVisualizer] (DEBUG) Press 'C' to complete this node immediately.");

        // 1) Disable all interactions
        foreach (var nv in _nodeViewMap.Values)
            nv.SetInteractable(false);

        // 2) Record this as the “current” node
        _currentNode = node;

        CombatManager.CreateCombatent(node.EnemiesData);
        GameManager.StartEvent(UEnums.MapNodeType.Combat);
    }

    /// <summary>
    /// Call this once the player has WON the battle at _currentNode_.
    /// We then:
    ///   • Update _currentFloorIndex = floorIndexOfCompletedNode  
    ///   • Set _lastCompletedNode = _currentNode  
    ///   • Clear out all old unlocked nodes  
    ///   • Unlock only the direct children of the completed node  
    ///   • Re-apply vision logic (revealing floors up to _currentFloorIndex + vision)  
    ///   • Re-highlight only lines from _lastCompletedNode to its children
    /// </summary>
    public void CompleteBattle()
    {
        if (_currentNode == null)
            return;

        // 1) Update currentFloorIndex to this node's floor
        _currentFloorIndex = _currentNode.FloorIndex;

        // 2) Mark this as last completed
        _lastCompletedNode = _currentNode;

        // 3) Clear all previously unlocked nodes
        _unlockedNodes.Clear();

        // 4) Only unlock the direct children of the completed node
        foreach (var child in _currentNode.Connections)
        {
            _unlockedNodes.Add(child);
        }

        // 5) Clear currentNode so we don’t re-complete it
        _currentNode = null;

        // 6) Update visibility, interactability, and line highlighting
        UpdateVisibilityAndInteractability();
        UpdateLineHighlighting();


        // 7) Notify GameManager that the battle is complete
        GameManager.EndEvent(UEnums.MapNodeType.Combat);
        Debug.Log("[MapVisualizer] Node completed! Vision shifted; next nodes unlocked.");
    }

    /// <summary>
    /// Updates every NodeView:
    ///   • If node.FloorIndex > (_currentFloorIndex + vision), hide it under “fog.”  
    ///   • Else (within vision): reveal it; then SetInteractable(true) if unlocked, else false.  
    /// </summary>
    private void UpdateVisibilityAndInteractability()
    {
        foreach (var kvp in _nodeViewMap)
        {
            MapNode node = kvp.Key;
            NodeView view = kvp.Value;

            int nodeFloor = node.FloorIndex;

            if (nodeFloor > _currentFloorIndex + vision)
            {
                // 1) Too far ahead—hide under “fog”
                view.SetHidden(true);
            }
            else
            {
                // 2) Within vision—unhide and set interactable if unlocked
                view.SetHidden(false);

                bool isUnlocked = _unlockedNodes.Contains(node);
                view.SetInteractable(isUnlocked);
            }
        }
    }

    /// <summary>
    /// Loops through every recorded ConnectionLine:
    ///   • If connection.parent == _lastCompletedNode → highlight that line  
    ///   • Else → revert to default color  
    /// </summary>
    private void UpdateLineHighlighting()
    {
        foreach (var cl in _allConnectionLines)
        {
            var img = cl.lineGO.GetComponent<Image>();
            if (img == null) continue;

            if (_lastCompletedNode != null && cl.parent == _lastCompletedNode)
            {
                img.color = highlightLineColor;
            }
            else
            {
                img.color = defaultLineColor;
            }
        }
    }

    /// <summary>
    /// In PlayMode, pressing 'C' will complete the currently selected node
    /// (for testing purposes), automatically unlocking its children, shifting
    /// the vision window forward, and re-highlighting the relevant lines.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && _currentNode != null)
        {
            CompleteBattle();
        }
    }
}