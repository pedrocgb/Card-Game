using System.Collections.Generic;
using UnityEngine;
using static UEnums; 

public class RewardManager : MonoBehaviour
{
    #region Variables and Properties
    public static RewardManager Instance = null;

    [Header("All possible relics")]
    [Tooltip("Drag in every RelicData asset that can ever drop")]
    [SerializeField] private List<RelicData> _allRelics;

    [Header("Rarity Weights")]
    [Tooltip("Configure how common each Rarity should be")]
    [SerializeField] private List<RarirtyWeight> _rarityWeights;

    // Fast lookup table
    private Dictionary<Rarity, float> _weightMap;
    private RelicData _generatedRelic = null;
    public static RelicData GeneratedRelic => Instance._generatedRelic;
    #endregion

    // ========================================================================

    private void Awake()
    {
        Instance = this;

        // Build dictionary for quick reads
        _weightMap = new Dictionary<Rarity, float>();
        foreach (var rw in _rarityWeights)
        {
            _weightMap[rw.Rarity] = Mathf.Max(0, rw.Weight);
        }
    }

    // ========================================================================

    /// <summary>
    /// Generates one relic at random, weighted by the rarity→weight map.
    /// </summary>
    public void GenerateRelic()
    {
        if (_allRelics == null || _allRelics.Count == 0)
            return;

        // 1) Sum total weight
        float totalWeight = 0;
        foreach (var relic in _allRelics)
        {
            if (_weightMap.TryGetValue(relic.RelicRarity, out float w))
                totalWeight += w;
        }

        if (totalWeight <= 0)
            return;

        // 2) Roll a random value in [0, totalWeight)
        float roll = Random.Range(0, totalWeight);
        float cumulative = 0;

        // 3) Find which relic corresponds to that roll
        foreach (var relic in _allRelics)
        {
            if (!_weightMap.TryGetValue(relic.RelicRarity, out float w))
                continue;

            cumulative += w;
            if (roll < cumulative)
            {
                _generatedRelic = relic;
                RelicRewardUI.ShowUI(_generatedRelic);
                return;
            }
        }
    }

    public void GenerateGold()
    {

    }

    public void GenerateSoul()
    {

    }

    // ========================================================================
}

[System.Serializable]
public class RarirtyWeight
{
    public UEnums.Rarity Rarity;
    [Range(0,1f)]
    public float Weight;
}
