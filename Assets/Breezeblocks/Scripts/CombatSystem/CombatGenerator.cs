using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static UEnums;

public class CombatGenerator : MonoBehaviour
{
    #region Variables and Properties
    [FoldoutGroup("References", expanded: true)]
    [SerializeField]
    [InfoBox("Reference to your MapGenerator (so we can fetch floors after map is built).", InfoMessageType.None)]
    private MapGenerator _mapGenerator;

    [FoldoutGroup("Enemy Party Presets", expanded: true)]
    [SerializeField]
    [InfoBox("Define one entry per level. Each level has a list of possible EnemyParties. At runtime we pick one party at random.", InfoMessageType.None)]
    private List<LevelPreset> _levelPresets = new List<LevelPreset>();

    [System.Serializable]
    public class LevelPreset
    {
        [Tooltip("This preset applies to nodes on floorIndex = (level - 1). E.g. level=1 → floorIndex=0, level=2 → floorIndex=1.")]
        public int level;

        [Tooltip("List of different enemy parties you can choose from at this level.")]
        public List<EnemyParty> parties = new List<EnemyParty>();
    }

    [System.Serializable]
    public class EnemyParty
    {
        [InfoBox("A list of enemy data which will feed the enemy gameobject (one = one enemy) for this party. Also the position matters, " +
            "first is going to be in position 1 and so on.", InfoMessageType.Info)]
        public List<ActorData> enemyPrefabs = new List<ActorData>();
    }
    #endregion

    // ========================================================================

    #region Combat Methods
    /// <summary>
    /// Iterates over every floor & node in the generated map.
    /// For each node with Type == Combat or Elite, pick a random party from _levelPresets[level].
    /// Then assign that party’s prefabs into node.EnemyPrefabs.
    /// </summary>
    public void GenerateCombats()
    {
        List<List<MapNode>> floors = _mapGenerator.GetFloors();
        if (floors == null || floors.Count == 0)
        {
            Debug.LogWarning("[CombatGenerator] No floors found. Did MapGenerator.GenerateMap() run?");
            return;
        }

        // Build a quick lookup: level → all presets for that level
        Dictionary<int, LevelPreset> presetLookup = new Dictionary<int, LevelPreset>();
        foreach (var lp in _levelPresets)
        {
            if (!presetLookup.ContainsKey(lp.level))
                presetLookup.Add(lp.level, lp);
            else
                Debug.LogWarning($"[CombatGenerator] Duplicate presets for level {lp.level}");
        }

        // For each floor f = 0..floors.Count-1, level = f+1
        for (int f = 0; f < floors.Count; f++)
        {
            int level = f + 1;

            // If we have no presets for this level, skip all nodes on that floor
            if (!presetLookup.ContainsKey(level))
                continue;

            LevelPreset lpref = presetLookup[level];
            if (lpref.parties == null || lpref.parties.Count == 0)
            {
                Debug.LogWarning($"[CombatGenerator] No party presets defined for level {level}.");
                continue;
            }

            foreach (var node in floors[f])
            {
                // Only generate for Combat or Elite nodes (skip Shop, Rest, etc.)
                if (node.Type != MapNodeType.Combat && node.Type != MapNodeType.Elite)
                    continue;

                // Pick a random party from this level’s list
                int idx = Random.Range(0, lpref.parties.Count);
                EnemyParty chosenParty = lpref.parties[idx];

                // Assign those prefabs into the node
                node.EnemiesData = new List<ActorData>(chosenParty.enemyPrefabs);
            }
        }

        Debug.Log("[CombatGenerator] Assigned enemy parties to all combat/elite nodes.");
    }
    #endregion

    // ========================================================================
}
