using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ExtraActionRelic", menuName = "Breezeblocks/Relics/Extra Action Relic")]
public class ExtraActionRelic : RelicData
{
    [FoldoutGroup("Relic Power Info", expanded: true)]
    [SerializeField] private int _actionAmount = 1;

    public override void OnEquip(ActorManager holder)
    {
        
    }

    public override void OnUnequip(ActorManager holder)
    {
        
    }

    public override void OnTurnStart(ActorManager holder)
    {
       holder.Stats.IncreaseAction(_actionAmount);
    }
}
