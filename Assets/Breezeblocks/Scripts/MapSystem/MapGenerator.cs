using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static UEnums;

/// <summary>
/// Generates a Slay-the-Spire-style branching map of MapNode objects,
/// with optional forced node counts/types per floor and repeatable seed support.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    #region Variables and Properties

    [FoldoutGroup("Seed Settings", expanded: true)]
    [SerializeField]
    [InfoBox("If true, uses the provided Seed to initialize the RNG. Otherwise, a random seed is generated and logged.", InfoMessageType.None)]
    private bool _useCustomSeed = false;

    [FoldoutGroup("Seed Settings", expanded: true)]
    [SerializeField]
    [InfoBox("Integer seed for repeatable map generation. Only used if Use Custom Seed is checked.", InfoMessageType.None)]
    private int _seed = 0;

    [FoldoutGroup("Map Dimensions", expanded: true)]
    [SerializeField]
    [InfoBox("Total number of floors (rows) including start (floor 0) and boss (floorCount - 1).", InfoMessageType.None)]
    private int _totalFloors = 7;

    [FoldoutGroup("Map Dimensions", expanded: true)]
    [SerializeField]
    [InfoBox("Minimum number of nodes per floor (excluding start and boss floors).", InfoMessageType.None)]
    public int _minNodesPerFloor = 1;

    [FoldoutGroup("Map Dimensions", expanded: true)]
    [SerializeField]
    [InfoBox("Maximum number of nodes per floor (excluding start and boss floors).", InfoMessageType.None)]
    public int _maxNodesPerFloor = 4;

    [FoldoutGroup("Layout", expanded: true)]
    [SerializeField]
    [InfoBox("Horizontal spacing between nodes in the same floor (units).", InfoMessageType.None)]
    public float _horizontalSpacing = 3f;

    [FoldoutGroup("Layout", expanded: true)]
    [SerializeField]
    [InfoBox("Vertical spacing between floors (units).", InfoMessageType.None)]
    public float _verticalSpacing = 2.5f;

    [FoldoutGroup("Node Weights", expanded: true)]
    [SerializeField]
    [InfoBox("Weight for creating a Combat node.", InfoMessageType.None)]
    public float weightCombat = 1f;

    [FoldoutGroup("Node Weights", expanded: true)]
    [SerializeField]
    [InfoBox("Weight for creating a Shop node.", InfoMessageType.None)]
    public float weightShop = 0.1f;

    [FoldoutGroup("Node Weights", expanded: true)]
    [SerializeField]
    [InfoBox("Weight for creating an Elite node.", InfoMessageType.None)]
    public float weightElite = 0.1f;

    [FoldoutGroup("Node Weights", expanded: true)]
    [SerializeField]
    [InfoBox("Weight for creating a Treasure/Event node.", InfoMessageType.None)]
    public float weightTreasure = 0.3f;

    [FoldoutGroup("Node Weights", expanded: true)]
    [SerializeField]
    [InfoBox("Weight for creating a Corruption node.", InfoMessageType.None)]
    public float weightCorruption = 0.1f;

    [FoldoutGroup("Node Weights", expanded: true)]
    [SerializeField]
    [InfoBox("Weight for forcing Boss on final floor (should be highest or 1.0).", InfoMessageType.None)]
    public float weightBoss = 1f;

    [FoldoutGroup("Per-Floor Type Limits", expanded: true)]
    [SerializeField]
    [InfoBox("Maximum number of Treasure nodes allowed per floor.", InfoMessageType.None)]
    public int _maxTreasurePerFloor = 1;

    [FoldoutGroup("Per-Floor Type Limits", expanded: true)]
    [SerializeField]
    [InfoBox("Maximum number of Shop nodes allowed per floor.", InfoMessageType.None)]
    public int _maxShopPerFloor = 1;

    [FoldoutGroup("Per-Floor Type Limits", expanded: true)]
    [SerializeField]
    [InfoBox("Maximum number of Elite nodes allowed per floor.", InfoMessageType.None)]
    public int _maxElitePerFloor = 1;

    [FoldoutGroup("Per-Floor Type Limits", expanded: true)]
    [SerializeField]
    [InfoBox("Maximum number of Corruption nodes allowed per floor.", InfoMessageType.None)]
    public int _maxCorruptionPerFloor = 1;

    [FoldoutGroup("Forced Requirements", expanded: true)]
    [SerializeField]
    [InfoBox("Specify floor-specific forced counts of given node types.\n" +
             "When a floor has any ForcedRequirement entries, that floor will contain EXACTLY those forced nodes,\n" +
             "ignoring random counts, weights, and per-floor caps.", InfoMessageType.None)]
    private List<ForcedRequirement> _forcedRequirements = new List<ForcedRequirement>();

    [System.Serializable]
    public class ForcedRequirement
    {
        [Tooltip("Floor index on which to force nodes.")]
        public int floorIndex;

        [Tooltip("Type of node to force on that floor.")]
        public MapNodeType nodeType;

        [Tooltip("Number of that node type to force.")]
        public int count;
    }

    // The entire map stored floor by floor
    private List<List<MapNode>> _floors;

    private CombatGenerator _combatGenerator;
    #endregion

    // ========================================================================

    private void Awake()
    {
        _combatGenerator = FindAnyObjectByType<CombatGenerator>();
    }

    // ========================================================================

    #region Map Generation Methods

    /// <summary>
    /// Call this to rebuild the map at runtime (e.g. on Start or via inspector button).
    /// </summary>
    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        // Initialize RNG based on seed settings
        if (_useCustomSeed)
        {
            Random.InitState(_seed);
            Debug.Log($"[MapGenerator] Using custom seed: {_seed}");
        }
        else
        {
            _seed = Random.Range(int.MinValue, int.MaxValue);
            Random.InitState(_seed);
            Debug.Log($"[MapGenerator] Generated random seed: {_seed}");
        }

        // 1) Initialize data structures.
        _floors = new List<List<MapNode>>();

        // 2) Floor 0: always a single START node at (0,0).
        var floor0 = new List<MapNode>();
        var startNode = new MapNode(
            floorIndex: 0,
            position: new Vector2(0, 0),
            type: MapNodeType.Combat // Treat as start/floor-0 node
        );
        floor0.Add(startNode);
        _floors.Add(floor0);

        // 3) Build floors 1..(_totalFloors-2) with either forced requirements or randomized nodes.
        for (int f = 1; f < _totalFloors - 1; f++)
        {
            // Gather forced entries for this floor
            var floorForces = _forcedRequirements.FindAll(r => r.floorIndex == f);
            if (floorForces.Count > 0)
            {
                // If any forced requirement exists, override random generation.
                // Sum total forced count
                int totalForced = 0;
                foreach (var req in floorForces)
                {
                    totalForced += req.count;
                }
                // Ensure at least 1 node if someone forced 0 for all types
                totalForced = Mathf.Max(totalForced, 1);

                var thisFloor = new List<MapNode>();
                // Create exactly totalForced nodes, distributing types as specified
                foreach (var req in floorForces)
                {
                    int placeCount = Mathf.Min(req.count, totalForced - thisFloor.Count);
                    for (int i = 0; i < placeCount; i++)
                    {
                        var node = new MapNode(
                            floorIndex: f,
                            position: Vector2.zero, // assigned later
                            type: req.nodeType
                        );
                        thisFloor.Add(node);
                    }
                }
                // If sum of req.count is less than totalForced due to rounding, fill rest as Combat
                while (thisFloor.Count < totalForced)
                {
                    var node = new MapNode(
                        floorIndex: f,
                        position: Vector2.zero,
                        type: MapNodeType.Combat
                    );
                    thisFloor.Add(node);
                }

                _floors.Add(thisFloor);
                continue;
            }

            // No forced requirement on this floor; generate normally
            int countThisFloor = Random.Range(_minNodesPerFloor, _maxNodesPerFloor + 1);
            var thisFloorNormal = new List<MapNode>();

            // Track per-floor counts for limiting
            int treasureCount = 0;
            int shopCount = 0;
            int eliteCount = 0;
            int corruptionCount = 0;

            for (int i = 0; i < countThisFloor; i++)
            {
                MapNodeType chosenType = PickNodeTypeWithLimits(
                    treasureCount, shopCount, eliteCount, corruptionCount);

                switch (chosenType)
                {
                    case MapNodeType.Treasure: treasureCount++; break;
                    case MapNodeType.Shop: shopCount++; break;
                    case MapNodeType.Elite: eliteCount++; break;
                    case MapNodeType.Corruption: corruptionCount++; break;
                }

                var node = new MapNode(
                    floorIndex: f,
                    position: Vector2.zero,
                    type: chosenType
                );
                thisFloorNormal.Add(node);
            }

            _floors.Add(thisFloorNormal);
        }

        // 4) Final floor: exactly one BOSS node.
        var lastFloor = new List<MapNode>();
        var bossNode = new MapNode(
            floorIndex: _totalFloors - 1,
            position: Vector2.zero,
            type: MapNodeType.Boss
        );
        lastFloor.Add(bossNode);
        _floors.Add(lastFloor);

        // 5) Assign positions for every node now that we know counts per floor.
        AssignNodePositions();

        // 6) Connect nodes between floors (0→1, 1→2, …).
        ConnectFloors();

        // 7) Generate combat events
        _combatGenerator.GenerateCombats();
    }

    /// <summary>
    /// Chooses a NodeType at random based on weights and per-floor limits.
    /// </summary>
    private MapNodeType PickNodeTypeWithLimits(
        int treasureCount,
        int shopCount,
        int eliteCount,
        int corruptionCount)
    {
        var types = new List<MapNodeType>();
        var weights = new List<float>();

        // Combat always allowed
        types.Add(MapNodeType.Combat);
        weights.Add(weightCombat);

        // Shop if below limit
        if (shopCount < _maxShopPerFloor)
        {
            types.Add(MapNodeType.Shop);
            weights.Add(weightShop);
        }

        // Elite if below limit
        if (eliteCount < _maxElitePerFloor)
        {
            types.Add(MapNodeType.Elite);
            weights.Add(weightElite);
        }

        // Treasure if below limit
        if (treasureCount < _maxTreasurePerFloor)
        {
            types.Add(MapNodeType.Treasure);
            weights.Add(weightTreasure);
        }

        // Corruption if below limit
        if (corruptionCount < _maxCorruptionPerFloor)
        {
            types.Add(MapNodeType.Corruption);
            weights.Add(weightCorruption);
        }

        float total = 0f;
        foreach (var w in weights) total += w;

        float r = Random.Range(0f, total);
        float cumulative = 0f;
        for (int i = 0; i < types.Count; i++)
        {
            cumulative += weights[i];
            if (r <= cumulative)
                return types[i];
        }

        return types[0];
    }

    /// <summary>
    /// Computes and assigns a 2D position for each node, stacking floors vertically.
    /// Floors are spaced by _verticalSpacing. Within each floor, nodes are horizontally centered
    /// and spaced by _horizontalSpacing.
    /// </summary>
    private void AssignNodePositions()
    {
        for (int f = 0; f < _floors.Count; f++)
        {
            var thisFloor = _floors[f];
            int count = thisFloor.Count;
            float totalWidth = (count - 1) * _horizontalSpacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                float x = startX + i * _horizontalSpacing;
                float y = -f * _verticalSpacing;
                thisFloor[i].Position = new Vector2(x, y);
            }
        }
    }

    /// <summary>
    /// Connects each node on floor f to at least one node on floor f+1.
    /// Ensures connectivity and adds extra random edges for branching.
    /// </summary>
    private void ConnectFloors()
    {
        for (int f = 0; f < _floors.Count - 1; f++)
        {
            var currentFloor = _floors[f];
            var nextFloor = _floors[f + 1];

            // 1) Ensure each currentFloor node connects to at least one nextFloor node
            var nextUnconnected = new HashSet<MapNode>(nextFloor);
            var shuffledNext = new List<MapNode>(nextFloor);
            for (int i = 0; i < shuffledNext.Count; i++)
            {
                int j = Random.Range(i, shuffledNext.Count);
                var temp = shuffledNext[i];
                shuffledNext[i] = shuffledNext[j];
                shuffledNext[j] = temp;
            }

            foreach (var curr in currentFloor)
            {
                MapNode target;
                if (nextUnconnected.Count > 0)
                {
                    int idx = Random.Range(0, nextUnconnected.Count);
                    var enumerator = nextUnconnected.GetEnumerator();
                    for (int k = 0; k <= idx; k++) enumerator.MoveNext();
                    target = enumerator.Current;
                    nextUnconnected.Remove(target);
                }
                else
                {
                    target = shuffledNext[Random.Range(0, shuffledNext.Count)];
                }
                curr.ConnectTo(target);
            }

            // 2) Ensure every nextFloor node has incoming
            foreach (var orphan in nextUnconnected)
            {
                var randomPrev = currentFloor[Random.Range(0, currentFloor.Count)];
                randomPrev.ConnectTo(orphan);
            }

            // 3) Add extra random edges for branching
            float extraEdgeChance = 0.3f;
            for (int i = 0; i < currentFloor.Count; i++)
            {
                for (int j = 0; j < nextFloor.Count; j++)
                {
                    var curr = currentFloor[i];
                    var nxt = nextFloor[j];
                    if (!curr.Connections.Contains(nxt) && Random.value < extraEdgeChance)
                    {
                        curr.ConnectTo(nxt);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Allows MapVisualizer to fetch the generated floors.
    /// </summary>
    public List<List<MapNode>> GetFloors()
    {
        return _floors;
    }

    #endregion

    // ========================================================================

    /// <summary>
    /// (Optional) Draw gizmos in editor to visualize the generated map.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (_floors == null) return;

        foreach (var floorList in _floors)
        {
            foreach (var node in floorList)
            {
                switch (node.Type)
                {
                    case MapNodeType.Combat:
                        UnityEditor.Handles.color = Color.red;
                        break;
                    case MapNodeType.Shop:
                        UnityEditor.Handles.color = Color.green;
                        break;
                    case MapNodeType.Treasure:
                        UnityEditor.Handles.color = Color.cyan;
                        break;
                    case MapNodeType.Elite:
                        UnityEditor.Handles.color = Color.magenta;
                        break;
                    case MapNodeType.Corruption:
                        UnityEditor.Handles.color = Color.blue;
                        break;
                    case MapNodeType.Boss:
                        UnityEditor.Handles.color = Color.yellow;
                        break;
                    default:
                        UnityEditor.Handles.color = Color.white;
                        break;
                }

                UnityEditor.Handles.DrawSolidDisc(
                    new Vector3(node.Position.x, node.Position.y, 0f),
                    Vector3.forward,
                    0.2f);

                foreach (var conn in node.Connections)
                {
                    Gizmos.color = Color.white;
                    Vector3 from = new Vector3(node.Position.x, node.Position.y, 0f);
                    Vector3 to = new Vector3(conn.Position.x, conn.Position.y, 0f);
                    Gizmos.DrawLine(from, to);
                }
            }
        }
#endif
    }
}
