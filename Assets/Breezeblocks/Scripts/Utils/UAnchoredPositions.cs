using UnityEngine;

public static class UAnchoredPositions
{
    public static Vector2 GetAnchoredPositionFromWorld(Transform worldTarget, RectTransform uiParent, Canvas canvas)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldTarget.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(uiParent, screenPoint, canvas.worldCamera, out Vector2 localPoint);
        return localPoint;
    }

    /// <summary>
    /// Converts a world‐space position into the local anchored position
    /// inside the given Canvas’s RectTransform.
    /// </summary>
    /// <param name="canvas">The target UI Canvas (Screen Space – Camera or Overlay).</param>
    /// <param name="worldPos">The world‐space position you want to map.</param>
    /// <returns>Anchored position in the Canvas’s RectTransform.</returns>
    public static Vector2 WorldToCanvasPosition(Canvas canvas, Vector3 worldPos)
    {
        // 1) World → screen space
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            canvas.worldCamera, worldPos);

        // 2) Screen → local point in the canvas RectTransform
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }
}
