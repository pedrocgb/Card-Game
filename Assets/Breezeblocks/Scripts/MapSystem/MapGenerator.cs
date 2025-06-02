using System.Collections.Generic;
using UnityEngine;
using static UEnums;

/// <summary>
/// Generates a Slay-the-Spire-style branching map of MapNode objects.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    [Header("Map Dimensions")]
    [Tooltip("Total number of floors (rows) including start (floor 0) and boss (floorCount - 1).")]
    public int floors = 7;

    [Tooltip("Minimum number of nodes per floor (excluding start and boss floors).")]
    public int minNodesPerFloor = 1;

    [Tooltip("Maximum number of nodes per floor (excluding start and boss floors).")]
    public int maxNodesPerFloor = 4;

    [Header("Layout Spacing")]
    [Tooltip("Horizontal spacing between nodes in the same floor (units).")]
    public float horizontalSpacing = 3f;

    [Tooltip("Vertical spacing between floors (units).")]
    public float verticalSpacing = 2.5f;

    [Header("Node Type Weights")]
    [Tooltip("Weight for creating a Combat node.")]
    public float weightCombat = 1f;

    [Tooltip("Weight for creating a Shop node.")]
    public float weightShop = 0.1f;

    [Tooltip("Weight for creating an Elite node.")]
    public float weightElite = 0.1f;

    [Tooltip("Weight for creating a Treasure/Event node. (Optional—you can reuse existing enum or add more.)")]
    public float weightTreasure = 0.3f;

    [Tooltip("Weight for creating a Shop node.")]
    public float weighCorruption = 0.1f;

    [Tooltip("Weight for forcing Boss on final floor (should be highest or 1.0).")]
    public float weightBoss = 1f;

    // The entire map stored floor by floor
    private List<List<MapNode>> _floors;

    /// <summary>
    /// Call this to rebuild the map at runtime (e.g. on Start or via inspector button).
    /// </summary>
    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        // 1) Initialize data structures.
        _floors = new List<List<MapNode>>();

        // 2) Floor 0: always a single START node at (0, 0).
        var floor0 = new List<MapNode>();
        var startNode = new MapNode(
            floorIndex: 0,
            position: new Vector2(0, 0),
            type: MapNodeType.Combat // You can treat floor 0 as a 'safe combat' or 'start' type if desired.
        );
        floor0.Add(startNode);
        _floors.Add(floor0);

        // 3) Build floors 1..(floors-2) with randomized node counts and types.
        for (int f = 1; f < floors - 1; f++)
        {
            int countThisFloor = Random.Range(minNodesPerFloor, maxNodesPerFloor + 1);
            var thisFloor = new List<MapNode>();

            // For each node in this floor, pick a random type according to weights.
            for (int i = 0; i < countThisFloor; i++)
            {
                MapNodeType chosenType = PickNodeType(randomForBoss: false);
                // Position will be set later after we know how many nodes are in this floor.
                var node = new MapNode(floorIndex: f, position: Vector2.zero, type: chosenType);
                thisFloor.Add(node);
            }

            _floors.Add(thisFloor);
        }

        // 4) Final floor: exactly one BOSS node.
        var lastFloor = new List<MapNode>();
        var bossNode = new MapNode(
            floorIndex: floors - 1,
            position: Vector2.zero,
            type: MapNodeType.Boss
        );
        lastFloor.Add(bossNode);
        _floors.Add(lastFloor);

        // 5) Assign positions for every node now that we know counts per floor.
        AssignNodePositions();

        // 6) Connect nodes between floors (0→1, 1→2, …).
        ConnectFloors();

        // (Optional) 7) At this point, _floors holds all nodes and their forward connections.
        // You can now instantiate prefabs at each node.Position or draw gizmos to visualize.
        Debug.Log("Map generation complete. Total floors: " + floors);
    }

    /// <summary>
    /// Chooses a NodeType at random based on the weights.
    /// If randomForBoss == true, returns Boss type forcibly.
    /// </summary>
    private MapNodeType PickNodeType(bool randomForBoss)
    {
        if (randomForBoss)
            return MapNodeType.Boss;

        // Build an array of types and their cumulative weights.
        // Note: We include extra types whenever you expand the enum.
        var types = new List<MapNodeType>
        {
            MapNodeType.Combat,
            MapNodeType.Treasure,
            MapNodeType.Elite,
            //MapNodeType.Shop,
            //MapNodeType.Corruption
            // If you add Treasure or Event to the enum, include it here:
            // MapNode.NodeType.Treasure,
            // etc.
        };

        var weights = new List<float>
        {
            weightCombat,
            weightTreasure,
            weightElite,
            //weighCorruption,
            //weightShop,
        };

        // Compute total weight.
        float total = 0f;
        foreach (var w in weights) total += w;

        float r = Random.Range(0f, total);
        float cumulative = 0f;
        for (int i = 0; i < types.Count; i++)
        {
            cumulative += weights[i];
            if (r <= cumulative)
            {
                return types[i];
            }
        }

        // Fallback:
        return MapNodeType.Combat;
    }

    /// <summary>
    /// Computes and assigns a 2D position for each node, stacking floors vertically.
    /// Floors are spaced by verticalSpacing. Within each floor, nodes are centered horizontally
    /// and spaced by horizontalSpacing.
    /// </summary>
    private void AssignNodePositions()
    {
        for (int f = 0; f < _floors.Count; f++)
        {
            var thisFloor = _floors[f];
            int count = thisFloor.Count;
            float totalWidth = (count - 1) * horizontalSpacing;
            // Start X so that the floor is horizontally centered at X=0.
            float startX = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                float x = startX + i * horizontalSpacing;
                float y = -f * verticalSpacing;
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

            // 1) Ensure each node in currentFloor connects to at least one in nextFloor.
            // We’ll keep track of which nextFloor nodes have already been “hit”.
            var nextUnconnected = new HashSet<MapNode>(nextFloor);

            // Shuffle nextFloor list for randomness
            var shuffledNext = new List<MapNode>(nextFloor);
            for (int i = 0; i < shuffledNext.Count; i++)
            {
                int j = Random.Range(i, shuffledNext.Count);
                var temp = shuffledNext[i];
                shuffledNext[i] = shuffledNext[j];
                shuffledNext[j] = temp;
            }

            // First ensure every currentFloor node has at least one outgoing
            foreach (var curr in currentFloor)
            {
                // Randomly pick one unconnected next node if any remain; otherwise pick a random next
                MapNode target;
                if (nextUnconnected.Count > 0)
                {
                    int idx = Random.Range(0, nextUnconnected.Count);
                    // Grab that element by iterating
                    var enumerator = nextUnconnected.GetEnumerator();
                    for (int k = 0; k <= idx; k++) enumerator.MoveNext();
                    target = enumerator.Current;
                    nextUnconnected.Remove(target);
                }
                else
                {
                    // All next nodes already have at least one incoming; just pick randomly
                    target = shuffledNext[Random.Range(0, shuffledNext.Count)];
                }

                curr.ConnectTo(target);
            }

            // 2) Ensure each nextFloor node has at least one incoming:
            // If any nextFloor node is still in nextUnconnected, forcibly connect it from a random
            // node in currentFloor.
            foreach (var orphan in nextUnconnected)
            {
                var randomPrev = currentFloor[Random.Range(0, currentFloor.Count)];
                randomPrev.ConnectTo(orphan);
            }

            // 3) Add extra random edges for branching:
            // For every pair (curr, next), roll a small chance to connect even if already connected.
            float extraEdgeChance = 0.3f; // You can expose this as a public field if you want to tweak.
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

    /// <summary>
    /// (Optional) Draw gizmos in editor to visualize the generated map.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (_floors == null) return;

        // Draw nodes as circles and connections as lines.
        foreach (var floorList in _floors)
        {
            foreach (var node in floorList)
            {
                // Node color by type:
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
                    case MapNodeType.Boss:
                        UnityEditor.Handles.color = Color.yellow;
                        break;
                    // Add more colors if you add types.
                    default:
                        UnityEditor.Handles.color = Color.white;
                        break;
                }

                UnityEditor.Handles.DrawSolidDisc(new Vector3(node.Position.x, node.Position.y, 0f), Vector3.forward, 0.2f);

                // Draw connections:
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
