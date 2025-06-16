using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicActorPortrait : MonoBehaviour
{
    #region Variables and Properties
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image _portrait = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image _portraitBg = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _nameText = null;

    [FoldoutGroup("Sprites", expanded: true)]
    [SerializeField] private Sprite _selectedPortraitBg = null;
    [FoldoutGroup("Sprites", expanded: true)]
    [SerializeField] private Sprite _defaultPortraitBg = null;

    private ActorManager _myActor = null;
    public ActorManager Actor => _myActor;
    #endregion

    // ========================================================================

    public void Initialize(ActorManager actor)
    {
        _portrait.sprite = actor.Data.Portrait;
        _nameText.text = actor.ActorName;
        _myActor = actor;
    }

    // ========================================================================

    #region Selection Methods
    public void OnActorSelect()
    {
        RelicRewardUI.SelectActor(_myActor);
        _portraitBg.sprite = _selectedPortraitBg;
    }

    public void OnDeselectActor()
    {
        _portraitBg.sprite = _defaultPortraitBg;
    }
    #endregion

    // ========================================================================
}
