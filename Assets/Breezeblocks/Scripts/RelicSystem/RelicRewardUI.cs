using Sirenix.OdinInspector;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicRewardUI : MonoBehaviour
{
    #region Variables and Properties
    private static RelicRewardUI Instance = null;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private GameObject _relicPanel = null;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image _relicImage = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _relicNameText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI _relicDescriptionText = null;

    [FoldoutGroup("Portraits", expanded: true)]
    [SerializeField] private TextMeshProUGUI _selectedActorNameText = null;
    [FoldoutGroup("Portraits", expanded: true)]
    [SerializeField] private RelicActorPortrait[] _relicActorPortrait = new RelicActorPortrait[4];

    private ActorManager _selectedActor = null;
    #endregion

    // ========================================================================

    private void Awake()
    {
        Instance = this;
    }

    // ========================================================================

    #region Static Methods
    public static void SelectActor(ActorManager NewActor)
    {
        if (Instance == null) return;
 
        Instance.selectActor(NewActor);
    }

    public static void ShowUI(RelicData RelicData)
    {
        if (Instance == null) return;
        Instance.showUI(RelicData);
    }
    #endregion

    // ========================================================================

    public void ConfirmButton()
    {
        _selectedActor.MyRelics.TryEquip(RelicRewardManager.GeneratedRelic);
        GameManager.EndEvent(UEnums.MapNodeType.Treasure);
        _relicPanel.SetActive(false);
    }

    // ========================================================================

    #region Local Methods
    public void showUI(RelicData relicData)
    {
        _relicPanel.SetActive(true);
        _relicImage.sprite = relicData.RelicImage;
        _relicNameText.text = relicData.RelicName;
        _relicDescriptionText.text = relicData.RelicDescription;

        var sorted = CombatManager.Instance.PlayerActors
                    .Where(a => !a.Stats.IsDead)
                    .OrderBy(a => a.Positioning.CurrentPosition)  // casts enum to its underlying int
                    .ToList();

        for (int i = 0; i < _relicActorPortrait.Length; i++)
        {
            if (i < sorted.Count)
            {
                _relicActorPortrait[i].gameObject.SetActive(true);
                _relicActorPortrait[i].Initialize(sorted[i]);
            }
            else
            {
                _relicActorPortrait[i].gameObject.SetActive(false);
            }
        }

        _relicActorPortrait[0].OnActorSelect();
    }

    public void selectActor(ActorManager newActor)
    {
        _selectedActorNameText.text = newActor.ActorName;
        foreach (var p in _relicActorPortrait)        
            p.OnDeselectActor();
        
        _selectedActor = newActor;
    }
    #endregion

    // ========================================================================
}
