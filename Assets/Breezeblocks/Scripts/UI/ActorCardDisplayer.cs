using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActorCardDisplayer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [BoxGroup("Settings")]
    [SerializeField] private UEnums.CardDisplayerTypes _displayerType;

    // ========================================================================

    #region Pointer Methods
    public void OnPointerEnter(PointerEventData eventData)
    {
        CardPreviewManager.CardDisplayer(_displayerType, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CardPreviewManager.CardDisplayer(_displayerType, false);
    }
    #endregion

    // ========================================================================
}
