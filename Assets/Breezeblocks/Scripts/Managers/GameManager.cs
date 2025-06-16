using Sirenix.OdinInspector;
using UnityEngine;
using static UEnums;

public class GameManager : MonoBehaviour
{
    #region Variables and Properties
    private static GameManager Instance = null;

    [FoldoutGroup("Components")]
    [SerializeField]
    private GameObject _mapUi = null;
    [FoldoutGroup("Components")]
    [SerializeField]
    private GameObject _combatUi = null;
    [FoldoutGroup("Components")]
    [SerializeField]
    private GameObject _infoUi = null;
    [FoldoutGroup("Components")]
    [SerializeField]
    private GameObject _combatRewardPanel = null;

    [FoldoutGroup("Components/Actors")]
    [SerializeField]
    private Sprite _deadActorPortrait = null;
    public static Sprite DeadActorPortrait => Instance._deadActorPortrait;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        Instance = this;

        _combatUi.SetActive(false);
        _infoUi.SetActive(false);
        _mapUi.SetActive(true);
    }
    #endregion

    // ========================================================================

    #region Loop Methods
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            _infoUi.SetActive(!_infoUi.activeSelf);
        }
    }
    #endregion

    // ========================================================================

    #region Static Methods
    public static void StartEvent(MapNodeType Event)
    {
        if (Instance == null)
        {
            Debug.LogError("GameManager instance is not initialized.");
            return;
        }

        Instance.startEvent(Event);
    }

    public static void EndEvent(MapNodeType Event)
    {
        if (Instance == null)
        {
            Debug.LogError("GameManager instance is not initialized.");
            return;
        }
        Instance.endEvent(Event);
    }
    #endregion

    // ========================================================================

    #region Local Methods
    private void startEvent(MapNodeType mapEvent)
    {
        switch (mapEvent)
        {
            default:
            case MapNodeType.Combat:
                StartCombat();
                break;

            case MapNodeType.Shop:
                break;

            case MapNodeType.Treasure:
                RelicRewardManager.Instance.GenerateRelic();
                break;

            case MapNodeType.Elite:
                break;

            case MapNodeType.Boss:
                break;

            case MapNodeType.Corruption:
                break;
        }
    }

    private void endEvent(MapNodeType mapEvent)
    {
        switch (mapEvent)
        {
            default:
            case MapNodeType.Combat:
                EndCombat();
                break;
            case MapNodeType.Shop:
                break;
            case MapNodeType.Treasure:
                EndTreasureEvent();
                break;
            case MapNodeType.Elite:
                break;
            case MapNodeType.Boss:
                break;
            case MapNodeType.Corruption:
                break;
        }
    }
    #endregion

    // ========================================================================

    #region Events Methods
    private void StartCombat()
    {
        // Deactive Map UI and Show Combat UI
        _mapUi.SetActive(false);
        _combatUi.SetActive(true);
        PositionsManager.UpdatePositions();
        CombatManager.StartNewCombat();
    }

    private void EndCombat()
    {
        _mapUi.SetActive(true);
        _combatUi.SetActive(false);

        CombatManager.EndCombat();
    }

    private void EndTreasureEvent()
    {
        _mapUi.SetActive(true);
        MapVisualizer.Instance.CompleteEvent();
    }
    #endregion

    // ========================================================================
}
