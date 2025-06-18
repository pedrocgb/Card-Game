using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Breezeblocks.Managers;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
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

    [FoldoutGroup("Components/Actor Deck-Discard Displayer", expanded: true)]
    [SerializeField] private GameObject _actorDeckDiscardDisplayer = null;
    [FoldoutGroup("Components/Actor Deck-Discard Displayer", expanded: true)]
    [SerializeField] private TextMeshProUGUI _actorDeckDiscardTitleText = null;
    [FoldoutGroup("Components/Actor Deck-Discard Displayer", expanded: true)]
    [SerializeField] private Transform _actorDeckDiscardContainer = null;
    private List<CardSlideUI> _spawnedCards = new List<CardSlideUI>();
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

    public static void CardDisplayer(UEnums.CardDisplayerTypes Type, bool Activate)
    {
        if (Instance == null)
        {
            Debug.LogError("CardPreviewUI instance not found!");
            return;
        }
        Instance.cardDisplayer(Type, Activate);
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

    #region Deck and Discard Displayer
    private void cardDisplayer(UEnums.CardDisplayerTypes type, bool activate)
    {
        if (!activate)
        {
            // tear down previous
            foreach (var ui in _spawnedCards)
            {
                ui.transform.SetParent(null, false);
                ui.gameObject.SetActive(false);
            }
            _spawnedCards.Clear();

            _actorDeckDiscardDisplayer.SetActive(false);
            return;
        }

        _actorDeckDiscardDisplayer.SetActive(true);
      
        List<CardInstance> sorted = new List<CardInstance>();
        switch (type)
        {
            default:
            case UEnums.CardDisplayerTypes.Deck:
                _actorDeckDiscardTitleText.text = CombatManager.Instance.CurrentCombatent.ActorName + "'s Deck Pile";
                sorted = CombatManager.Instance.CurrentCombatent.Deck.CurrentDeck
                         .OrderBy(c => c.ActionCost)
                         .ToList();
                break;
            case UEnums.CardDisplayerTypes.Discard:
                _actorDeckDiscardTitleText.text = CombatManager.Instance.CurrentCombatent.ActorName + "'s Discard Pile";
                sorted = CombatManager.Instance.CurrentCombatent.Deck.DiscardPile
                         .OrderBy(c => c.ActionCost)
                         .ToList();
                break;
            case UEnums.CardDisplayerTypes.Consume:
                _actorDeckDiscardTitleText.text = CombatManager.Instance.CurrentCombatent.ActorName + "'s Consumed Cards Pile";
                sorted = CombatManager.Instance.CurrentCombatent.Deck.ConsumedPile
                         .OrderBy(c => c.ActionCost)
                         .ToList();
                break;
        }

        foreach (CardInstance card in sorted)
        {
            var ui = ObjectPooler
                .SpawnFromPool("Card Slide UI", _actorDeckDiscardContainer.position, Quaternion.identity)
                .GetComponent<CardSlideUI>();

            ui.transform.SetParent(_actorDeckDiscardContainer, worldPositionStays: false);
            ui.Initialize(card);
            _spawnedCards.Add(ui);
        }
    }
    #endregion

    // ========================================================================
}