using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using static UEnums;

public class UIconsDatabase : MonoBehaviour
{
    #region Variables and Properties
    public static UIconsDatabase Instance;

    private Dictionary<StatusEffects, StatusIconEntry> _lookup;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private List<StatusIconEntry> _entries;
    #endregion

    // ========================================================================

    #region Static Methods
    public static Sprite GetIcon(StatusEffects Type)
    {
        if (Instance == null)
            return null;

        return Instance.getIcon(Type);
    }

    public static StatusStackingMode GetStackingMode(StatusEffects Effect)
    {
        if (Instance == null)
            return StatusStackingMode.StackBoth;

        return Instance.getStackingMode(Effect);
    }
    #endregion

        // ========================================================================

        #region Initialization
    private void Awake()
    {
        Instance = this;
        _lookup = new Dictionary<StatusEffects, StatusIconEntry>();

        foreach (var entry in _entries)
        {
            _lookup[entry.type] = entry;
        }
    }
    #endregion

    // ========================================================================

    #region Icons Methods
    private Sprite getIcon(StatusEffects type)
    {
        return _lookup.TryGetValue(type, out var entry) ? entry.icon : null;
    }

    private StatusStackingMode getStackingMode(StatusEffects effect)
    {
        return _lookup.TryGetValue(effect, out var entry) ? entry.stackingMode : StatusStackingMode.StackBoth;
    }
    #endregion

    // ========================================================================
}

[System.Serializable]
public class StatusIconEntry
{
    public StatusEffects type;
    public StatusStackingMode stackingMode;
    public Sprite icon;
}
