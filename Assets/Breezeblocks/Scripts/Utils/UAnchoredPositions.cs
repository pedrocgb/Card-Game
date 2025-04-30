using UnityEngine;

public static class UAnchoredPositions
{
    public static Vector2 GetAnchoredPositionFromWorld(Transform worldTarget, RectTransform uiParent, Canvas canvas)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldTarget.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(uiParent, screenPoint, canvas.worldCamera, out Vector2 localPoint);
        return localPoint;
    }
}
