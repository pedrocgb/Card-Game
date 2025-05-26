using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Specialization", menuName = "Breezeblocks/Specialization")]
public class SpecializationData : ScriptableObject
{
    [FoldoutGroup("Info", expanded: true)]
    [SerializeField]
    private string _specializationName = string.Empty;
    public string SpecializationName => _specializationName;
    [FoldoutGroup("Info", expanded: true)]
    [SerializeField]
    [TextArea(3, 5)]
    private string _specializationDescription = string.Empty;
    public string SpecializationDescription => _specializationDescription;

    // ========================================================================

    [FoldoutGroup("Cards", expanded: true)]
    [SerializeField]
    private List<CardData> _specializationCards = new List<CardData>();
    public List<CardData> SpecializationCards => _specializationCards;

    // ========================================================================
}
