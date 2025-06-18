using Sirenix.OdinInspector;
using UnityEngine;

public abstract class RelicData : ScriptableObject
{
    [FoldoutGroup("Basic Info", expanded: true)]
    [SerializeField] private string _relicName = string.Empty;
    public string RelicName => _relicName;

    [FoldoutGroup("Basic Info", expanded: true)]
    [SerializeField] private string _relicDescription = string.Empty;
    public string RelicDescription => _relicDescription;

    [FoldoutGroup("Basic Info", expanded: true)]
    [SerializeField] private UEnums.Rarity _relicRarity = UEnums.Rarity.Common;
    public UEnums.Rarity RelicRarity => _relicRarity;   

    [FoldoutGroup("Basic Info", expanded: true)]
    [PreviewField(height: 100, alignment: ObjectFieldAlignment.Left)]
    [SerializeField] private Sprite _relicImage = null;
    public Sprite RelicImage => _relicImage;

    // ========================================================================

    public abstract void OnEquip(ActorManager holder);
    public abstract void OnUnequip(ActorManager holder);

    // ========================================================================

    public virtual void OnTurnStart(ActorManager holder) { }
    public virtual void OnTurnEnd(ActorManager holder) { }

    // ========================================================================

    public virtual bool CheckRemovalCondition(ActorManager holder) => false;

    // ========================================================================
}
