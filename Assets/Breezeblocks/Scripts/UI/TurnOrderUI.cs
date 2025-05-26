using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using Breezeblocks.Managers;

public class TurnOrderUI : MonoBehaviour
{
    #region Variables and Properties
    public static TurnOrderUI Instance;
    private List<TurnIcon> _iconList = new();

    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField] 
    private string _turnIconPrefab;
    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField] 
    private float _slideDuration = 0.3f;
    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField] 
    private Vector2 _defaultSize = new Vector2(50f, 50f);
    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField] 
    private Vector2 _expandedSize = new Vector2(100f, 100f);
    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField] 
    private float _spacing = 10f; // Optional: horizontal spacing between icons

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Transform _turnOrderContainer; // Parent for icon layout
    #endregion

    // ========================================================================

    private void Awake() => Instance = this;

    // ========================================================================

    #region Turn UI Methods
    public void UpdateUI(List<ActorManager> turnOrder)
    {
        foreach (var icon in _iconList)
            icon.gameObject.SetActive(false);
        _iconList.Clear();

        float startX = 0f;

        foreach (var actor in turnOrder)
        {
            GameObject iconGO = ObjectPooler.SpawnFromPool(_turnIconPrefab, _turnOrderContainer.position, Quaternion.identity);
            RectTransform rt = iconGO.GetComponent<RectTransform>();
            TurnIcon icon = iconGO.GetComponent<TurnIcon>();

            icon.Initialize(actor);
            rt.SetParent(_turnOrderContainer);
            rt.sizeDelta = _defaultSize;
            rt.anchoredPosition = new Vector2(startX, 0);

            _iconList.Add(icon);
            startX += _defaultSize.x + _spacing;
        }

        if (_iconList.Count > 0)
        {
            _iconList[0].Expand(_expandedSize);
        }
    }

    public void AdvanceTurn()
    {
        if (_iconList.Count == 0) return;

        TurnIcon finished = _iconList[0];
        _iconList.RemoveAt(0);
        finished.FadeOutAndDestroy();

        float startX = 0f;

        for (int i = 0; i < _iconList.Count; i++)
        {
            RectTransform rt = _iconList[i].GetComponent<RectTransform>();
            Vector2 targetSize = (i == 0) ? _expandedSize : _defaultSize;

            _iconList[i].AnimateResize(targetSize);
            rt.DOAnchorPos(new Vector2(startX, 0), _slideDuration).SetEase(Ease.OutCubic);

            startX += targetSize.x + _spacing;
        }
    }

    public void RemoveActorFromTurn(ActorManager actor)
    {
        TurnIcon icon = _iconList.Find(i => i.LinkedActor == actor);
        if (icon != null)
        {
            _iconList.Remove(icon);
            icon.FadeOutAndDestroy();
            RebuildUI();
        }
    }

    private void RebuildUI()
    {
        float startX = 0f;

        for (int i = 0; i < _iconList.Count; i++)
        {
            RectTransform rt = _iconList[i].GetComponent<RectTransform>();
            Vector2 targetSize = (i == 0) ? _expandedSize : _defaultSize;

            _iconList[i].AnimateResize(targetSize);
            rt.DOAnchorPos(new Vector2(startX, 0), 0.3f).SetEase(Ease.OutCubic);

            startX += targetSize.x + _spacing;
        }
    }
    #endregion

    // ========================================================================
}
