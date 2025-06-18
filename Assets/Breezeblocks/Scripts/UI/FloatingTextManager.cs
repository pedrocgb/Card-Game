using Breezeblocks.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using static UEnums;

public class FloatingTextManager : MonoBehaviour
{
    #region Variables and Properties
    private static FloatingTextManager Instance = null;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Canvas _worldCanvas = null;
    #endregion

    // ========================================================================

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // ========================================================================

    public static void SpawnText(Vector3 WorldPosition, string Text, HealthModColors DamageMod)
    {
        if (Instance == null)
        {
            Debug.LogError("FloatingTextManager instance is not set.");
            return;
        }

        Instance.floatingTextAnimation(WorldPosition, Text, DamageMod);
    }

    // ========================================================================

    private void floatingTextAnimation(Vector3 worldPosition, string text, HealthModColors damageMod)
    {
        FloatingText f = ObjectPooler.SpawnFromPool("Floating Damage Text", worldPosition, Quaternion.identity).GetComponent<FloatingText>();
        f.transform.SetParent(_worldCanvas.transform);
        f.transform.position = worldPosition;
        f.UpdateText(text, 0.6f, damageMod);
    }

    // ========================================================================
}
