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
    public void HighLightActors(ActorManager Source, List<UEnums.Positions> ValidPositions, UEnums.Target TargetAlignment, bool CanTargetSelf)
    {
        // Clear all highlights before applying new ones.
        ClearHightLights();

        // If the actor using the card is PLAYER, highlight all actors (or self) based on the target alignment and positioning.
        if (Source is PlayerActor)
        {
            switch (TargetAlignment)
            {
                default:
                case UEnums.Target.Self:
                    Source.HightLightActor(TargetAlignment);
                    break;
                case UEnums.Target.Ally:
                    foreach (var a in _combatManager.PlayerActors)
                    {
                        bool valid = ValidPositions.Contains(a.Positioning.CurrentPosition);
                        if (a == Source && !CanTargetSelf)
                            valid = false;

                        if (valid)
                            a.HightLightActor(TargetAlignment);
                    }
                    break;
                case UEnums.Target.Enemy:
                    foreach (var e in _combatManager.EnemyActors)
                    {
                        bool valid = ValidPositions.Contains(e.Positioning.CurrentPosition);
                        if (valid)
                            e.HightLightActor(TargetAlignment);
                    }
                    break;
            }
        }

        // If the actor using the card is PLAYER, highlight all actors (or self) based on the target alignment and positioning.
        else if (Source is EnemyActor)
        {
            switch (TargetAlignment)
            {
                default:
                case UEnums.Target.Self:
                    Source.HightLightActor(TargetAlignment);
                    break;
                case UEnums.Target.Ally:
                    foreach (var a in _combatManager.EnemyActors)
                    {
                        bool valid = ValidPositions.Contains(a.Positioning.CurrentPosition);
                        if (a == Source && !CanTargetSelf)
                            valid = false;

                        if (valid)
                            a.HightLightActor(TargetAlignment);
                    }
                    break;
                case UEnums.Target.Enemy:
                    foreach (var e in _combatManager.PlayerActors)
                    {
                        bool valid = ValidPositions.Contains(e.Positioning.CurrentPosition);
                        if (valid)
                            e.HightLightActor(TargetAlignment);
                    }
                    break;
            }
        }
    }

    public void HighTargetActors(ActorManager Source, List<UEnums.Positions> ValidPositions, UEnums.Target TargetAlignment, bool CanTargetSelf)
    {
        // If the actor using the card is PLAYER, highlight all actors (or self) based on the target alignment and positioning.
        if (Source is PlayerActor)
        {
            switch (TargetAlignment)
            {
                default:
                case UEnums.Target.Self:
                    Source.HighTargetActor();
                    break;
                case UEnums.Target.Ally:
                    foreach (var a in _combatManager.PlayerActors)
                    {
                        bool valid = ValidPositions.Contains(a.Positioning.CurrentPosition);
                        if (a == Source && !CanTargetSelf)
                            valid = false;

                        if (valid)
                            a.HighTargetActor();
                    }
                    break;
                case UEnums.Target.Enemy:
                    foreach (var e in _combatManager.EnemyActors)
                    {
                        bool valid = ValidPositions.Contains(e.Positioning.CurrentPosition);
                        if (valid)
                            e.HighTargetActor();
                    }
                    break;
            }
        }

        // If the actor using the card is PLAYER, highlight all actors (or self) based on the target alignment and positioning.
        else if (Source is EnemyActor)
        {
            switch (TargetAlignment)
            {
                default:
                case UEnums.Target.Self:
                    Source.HighTargetActor();
                    break;
                case UEnums.Target.Ally:
                    foreach (var a in _combatManager.EnemyActors)
                    {
                        bool valid = ValidPositions.Contains(a.Positioning.CurrentPosition);
                        if (a == Source && !CanTargetSelf)
                            valid = false;

                        if (valid)
                            a.HighTargetActor();
                    }
                    break;
                case UEnums.Target.Enemy:
                    foreach (var e in _combatManager.PlayerActors)
                    {
                        bool valid = ValidPositions.Contains(e.Positioning.CurrentPosition);
                        if (valid)
                            e.HighTargetActor();
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Remove all highlights from all sources.
    /// </summary>
    public void ClearHightLights()
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
