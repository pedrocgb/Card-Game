using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using Breezeblocks.Managers;
using static UEnums;

[RequireComponent(typeof(MapGenerator))]
public class MapVisualizer : MonoBehaviour
{
    #region Variables and Properties
    public static MapVisualizer Instance = null;

    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField]
    private bool _startNodeIntiallySelected = true;

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

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    [InfoBox("UI Button which, when clicked, will start the event on the selected node.", InfoMessageType.None)]
    private Button _startButton;

    [FoldoutGroup("Layout", expanded: true)]
    [SerializeField]
    [InfoBox("Multiplier applied to raw node.Position values to compute UI coordinates.", InfoMessageType.None)]
    public Vector2 _positionMultiplier = new Vector2(100f, 100f);

    [FoldoutGroup("Vision", expanded: true)]
    [SerializeField]
    [InfoBox("How many floors ahead (from the completed/starting floor) are visible.", InfoMessageType.None)]
    [Range(0, 10)]
    public int _vision = 2;

    // The node that was last completed (or start node at initialization).
    private MapNode _lastCompletedNode = null;
    // The floor index of the last‐completed node (or 0 at start).
    private int _currentFloorIndex = 0;

    // Currently selected node for revealing/hiding its connections.
    private MapNode _selectedNode = null;
    // The node whose event is about to start.
    private MapNode _currentNode = null;

    // Tracks every active NodeView GameObject so we can deactivate them.
    private readonly List<GameObject> _activeNodes = new List<GameObject>();
    // Tracks every active Line GameObject so we can deactivate them.
    private readonly List<GameObject> _activeLines = new List<GameObject>();

    // We record each connection line with its parent/child nodes.
    private struct ConnectionLine
    {
        public MapNode parent;
        public MapNode child;
        public GameObject lineGO;
    }
    private readonly List<ConnectionLine> _allConnectionLines = new List<ConnectionLine>();

    // Map each MapNode to its NodeView so we can toggle interactability & visibility.
    private readonly Dictionary<MapNode, NodeView> _nodeViewMap = new Dictionary<MapNode, NodeView>();
    // Which nodes are currently “unlocked” (clickable).
    private readonly HashSet<MapNode> _unlockedNodes = new HashSet<MapNode>();

    // Keeps track of every connection (parent→child) the player has traversed.
    private readonly HashSet<(MapNode parent, MapNode child)> _usedConnections =
        new HashSet<(MapNode, MapNode)>();

    [FoldoutGroup("Line Settings", expanded: true)]
    [SerializeField]
    [InfoBox("Thickness of each connection line, in UI units.", InfoMessageType.None)]
    public float _lineThickness = 8f;

    [FoldoutGroup("Line Settings", expanded: true)]
    [SerializeField]
    [InfoBox("Duration (seconds) for line fade‐in animation.", InfoMessageType.None)]
    public float _lineFadeDuration = 0.5f;

    [FoldoutGroup("Line Settings", expanded: true)]
    [SerializeField]
    [InfoBox("Default color of all lines (non‐highlighted).", InfoMessageType.None)]
    public Color _defaultLineColor = Color.white;

    [FoldoutGroup("Line Settings", expanded: true)]
    [SerializeField]
    [InfoBox("Color for highlighting lines whose child node is directly next of the last completed node.", InfoMessageType.None)]
    public Color _highlightLineColor = Color.yellow;

    private MapGenerator _mapGen;
    private CombatGenerator _combatGen;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        Instance = this;

        _mapGen = GetComponent<MapGenerator>();
        _combatGen = FindAnyObjectByType<CombatGenerator>();
        // Disable the Start button initially (no node is selected).
        if (_startButton != null)
            _startButton.interactable = false;
    }

    private void Start()
    {
        _mapGen.GenerateMap();
        VisualizeMap();
    }
    #endregion

    // ========================================================================

    /// <summary>
    /// Called by MapGenerator after generating all MapNode objects.
    /// This method:
    ///   1) Deactivates any previously spawned node/line GameObjects
    ///   2) Spawns new NodeViews for every MapNode
    ///   3) Spawns a line UI Image between each parent→child pair (animated)
    ///   4) Initially hides all connections except those from the Start node
    ///   5) Unlocks only the Start node, sets it as _lastCompletedNode
    ///   6) Applies vision, interactability, and highlighting logic
    ///   7) Ensures lines are below nodes in UI order
    /// </summary>
    public void VisualizeMap()
    {
        // 1) Release all previously active nodes + lines
        foreach (var nodeGO in _activeNodes)
            nodeGO.SetActive(false);
        _activeNodes.Clear();

        foreach (var lineGO in _activeLines)
            lineGO.SetActive(false);
        _activeLines.Clear();
        _allConnectionLines.Clear();

        _nodeViewMap.Clear();
        _unlockedNodes.Clear();
        _currentNode = null;
        _lastCompletedNode = null;
        _currentFloorIndex = 0;
        // Deselect any previously selected node
        if (_selectedNode != null && _nodeViewMap.TryGetValue(_selectedNode, out var prevNV))
        {
            prevNV.HideSelection();
            _selectedNode = null;
        }
        _usedConnections.Clear();

        // Disable Start button until a node is selected
        if (_startButton != null)
            _startButton.interactable = false;

        // 2) Let MapGenerator build floors[] (List<List<MapNode>>) & return it to us:
        List<List<MapNode>> floors = _mapGen.GenerateMap();
        if (floors == null || floors.Count == 0)
            return;

        // 3) Spawn & initialize NodeViews for every MapNode
        for (int f = 0; f < floors.Count; f++)
        {
            foreach (var mapNode in floors[f])
            {
                GameObject nodeGO = ObjectPooler.SpawnFromPool(_nodePrefab, Vector3.zero, Quaternion.identity);
                _activeNodes.Add(nodeGO);

                // Position it in UI:
                var rt = nodeGO.GetComponent<RectTransform>();
                rt.SetParent(_mapContainer, false);
                Vector2 uiPos = mapNode.Position * _positionMultiplier;
                rt.anchoredPosition = uiPos;

                // Initialize NodeView (pass our click‐callback):
                var nv = nodeGO.GetComponent<NodeView>();
                nv.Initialize(mapNode, OnNodeClicked);

                // Ensure this node is drawn on top of lines:
                nodeGO.transform.SetAsLastSibling();

                // Save mapping so we can toggle later:
                _nodeViewMap[mapNode] = nv;
            }
        }

        // 4) Draw all connection lines BETWEEN floors (animated):
        DrawAllConnections(floors);

        // 5) Initially, only the Start node (floor 0, index 0) is “unlocked.”
        var startNode = floors[0][0];
        _unlockedNodes.Add(startNode);
        _lastCompletedNode = startNode;

        // 6) If start node should be selected initially:
        if (_startNodeIntiallySelected)
        {
            _selectedNode = startNode;
            if (_nodeViewMap.TryGetValue(startNode, out var startNV))
                startNV.ShowSelection();

            foreach (var cl in _allConnectionLines)
            {
                cl.lineGO.SetActive(cl.parent == startNode);
            }

            if (_startButton != null)
                _startButton.interactable = true;
        }
        else
        {
            // Hide every connection except those whose parent == startNode
            foreach (var cl in _allConnectionLines)
            {
                cl.lineGO.SetActive(cl.parent == startNode);
            }
        }

        // 7) Apply vision, interactability, and highlighting logic
        UpdateVisibilityAndInteractability();
        UpdateLineHighlighting();

        // 8) Ensure lines are drawn below nodes in the canvas
        foreach (var lineGO in _activeLines)
        {
            lineGO.transform.SetAsFirstSibling();
        }
        foreach (var nodeGO in _activeNodes)
        {
            nodeGO.transform.SetAsLastSibling();
        }
    }

    // ========================================================================


    #region Linesa and Connections Methods
    /// <summary>
    /// Loops through every MapNode & its children, spawns a line UI Image
    /// between each pair (animated via DOTween), then forces it behind nodes.
    /// Records each line in _allConnectionLines for later toggling.
    /// </summary>
    private void DrawAllConnections(List<List<MapNode>> floors)
    {
        for (int f = 0; f < floors.Count - 1; f++)
        {
            foreach (var parent in floors[f])
            {
                Vector2 start = parent.Position * _positionMultiplier;
                if (parent.Connections == null) continue;

                foreach (var child in parent.Connections)
                {
                    Vector2 end = child.Position * _positionMultiplier;
                    GameObject lineGO = SpawnAnimatedLine(start, end);
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
    /// Spawns a single line UI Image (via pool) between two UI‐space points.
    /// Animates its scale and fade using DOTween for a smooth reveal.
    /// Returns the new GameObject so that calling code can track it.
    /// </summary>
    private GameObject SpawnAnimatedLine(Vector2 start, Vector2 end)
    {
        GameObject lineGO = ObjectPooler.SpawnFromPool(_linePrefab, Vector3.zero, Quaternion.identity);
        _activeLines.Add(lineGO);
        lineGO.SetActive(true);

        var rt = lineGO.GetComponent<RectTransform>();
        rt.SetParent(_mapContainer, false);

        // Compute rotation & length
        float length = Vector2.Distance(start, end);
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

        // Place at midpoint, rotate, size
        rt.anchoredPosition = (start + end) / 2f;
        rt.localRotation = Quaternion.Euler(0f, 0f, angle);
        rt.sizeDelta = new Vector2(length, _lineThickness);

        // Make sure it's behind everything else
        lineGO.transform.SetAsFirstSibling();

        // Set its final alpha immediately
        var img = lineGO.GetComponent<Image>();
        if (img != null)
        {
            Color c = _defaultLineColor;
            img.color = c; // no fade, full alpha
        }

        // No tween here. Just return the fully‐formed line.
        return lineGO;
    }


    /// <summary>
    /// Updates every NodeView:
    ///   • If node.FloorIndex > (_currentFloorIndex + _vision), hide under “fog”
    ///   • Else (within vision): reveal; then SetInteractable(true) if unlocked, else false
    /// </summary>
    private void UpdateVisibilityAndInteractability()
    {
        foreach (var kvp in _nodeViewMap)
        {
            MapNode node = kvp.Key;
            NodeView view = kvp.Value;

            int nodeFloor = node.FloorIndex;

            if (nodeFloor > _currentFloorIndex + _vision)
            {
                // Too far ahead — hide under “fog”
                view.SetHidden(true);
            }
            else
            {
                // Within vision — unhide and set interactable if unlocked
                view.SetHidden(false);
                bool isUnlocked = _unlockedNodes.Contains(node);
                view.SetInteractable(isUnlocked);
            }
        }
    }

    /// <summary>
    /// Recolors & reveals connection lines based on:
    ///   • Any previously used (parent→child) pair → stay black & visible forever
    ///   • Outgoing from the current _lastCompletedNode → highlight & ensure visible
    ///   • All others → default color (visibility is managed elsewhere)
    /// </summary>
    private void UpdateLineHighlighting()
    {
        foreach (var cl in _allConnectionLines)
        {
            var img = cl.lineGO.GetComponent<Image>();
            if (img == null) continue;

            // 1) If this connection was traversed in the past, it stays black & visible forever:
            if (_usedConnections.Contains((cl.parent, cl.child)))
            {
                img.color = Color.black;
                cl.lineGO.SetActive(true);
            }
            // 2) Else if it's an outgoing from the most recently completed node, highlight it:
            else if (_lastCompletedNode != null && cl.parent == _lastCompletedNode)
            {
                img.color = _highlightLineColor;
                cl.lineGO.SetActive(true);
            }
            // 3) Otherwise, revert color to default (but do NOT force-hide here)
            else
            {
                img.color = _defaultLineColor;
                // Visibility left as-is (Reveal/Hide methods control it)
            }
        }
    }

    /// <summary>
    /// Reveals connection lines from the given node.
    /// </summary>
    private void RevealConnections(MapNode node)
    {
        foreach (var cl in _allConnectionLines)
        {
            if (cl.parent == node)
            {
                // Animate this line from invisible→visible:
                var lineGO = cl.lineGO;
                var img = lineGO.GetComponent<Image>();
                // Start from zero scale & zero alpha:
                lineGO.transform.localScale = new Vector3(0f, 1f, 1f);
                if (img != null)
                {
                    Color c = img.color;
                    c.a = 0f;
                    img.color = c;
                }

                lineGO.SetActive(true);

                // Tween scale X from 0→1, alpha from 0→default:
                if (img != null)
                {
                    float targetAlpha = _defaultLineColor.a;
                    Sequence seq = DOTween.Sequence();
                    seq.Append(lineGO.transform.DOScaleX(1f, _lineFadeDuration).SetEase(Ease.OutQuad))
                       .Join(img.DOFade(targetAlpha, _lineFadeDuration).SetEase(Ease.InOutSine))
                       .SetUpdate(true);
                }
                else
                {
                    lineGO.transform.DOScaleX(1f, _lineFadeDuration).SetEase(Ease.OutQuad).SetUpdate(true);
                }
            }
        }
    }

    /// <summary>
    /// Hides connection lines from the given node,
    /// except if they are marked as “used” (i.e. part of the path taken).
    /// </summary>
    private void HideConnections(MapNode node)
    {
        foreach (var cl in _allConnectionLines)
        {
            if (cl.parent == node)
            {
                // If this connection has already been traversed in the past, keep it visible:
                if (_usedConnections.Contains((cl.parent, cl.child)))
                    continue;

                cl.lineGO.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Spawns a single line UI Image (via pool) between two UI‐space points.
    /// Pivot is set to (0,0.5) so scaling from X=0 expands from the parent end.
    /// Returns the new GameObject so calling code can track it.
    /// </summary>
    private GameObject SpawnLineAtParent(Vector2 start, Vector2 end)
    {
        GameObject lineGO = ObjectPooler.SpawnFromPool(_linePrefab, Vector3.zero, Quaternion.identity);
        _activeLines.Add(lineGO);

        var rt = lineGO.GetComponent<RectTransform>();
        rt.SetParent(_mapContainer, false);

        // Pivot at the parent end
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = start;

        float length = Vector2.Distance(start, end);
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0f, 0f, angle);
        rt.sizeDelta = new Vector2(length, _lineThickness);
        lineGO.transform.localScale = Vector3.one;

        var img = lineGO.GetComponent<Image>();
        if (img != null)
        {
            Color c = _defaultLineColor;
            c.a = 0f;
            img.color = c;
        }

        lineGO.SetActive(false);
        return lineGO;
    }
    #endregion

    // ========================================================================

    #region Events
    /// <summary>
    /// Called whenever a node is clicked.
    /// Toggles selection and reveals/hides its own connections.
    /// Also enables/disables the _startButton accordingly.
    /// </summary>
    private void OnNodeClicked(MapNode node)
    {
        if (!_unlockedNodes.Contains(node))
            return;

        // If clicking the already-selected node, deselect and hide its connections:
        if (_selectedNode == node)
        {
            // Hide radial selection
            if (_nodeViewMap.TryGetValue(node, out var nvToHide))
                nvToHide.HideSelection();

            HideConnections(node);
            _selectedNode = null;
        }
        else
        {
            // Deselect previous if any:
            if (_selectedNode != null)
            {
                HideConnections(_selectedNode);
                if (_nodeViewMap.TryGetValue(_selectedNode, out var nvPrev))
                    nvPrev.HideSelection();
            }

            // Select new node: show radial selection and reveal its connections:
            _selectedNode = node;
            if (_nodeViewMap.TryGetValue(node, out var nvToShow))
                nvToShow.ShowSelection();

            RevealConnections(node);
        }

        // Enable or disable Start Button based on whether we have a selected node:
        if (_startButton != null)
            _startButton.interactable = (_selectedNode != null);
    }

    /// <summary>
    /// Called when the Start-Event button is clicked, or when Enter is pressed.
    /// Records the chosen path (parent→child), blacks out that line permanently,
    /// hides all sibling lines from the parent, then starts the event.
    /// </summary>
    public void StartEventOnSelected()
    {
        if (_selectedNode == null || _lastCompletedNode == null)
            return;

        // Hide radial selection on the selected node:
        if (_nodeViewMap.TryGetValue(_selectedNode, out var nv))
            nv.HideSelection();

        // 1) Record the chosen connection: (parent = lastCompletedNode → child = selectedNode)
        var chosenPair = (_lastCompletedNode, _selectedNode);
        _usedConnections.Add(chosenPair);

        // 2) Immediately turn that chosen line black & ensure it's visible:
        foreach (var cl in _allConnectionLines)
        {
            if (cl.parent == chosenPair.Item1)
            {
                if (cl.child == chosenPair.Item2)
                {
                    var img = cl.lineGO.GetComponent<Image>();
                    if (img != null)
                        img.color = Color.black;
                    cl.lineGO.SetActive(true);
                }
                else
                {
                    // Hide all other outgoing lines from the parent (siblings):
                    cl.lineGO.SetActive(false);
                }
            }
        }

        // 3) Now set this node as current, so completion logic can use it:
        _currentNode = _selectedNode;

        // 4) Reset selection & disable the Start button:
        _selectedNode = null;
        if (_startButton != null)
            _startButton.interactable = false;

        // 5) Begin the event using GameManager (same as original behavior):
        CombatManager.CreateCombatent(_currentNode.EnemiesData);
        Debug.Log(nv.Node.Type);
        GameManager.StartEvent(nv.Node.Type);
    }

    /// <summary>
    /// Call this once the player has completed any EVENT
    /// We then:
    ///   • Update _currentFloorIndex = floor index of completed node
    ///   • Set _lastCompletedNode = _currentNode
    ///   • Clear out all old unlocked nodes
    ///   • Unlock only the direct children of the completed node
    ///   • Re-apply vision logic (revealing floors up to _currentFloorIndex + _vision)
    ///   • Re-highlight lines based on all used connections + new lastCompletedNode
    ///   • Notify GameManager that battle ended
    /// </summary>
    public void CompleteEvent()
    {
        if (_currentNode == null)
            return;

        // Hide any lingering selection radial
        if (_nodeViewMap.TryGetValue(_currentNode, out var nvCur))
            nvCur.HideSelection();

        // 1) Update floor index and lastCompletedNode:
        int completedFloor = _currentNode.FloorIndex;
        _currentFloorIndex = completedFloor;
        _lastCompletedNode = _currentNode;

        // 2) Clear out old unlocked nodes:
        _unlockedNodes.Clear();

        // 3) Unlock only direct children of the completed node:
        if (_currentNode.Connections != null)
        {
            foreach (var child in _currentNode.Connections)
                _unlockedNodes.Add(child);
        }

        // 4) Clear _currentNode so we don’t re-complete it:
        _currentNode = null;

        // 5) Update visibility, interactability:
        UpdateVisibilityAndInteractability();

        // 6) Re-highlight lines: black for every usedConnection; highlight only outgoing from new lastCompletedNode
        UpdateLineHighlighting();

        // 7) Notify GameManager that the battle is complete:
        GameManager.EndEvent(nvCur.Node.Type);
    }
    #endregion

    // ========================================================================

    /// <summary>
    /// In PlayMode, pressing 'C' will complete the currently selected node
    /// (for testing purposes), automatically unlocking its children, shifting
    /// the vision window forward, and re-highlighting the relevant lines.
    ///
    /// Pressing 'Return' (Enter) will do the same as clicking the Start button.
    /// </summary>
    private void Update()
    {
        // 'C' for completing the battle at _currentNode_:
        if (Input.GetKeyDown(KeyCode.F2) && _currentNode != null)
        {
            CompleteEvent();
        }

        // 'Enter' to start the selected node's event:
        if (Input.GetKeyDown(KeyCode.Return) && _selectedNode != null)
        {
            StartEventOnSelected();
        }
    }

    // ========================================================================

}
