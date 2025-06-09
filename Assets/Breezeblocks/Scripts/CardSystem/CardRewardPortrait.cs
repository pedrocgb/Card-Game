using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardRewardPortrait : MonoBehaviour
{
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Image _portraitImage = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _actorName = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _actorRace = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private TextMeshProUGUI _deckText = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private RectTransform _cardsContainer = null;
    public RectTransform CardsContainer => _cardsContainer;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField]
    private Button _skipRewardBtn = null;
    public Button SkipRewardButton => _skipRewardBtn;
    
    public ActorManager MyActor { get; private set; }

    public void Initialize(Sprite Portrait, string ActorName, string ActorRace, string ActorSpec, int DeckSize, ActorManager Actor)
    {
        MyActor = Actor;

        if (Actor.Stats.IsDead)
        {
            _actorName.text = $"{ActorName} (Dead)";
            _portraitImage.sprite = GameManager.DeadActorPortrait;
        }
        else
        {
            _actorName.text = ActorName;
            _portraitImage.sprite = Portrait;
        }
       
        _actorRace.text = $"{ActorRace} ({ActorSpec})";
        _deckText.text = $"Deck Size: ({DeckSize})";
    }
}
