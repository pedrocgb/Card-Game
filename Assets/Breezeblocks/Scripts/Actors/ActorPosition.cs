using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

public class ActorPosition : MonoBehaviour
{
    #region Variables and Properties
    private ActorManager _parentActor = null;

    // Actor position
    [FoldoutGroup("Position", expanded: true)]
    [SerializeField]
    private UEnums.Positions _currentPosition = UEnums.Positions.Front;
    public UEnums.Positions CurrentPosition
    {
        get { return _currentPosition; }
        set { _currentPosition = value; }
    }
    #endregion

    // ========================================================================

    #region Initialization Methods
    private void Awake()
    {
        _parentActor = GetComponent<ActorManager>();    
    }
    #endregion

    // ========================================================================

    #region Movement and Positioning Methods
    public void SetCombatPosition(int newPos)
    {
        newPos = Mathf.Clamp(newPos, 1, 4);
        _currentPosition = (UEnums.Positions)newPos;

        var anchor = PositionAchorSlots.GetAnchor(_parentActor, newPos);
        AnimatePosition(anchor.position, 0.75f);
    }

    public void MoveActor(int amount)
    {
        if (_parentActor.Stats.IsRestrained)
        {
            Console.Log($"{_parentActor.ActorName} is restrained and cannot move.");
            return;
        }

        PositionsManager.MoveActor(_parentActor, amount);
    }

    public void ForceMovement(int amount)
    {
        if (_parentActor.Stats.IsRestrained)
        {
            Console.Log($"{_parentActor.ActorName} is restrained and cannot move.");
            return;
        }

        PositionsManager.MoveActor(_parentActor, amount);
    }
    #endregion

    // ========================================================================

    #region Animation Methods
    private void AnimatePosition(Vector3 worldTargetPosition, float duration)
    {
        // Animate the position of the actor
        transform.DOMove(worldTargetPosition, duration)
            .SetEase(Ease.OutBack);
    }
    #endregion

    // ========================================================================
}
