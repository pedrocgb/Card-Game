using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(MapGenerator))]
public class MapVisualizer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("RectTransform under your Canvas that holds all node + line UI elements.")]
    public RectTransform mapContainer;

    [Tooltip("NodeView prefab (must contain NodeView script + an Image).")]
    public GameObject nodePrefab;

    [Tooltip("Line prefab (UI Image with pivot 0.5,0.5) used to draw connections.")]
    public GameObject linePrefab;

    [Header("Pooling Settings")]
    public int poolDefaultCapacity = 20;
    public int poolMaxSize = 50;

    [Header("Map Layout")]
    [Tooltip("Multiply every MapNode.Position by this before placing in the UI.")]
    public float positionMultiplier = 100f;

    [Header("Line Settings")]
    [Tooltip("Thickness of each connection line, in UI units.")]
    public float lineThickness = 8f;

    [Tooltip("Duration (seconds) for line fade‐in animation.")]
    public float lineFadeDuration = 0.5f;

    [Tooltip("Color of the connection lines.")]
    public Color lineColor = Color.white;

    [Header("Vision Settings")]
    [Tooltip("How many floors ahead (from the completed/starting floor) are visible.")]
    [Range(0, 10)]
    public int vision = 2;

    private MapGenerator _mapGen;

    // Pools for reuse
    private ObjectPool<GameObject> _nodePool;
    private ObjectPool<GameObject> _linePool;

    // Active instances so we can Release(...) them
    private readonly List<GameObject> _activeNodes = new List<GameObject>();
    private readonly List<GameObject> _activeLines = new List<GameObject>();

    // Map each MapNode to its NodeView so we can toggle interactability & visibility
    private readonly Dictionary<MapNode, NodeView> _nodeViewMap = new Dictionary<MapNode, NodeView>();

    // Which nodes are currently “unlocked” (clickable).
    private readonly HashSet<MapNode> _unlockedNodes = new HashSet<MapNode>();

    // The node whose battle was most recently entered (clicked).
    private MapNode _currentNode = null;

    // The floor index of the last‐completed node (or 0 at start).
    private int _currentFloorIndex = 0;

    private void Awake()
    {
        _mapGen = GetComponent<MapGenerator>();

        // === Node Pool Setup ===
        _nodePool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                var go = Instantiate(nodePrefab, mapContainer);
                go.SetActive(false);
                return go;
            },
            actionOnGet: (go) =>
            {
                go.SetActive(true);
                // Ensure nodes render on top of any lines
                go.transform.SetAsLastSibling();
            },
            actionOnRelease: (go) =>
            {
                var nv = go.GetComponent<NodeView>();
                if (nv != null) nv.Deactivate();
            },
            actionOnDestroy: (go) =>
            {
                Destroy(go);
            },
            collectionCheck: false,
            defaultCapacity: poolDefaultCapacity,
            maxSize: poolMaxSize
        );

        // === Line Pool Setup ===
        _linePool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                var go = Instantiate(linePrefab, mapContainer);
                go.SetActive(false);
                return go;
            },
            actionOnGet: (go) =>
            {
                go.SetActive(true);
                // Always render lines behind nodes
                go.transform.SetAsFirstSibling();
            },
            actionOnRelease: (go) =>
            {
                var img = go.GetComponent<Image>();
                if (img != null)
                {
                    Color c = img.color;
                    c.a = 0f; // reset alpha
                    img.color = c;
                }
                go.SetActive(false);
            },
            actionOnDestroy: (go) =>
            {
                Destroy(go);
            },
            collectionCheck: false,
            defaultCapacity: poolDefaultCapacity,
            maxSize: poolMaxSize
        );
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
    ///  2. Instantiate and position all NodeViews & Lines (via pooling).
    ///  3. Initialize _currentFloorIndex = 0 (start floor).
    ///  4. Initially unlock only the start node (floor 0).
    ///  5. Apply vision logic based on _currentFloorIndex.
    /// </summary>
    [ContextMenu("Visualize Map")]
    public void VisualizeMap()
    {
        // 1) Release all previously active nodes + lines
        foreach (var go in _activeNodes)
            _nodePool.Release(go);
        _activeNodes.Clear();

        foreach (var go in _activeLines)
            _linePool.Release(go);
        _activeLines.Clear();

        _nodeViewMap.Clear();
        _unlockedNodes.Clear();
        _currentNode = null;
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
                GameObject nodeGO = _nodePool.Get();
                _activeNodes.Add(nodeGO);

                // Position it in UI:
                var rt = nodeGO.GetComponent<RectTransform>();
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

        // 6) Apply vision logic based on _currentFloorIndex (which is 0)
        UpdateVisibilityAndInteractability();
    }

    /// <summary>
    /// Loops through every MapNode & its children, spawns a line UI Image
    /// between each pair, then fades it in. Also forces lines behind nodes.
    /// </summary>
    private void DrawAllConnections(List<List<MapNode>> floors)
    {
        for (int f = 0; f < floors.Count - 1; f++)
        {
            foreach (var mapNode in floors[f])
            {
                Vector2 start = mapNode.Position * positionMultiplier;
                foreach (var child in mapNode.Connections)
                {
                    Vector2 end = child.Position * positionMultiplier;
                    SpawnLineBetween(start, end);
                }
            }
        }
    }

    /// <summary>
    /// Creates (or reuses) a UI‐Image line between two scaled UI points,
    /// stretches/rotates it, and fades it in. Also ensures it’s behind nodes.
    /// </summary>
    private void SpawnLineBetween(Vector2 start, Vector2 end)
    {
        GameObject lineGO = _linePool.Get();
        _activeLines.Add(lineGO);

        var rt = lineGO.GetComponent<RectTransform>();
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
            Color c = lineColor;
            c.a = 0f; // start transparent
            img.color = c;
            img
              .DOFade(lineColor.a, lineFadeDuration)
              .SetEase(Ease.InOutSine)
              .SetUpdate(true);
        }
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
    }

    /// <summary>
    /// Call this once the player has WON the battle at _currentNode_.
    /// We then:
    ///   • Update _currentFloorIndex = floorIndexOfCompletedNode  
    ///   • Clear out all old unlocked nodes (so you can’t revisit or go sideways).  
    ///   • Unlock only the direct children of the completed node.  
    ///   • Re-apply vision logic (with the new _currentFloorIndex).
    /// </summary>
    public void CompleteBattle()
    {
        if (_currentNode == null)
            return;

        // 1) Update currentFloorIndex to this node's floor
        _currentFloorIndex = _currentNode.FloorIndex;

        // 2) Clear all previously unlocked nodes
        _unlockedNodes.Clear();

        // 3) Only unlock the direct children of the completed node
        foreach (var child in _currentNode.Connections)
        {
            _unlockedNodes.Add(child);
        }

        // 4) Clear currentNode so we don’t re-complete it
        _currentNode = null;

        // 5) Update both visibility and interactability
        UpdateVisibilityAndInteractability();

        Debug.Log("[MapVisualizer] Node completed! Vision shifted; next nodes unlocked.");
    }

    /// <summary>
    /// Updates every NodeView:
    ///   • If node.FloorIndex > (_currentFloorIndex + vision), hide it completely.  
    ///   • Else (within vision): reveal it; then call SetInteractable(true) if unlocked, or SetInteractable(false) if locked.  
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
    /// In PlayMode, pressing 'C' will complete the currently selected node
    /// (for testing purposes), automatically unlocking its children and
    /// shifting the vision window forward.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && _currentNode != null)
        {
            CompleteBattle();
        }
    }
}
