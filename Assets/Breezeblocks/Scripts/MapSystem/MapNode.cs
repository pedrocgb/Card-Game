using System.Collections.Generic;
using UnityEngine;
using static UEnums;

/// <summary>
/// Represents a single node on the roguelike map (e.g. combat, shop, rest, boss).
/// </summary>
public class MapNode
{

    /// <summary>
    /// Which floor (0-based) this node lives on.
    /// </summary>
    public int FloorIndex { get; private set; }

    /// <summary>
    /// 2D position in world or Canvas space (you decide later how to interpret this).
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The type of this node (combat, shop, etc.).
    /// </summary>
    public MapNodeType Type { get; set; }

    /// <summary>
    /// Forward connections (edges) to nodes on the next floor.
    /// </summary>
    public List<MapNode> Connections { get; private set; }

    public MapNode(int floorIndex, Vector2 position, MapNodeType type)
    {
        FloorIndex = floorIndex;
        Position = position;
        Type = type;
        Connections = new List<MapNode>();
    }

    /// <summary>
    /// Connect this node to a node on the next floor.
    /// </summary>
    public void ConnectTo(MapNode other)
    {
        if (other != null && !Connections.Contains(other))
        {
            Connections.Add(other);
        }
    }
}
