using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UEnums;

[System.Serializable]
public struct RarityWeight
{
    public Rarity rarity;
    [Range(0, 1)]
    public float weight;
}

public class CardRewardManager : MonoBehaviour
{
    #region Variables and Properties
    public static CardRewardManager Instance = null;

    [Header("Global Settings")]
    [Tooltip("How many cards each actor sees per reward")]
    public int rewardsPerActor = 3;
    [Tooltip("Chance to pick a specialization card (if actor has one)")]
    [Range(0, 1)]
    public float specializationChance = 0.2f;

    [Header("Rarity Weights (should sum to 1)")]
    public List<RarityWeight> rarityWeightsList;

    // fast lookup
    private Dictionary<Rarity, float> _rarityWeights;
    #endregion

    // ========================================================================

    void Awake()
    {
        if (Instance == null) Instance = this;
        _rarityWeights = rarityWeightsList
            .ToDictionary(x => x.rarity, x => x.weight);
    }

    // ========================================================================

    /// <summary>
    /// Returns N random CardData for the given actor,
    /// pulling from actor.Data.Race and actor.Data.Specialization.
    /// </summary>
    public List<CardData> GenerateRewardsForActor(ActorManager actor)
    {
        var results = new List<CardData>();

        // 1) Grab this actor's pools
        var racePool = actor.Data.ActorRace.RacialCards;
        var specPool = actor.Data.HasSpecialization && actor.Data.ActorSpecialization != null
                     ? actor.Data.ActorSpecialization.SpecializationCards
                     : new List<CardData>();

        bool hasSpec = specPool != null && specPool.Count > 0;

        for (int i = 0; i < rewardsPerActor; i++)
        {
            // 2) Decide pool: specialization (rare) or racial
            bool pickSpec = hasSpec && Random.value < specializationChance;
            var pool = pickSpec ? specPool : racePool;

            // 3) Roll weighted rarity
            Rarity rar = PickRandomRarity();

            // 4) Filter by rarity (or fallback to whole pool)
            var candidates = pool.Where(c => c.CardRarity == rar).ToList();
            if (candidates.Count == 0)
                candidates = pool;

            // 5) Pick one at random
            results.Add(candidates[Random.Range(0, candidates.Count)]);
        }

        return results;
    }

    private Rarity PickRandomRarity()
    {
        float total = _rarityWeights.Values.Sum();
        float roll = Random.value * total;
        float accum = 0f;

        foreach (var kv in _rarityWeights)
        {
            accum += kv.Value;
            if (roll <= accum)
                return kv.Key;
        }

        // Fallback
        return _rarityWeights.Keys.Last();
    }

    // ========================================================================
}
