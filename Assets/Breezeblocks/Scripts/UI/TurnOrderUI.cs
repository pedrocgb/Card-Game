using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using Breezeblocks.Managers;
using TMPro;
using UnityEngine.UI;

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
    private float _spacing = 10f;
    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField]
    private float _startAnchoredX = 50f;

    [FoldoutGroup("Settings/Name Animation", expanded: true)]
    [SerializeField]
    private float _nameFade = 0.4f;
    [FoldoutGroup("Settings/Name Animation", expanded: true)]
    [SerializeField]
    private float _letterDelay = 0.05f;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Transform _turnOrderContainer; // Parent for icon layout
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _actorNameText = null;
    private CanvasGroup _actorNameCanvasGroup = null;

    [FoldoutGroup("Components/Combat Panel", expanded: true)]
    [SerializeField]
    private Image _combatPanelImage = null;
    [FoldoutGroup("Components/Combat Panel", expanded: true)]
    [SerializeField]
    private Image _actorHandImage = null;
    [FoldoutGroup("Components/Combat Panel", expanded: true)]
    [SerializeField]
    private Sprite _playerCombatPanelSprite = null;
    [FoldoutGroup("Components/Combat Panel", expanded: true)]
    [SerializeField]
    private Sprite _enemyCombatPanelSprite = null;
    #endregion

    // ========================================================================

    private void Awake()
    {
        Instance = this;
        _actorNameCanvasGroup = _actorNameText.GetComponent<CanvasGroup>();
    }

    // ========================================================================

    #region Turn UI Methods
    public void UpdateUI(List<ActorManager> turnOrder)
    {
        foreach (var icon in _iconList)
            icon.gameObject.SetActive(false);
        _iconList.Clear();

        float startX = _startAnchoredX;

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
            AnimateName(_iconList[0].LinkedActor.ActorName);    
        }
    }

    public void AdvanceTurn()
    {
        if (_iconList.Count == 0) return;

        TurnIcon finished = _iconList[0];
        _iconList.RemoveAt(0);
        finished.FadeOutAndDestroy();

        float startX = _startAnchoredX;

        for (int i = 0; i < _iconList.Count; i++)
        {
            RectTransform rt = _iconList[i].GetComponent<RectTransform>();
            Vector2 targetSize = (i == 0) ? _expandedSize : _defaultSize;

            _iconList[i].AnimateResize(targetSize);
            rt.DOAnchorPos(new Vector2(startX, 0), _slideDuration).SetEase(Ease.OutCubic);

            startX += targetSize.x + _spacing;
        }

        if (_iconList.Count > 0)
        {
            AnimateName(_iconList[0].LinkedActor.ActorName);
        }
        else
        {
            AnimateName("");
        }
    }

    public void RemoveActorFromTurn(ActorManager actor)
    {
        // find the icon
        TurnIcon icon = _iconList.Find(i => i.LinkedActor == actor);
        if (icon == null) return;

        int idx = _iconList.IndexOf(icon);

        // if they're the front icon, do nothing – AdvanceTurn() will pop it
        if (idx == 0)
            return;

        // otherwise remove them immediately
        _iconList.RemoveAt(idx);
        icon.FadeOutAndDestroy();
        RebuildUI();
    }

    public void ClearTurnOrder()
    {
        foreach (var icon in _iconList)
        {
            icon.FadeOutAndDestroy();
        }
        _iconList.Clear();
        _actorNameText.text = "";
        _actorNameCanvasGroup.alpha = 0;
    }

    private void RebuildUI()
    {
        float startX = _startAnchoredX;

        for (int i = 0; i < _iconList.Count; i++)
        {
            RectTransform rt = _iconList[i].GetComponent<RectTransform>();
            Vector2 targetSize = (i == 0) ? _expandedSize : _defaultSize;

            _iconList[i].AnimateResize(targetSize);
            rt.DOAnchorPos(new Vector2(startX, 0), 0.3f).SetEase(Ease.OutCubic);

            startX += targetSize.x + _spacing;
        }

        if (_iconList.Count > 0)
            AnimateName(_iconList[0].LinkedActor.ActorName);
        else
            _actorNameText.text = "";
     
    }

    private void AnimateName(string newName)
    {
        // fade out, then type and fade back in
        _actorNameCanvasGroup.DOKill();
        _actorNameCanvasGroup.DOFade(0, _nameFade).OnComplete(() =>
        {
            _actorNameText.text = "";
            _actorNameCanvasGroup.alpha = 1;
            // typewriter effect
            DOTween.To(() => 0, i =>
            {
                int length = Mathf.Clamp(i, 0, newName.Length);
                _actorNameText.text = newName.Substring(0, length);
            }, newName.Length, newName.Length * _letterDelay);
        });
    }
    #endregion

    // ========================================================================

    public void ChangeCombatPanel(bool isPlayer)
    {
        if (_combatPanelImage == null || _actorHandImage == null) return;
        _combatPanelImage.sprite = isPlayer ? _playerCombatPanelSprite : _enemyCombatPanelSprite;
        _actorHandImage.sprite = isPlayer ? _playerCombatPanelSprite : _enemyCombatPanelSprite;
    }

    // ========================================================================
}
