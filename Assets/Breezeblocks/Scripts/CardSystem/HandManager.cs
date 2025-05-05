using Breezeblocks.Managers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DeckManager))] 
public class HandManager : MonoBehaviour
{
    #region Variables and Properties
    private DeckManager _deckManager = null;
    private List<CardData> _currentHand = new List<CardData>();

    // Card animations settings
    [FoldoutGroup("Cards Animations", expanded: true)]
    [SerializeField]
    private float _startingAnchoredX = -800f;
    [FoldoutGroup("Cards Animations", expanded: true)]
    [SerializeField]
    private float _anchoredOffSet = 260f;

    // Components
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private RectTransform _deckUi = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private RectTransform _cardGroup = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Canvas _mainCanvas = null;
    #endregion

    // ========================================================================

    #region Initialization
    private void Start()
    {
        _deckManager = GetComponent<DeckManager>();
    }
    #endregion

    // ========================================================================

    #region Draw Cards Methods
    /// <summary>
    /// Draw a number of cards from the deck, add it to current hand and animate them.
    /// </summary>
    /// <param name="Quantity"></param>
    public void DrawCards(int Quantity)
    {
        StartCoroutine(EnumeratorDrawCards(Quantity));
    }

    private CardUI DrawCard()
    {
        // Deck is empty, force shuffle.
        if (_deckManager.CurrentDeck.Count == 0)
        {
            Debug.LogWarning("Deck is empty!");
            _deckManager.Shuffle();
            return null;
        }

        // Create a new card instance and apply to hand.
        CardData drawnCard = _deckManager.CurrentDeck[0];
        _deckManager.CurrentDeck.RemoveAt(0);

        CardUI card = ObjectPooler.SpawnFromPool("Card", _cardGroup.transform.position, Quaternion.identity).GetComponent<CardUI>();
        card.Initialize(drawnCard);
        card.transform.SetParent(_cardGroup, false);

        _currentHand.Add(drawnCard);

        return card;
    }

    private IEnumerator EnumeratorDrawCards(int quantity)
    {
        // Get the desired position for the card to spawn and where the deck is located.
        Vector3 pos = new Vector3(_cardGroup.anchoredPosition.x + _startingAnchoredX, 0f, 0f);
        Vector3 deckPos = UAnchoredPositions.GetAnchoredPositionFromWorld(_deckUi, _cardGroup, _mainCanvas);

        // Loop through the number of cards to draw, waiting for the animation to finish before drawing the next one.
        for (int i = 0; i < quantity; i++)
        {
            if (i > 0)
                pos += new Vector3(_anchoredOffSet, 0f, 0f);

            DrawCard().Animations.PlayDrawAnimation(deckPos, pos);
            yield return new WaitForSeconds(0.5f);
        }
    }
    #endregion

    // ========================================================================
}
