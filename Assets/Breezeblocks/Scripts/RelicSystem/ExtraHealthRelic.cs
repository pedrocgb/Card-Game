using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ExtraHealthRelic", menuName = "Breezeblocks/Relics/Extra Health Relic")]
public class ExtraHealthRelic : RelicData
{
    [FoldoutGroup("Relic Power", expanded: true)]
    [SerializeField] private bool _isFlatBonus = true;
    [FoldoutGroup("Relic Power", expanded: true)]
    [ShowIf("_isFlatBonus")]
    [SerializeField] private int _bonusFlatHp = 0;
    [FoldoutGroup("Relic Power", expanded: true)]
    [HideIf("_isFlatBonus")]
    [Range(0,1f)]
    [SerializeField] private float _bonusPercentageHp = 0f;

    // ========================================================================

    public override void OnEquip(ActorManager holder)
    {
        switch (_isFlatBonus)
        {
            case true:
                holder.Stats.IncreaseMaxHealth(_bonusFlatHp, true);
                break;
            case false:
                holder.Stats.IncreaseMaxHealth(_bonusPercentageHp, true);
                break;
        }
    }

    public override void OnUnequip(ActorManager holder)
    {
        switch (_isFlatBonus)
        {
            case true:
                holder.Stats.IncreaseMaxHealth(_bonusFlatHp, false);
                break;
            case false:
                holder.Stats.IncreaseMaxHealth(_bonusPercentageHp, false);
                break;
        }
    }

    // ========================================================================
}
