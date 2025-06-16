using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ExtraDrawRelic", menuName = "Breezeblocks/Relics/Extra Draw Relic")]
public class ExtraDrawRelic : RelicData
{
    [FoldoutGroup("Relic Power Info", expanded: true)]
    [SerializeField] private int _drawAmount = 1;

    public override void OnEquip(ActorManager holder)
    {
        holder.Stats.IncreaseCardBuy(_drawAmount, true);
    }

    public override void OnUnequip(ActorManager holder)
    {
        holder.Stats.IncreaseCardBuy(_drawAmount, false);
    }

}

