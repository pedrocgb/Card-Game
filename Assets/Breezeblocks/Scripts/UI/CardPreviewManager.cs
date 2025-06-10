using System.Collections;
using Breezeblocks.Managers;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class CardPreviewManager : MonoBehaviour
{
    #region Variables and Properties
    public static CardPreviewManager Instance;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] 
    private Canvas _mainCanvas;             // your UI canvas
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] 
    private RectTransform _previewParent;       // empty RectTransform under Canvas

    [FoldoutGroup("Animation Settings", expanded: true)]
    [SerializeField] 
    private float _moveDuration = 0.5f;
    [FoldoutGroup("Animation Settings", expanded: true)]
    [SerializeField] 
    private float _flipDuration = 0.3f;
    [FoldoutGroup("Animation Settings", expanded: true)]
    [SerializeField] 
    private float _holdDuration = 0.6f;
    [FoldoutGroup("Animation Settings", expanded: true)]
    [SerializeField] 
    private float _fadeDuration = 0.3f;
    #endregion

    // ========================================================================

    #region Initialization
    private void Awake() => Instance = this;
    #endregion

    // ========================================================================

    #region Static Methods
    public static void ShowEnemyCard(EnemyActor enemy, CardInstance cardData)
    {
        if (Instance == null)
        {
            Debug.LogError("CardPreviewUI instance not found!");
            return;
        }
        Instance.StartCoroutine(Instance.showEnemyCard(enemy, cardData));
    }
    #endregion

    // ========================================================================

    /// <summary>
    /// Spawns a CardUI at the enemy's screen position, flips it face-up, holds, then fades out.
    /// </summary>
    private IEnumerator showEnemyCard(EnemyActor enemy, CardInstance cardData)
    {
        // 1) Spawn & initialize
        GameObject go = ObjectPooler.SpawnFromPool("Card Preview", Vector3.zero, Quaternion.identity);
        CardPreview cardUI = go.GetComponent<CardPreview>();
        cardUI.Initialize(cardData);
        cardUI.transform.SetParent(_previewParent, false);

        // 2) Place at enemy's world→UI position
        RectTransform rt = cardUI.GetComponent<RectTransform>();
        Vector3 worldPos = enemy.transform.position + Vector3.up * 1.2f;
        Vector2 startAnchored = UAnchoredPositions.GetAnchoredPositionFromWorld(enemy.transform, _previewParent, _mainCanvas);
        rt.anchoredPosition = startAnchored;

        // 3) Start flipped (face-down). Assuming your backside is default or you swap sprite on flip
        rt.localRotation = Quaternion.Euler(0, 180, 0);

        // 4) Calculate center-screen target
        Vector2 center = Vector2.zero; // anchored center of the preview parent

        // 5) Build the sequence
        CanvasGroup cg = rt.GetComponent<CanvasGroup>() ?? rt.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1;

        var seq = DOTween.Sequence();

        // 5a) Flip face-up
        seq.Append(rt.DORotateQuaternion(Quaternion.identity, _flipDuration).SetEase(Ease.OutBack));

        // 5b) Move to center
        seq.Join(rt.DOAnchorPos(center, _moveDuration).SetEase(Ease.OutCubic));

        // 5c) Hold
        seq.AppendInterval(_holdDuration);

        // 5d) Fade out
        seq.Append(cg.DOFade(0, _fadeDuration));

        // 5e) Cleanup
        seq.OnComplete(() =>
        {
            go.SetActive(false);
        });

        yield return seq.WaitForCompletion();
    }

    // ========================================================================
}