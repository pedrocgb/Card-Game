using System.Collections.Generic;
using UnityEngine;

public class TargetingManager : MonoBehaviour
{
    #region Variables and Properties
    public static TargetingManager Instance;

    private CombatManager _combatManager = null;
    private ActorManager _target = null;
    public ActorManager CurrentTarget => _target;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake()
    {
        Instance = this;

        _combatManager = FindAnyObjectByType<CombatManager>();
    }
    #endregion

    // ========================================================================

    public void SetTarget(ActorManager newTarget)
    {
        _target = newTarget;
    }

    // ========================================================================

    #region HighLight Effect Methods
    /// <summary>
    /// HighLight all enemies based on the card's targetting and hostility.
    /// </summary>
    /// <param name="ValidPositions"></param>
    /// <param name="TargetAlignment"></param>
    public void HighLightActors(ActorManager Actor, List<UEnums.Positions> ValidPositions, UEnums.Target TargetAlignment)
    {
        // Clear all highlights before applying new ones.
        ClearHightLights();


        // If the actor using the card is PLAYER, highlight all actors (or self) based on the target alignment and positioning.
        if (Actor is PlayerActor)
        {
            switch (TargetAlignment)
            {
                default:
                case UEnums.Target.Self:
                    Actor.HightLightActor(TargetAlignment);
                    break;
                case UEnums.Target.Ally:
                    foreach (var a in _combatManager.PlayerActors)
                    {
                        bool valid = ValidPositions.Contains(a.CurrentPosition);
                        if (valid)
                            a.HightLightActor(TargetAlignment);
                    }
                    break;
                case UEnums.Target.Enemy:
                    foreach (var e in _combatManager.EnemyActors)
                    {
                        bool valid = ValidPositions.Contains(e.CurrentPosition);
                        if (valid)
                            e.HightLightActor(TargetAlignment);
                    }
                    break;
            }
        }

        // If the actor using the card is PLAYER, highlight all actors (or self) based on the target alignment and positioning.
        else if (Actor is EnemyActor)
        {
            switch (TargetAlignment)
            {
                default:
                case UEnums.Target.Self:
                    Actor.HightLightActor(TargetAlignment);
                    break;
                case UEnums.Target.Ally:
                    foreach (var a in _combatManager.EnemyActors)
                    {
                        bool valid = ValidPositions.Contains(a.CurrentPosition);
                        if (valid)
                            a.HightLightActor(TargetAlignment);
                    }
                    break;
                case UEnums.Target.Enemy:
                    foreach (var e in _combatManager.PlayerActors)
                    {
                        bool valid = ValidPositions.Contains(e.CurrentPosition);
                        if (valid)
                            e.HightLightActor(TargetAlignment);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Remove all highlights from all sources.
    /// </summary>
    private void ClearHightLights()
    {
        foreach (var e in _combatManager.EnemyActors)
        {
            e.RemoveHighLight();
        }
        foreach (var p in _combatManager.PlayerActors)
        {
            p.RemoveHighLight();
        }
    }
    #endregion

    // ========================================================================
}
