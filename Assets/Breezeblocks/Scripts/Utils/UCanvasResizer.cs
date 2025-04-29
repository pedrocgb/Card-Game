using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class UCanvasResizer : MonoBehaviour
{
    private RectTransform _canvasRectTransform = null;
    private RectTransform _myRectTransform = null;

    [FoldoutGroup("Setting", expanded: true)]
    [SerializeField]
    private float _sim = 0f;
    private Vector2 _size;

    private void Start()
    {
        _canvasRectTransform = FindAnyObjectByType<Canvas>().GetComponent<RectTransform>();
        _myRectTransform = GetComponent<RectTransform>();

        float widthRatio = _canvasRectTransform.rect.width / Screen.width;
        float heightRatio = _canvasRectTransform.rect.height / Screen.height;

        float offsetTop = (Screen.safeArea.yMax - Screen.height) * heightRatio;
        float offsetBottom = Screen.safeArea.yMin * heightRatio;
        float offsetLeft = Screen.safeArea.xMin * widthRatio;
        float offsetRight = (Screen.safeArea.xMax - Screen.width) * widthRatio;

        _myRectTransform.offsetMax = new Vector2(offsetRight, offsetTop);
        _myRectTransform.offsetMin = new Vector2(offsetLeft, offsetBottom);
        CanvasScaler canvasScaler = _canvasRectTransform.GetComponent<CanvasScaler>();
        canvasScaler.referenceResolution = new Vector2(canvasScaler.referenceResolution.x,
            canvasScaler.referenceResolution.y + Mathf.Abs(offsetTop)+ Mathf.Abs(offsetBottom));
    }
}
