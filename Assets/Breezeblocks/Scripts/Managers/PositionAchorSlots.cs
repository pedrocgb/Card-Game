using Sirenix.OdinInspector;
using UnityEngine;

public class PositionAchorSlots : MonoBehaviour
{
    #region Variables and Properties
    public static PositionAchorSlots Instance { get; private set; }

    [FoldoutGroup("Slots", expanded:true)]
    [SerializeField]
    private Transform[] _playerSlots = new Transform[4];
    [FoldoutGroup("Slots", expanded: true)]
    [SerializeField]
    private Transform[] _enemySlots = new Transform[4];
    #endregion

    // ========================================================================

    #region Initialization Methods
    private void Awake()
    {
        // Singleton pattern to ensure only one instance of this class exists
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    // ========================================================================

    #region Static Methods
    public static Transform GetAnchor(ActorManager Actor, int Pos)
    {
        if (Instance == null)
        {
            Debug.LogError("PositionAchorSlots instance is not initialized.");
            return null;
        }

        return Instance.getAnchor(Actor, Pos);
    }
    #endregion

    // ========================================================================

    #region Local Methods
    private Transform getAnchor(ActorManager actor, int pos)
    {
        pos = Mathf.Clamp(pos, 1, 4) - 1;
        return actor is PlayerActor ? _playerSlots[pos] : _enemySlots[pos];
    }
    #endregion

    // ========================================================================
}
