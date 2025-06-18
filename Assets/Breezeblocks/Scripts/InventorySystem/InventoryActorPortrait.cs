using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryActorPortrait : MonoBehaviour
{
    #region Variables and Properties
    private ActorManager _myActor = null;
    public ActorManager Actor => _myActor;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image _portraitImage = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _actorName = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _actorRace = null;
    #endregion

    // ========================================================================

    public void Initialize(ActorManager actor)
    {
        _myActor = actor;
        _portraitImage.sprite = actor.Data.Portrait;
        _actorName.text = actor.ActorName;
        if (actor.Data.HasSpecialization)
            _actorRace.text = actor.Data.ActorRace.RaceName + " (" + actor.Data.ActorSpecialization.SpecializationName + ")";
        else
            _actorRace.text = actor.Data.ActorRace.RaceName;
    }

    // ========================================================================

    public void OnPortraitSelection()
    {
        InventoryManager.SelectPortrait(_myActor);
    }

    // ========================================================================
}
