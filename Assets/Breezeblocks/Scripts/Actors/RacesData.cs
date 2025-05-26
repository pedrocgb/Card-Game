using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Race", menuName = "Breezeblocks/New Race")]
public class RacesData : ScriptableObject
{
    [FoldoutGroup("Info", expanded: true)]
    [SerializeField]
    private string _raceName = string.Empty;
    public string RaceName => _raceName;
    [FoldoutGroup("Info", expanded: true)]
    [SerializeField]
    [TextArea(3,5)]
    private string _raceDescription = string.Empty;
    public string RaceDescription => _raceDescription;

    // ========================================================================

    // Add race Imunities

    // Add race Effects

    // ========================================================================
}
